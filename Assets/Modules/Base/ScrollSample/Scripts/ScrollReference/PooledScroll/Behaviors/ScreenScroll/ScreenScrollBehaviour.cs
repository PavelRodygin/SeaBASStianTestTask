using System;
using System.Threading;
using Core.PooledScroll.Behaviors.BaseLogic;
using Core.PooledScroll.Behaviors.ScreenScroll.Callbacks;
using Core.PooledScroll.Events;
using Core.PooledScroll.Pool;
using Core.Pooling.Base;
using Core.Pooling.Base.Abstractions;
using Cysharp.Threading.Tasks;
using UI.Controls;
using UnityEngine;

namespace Core.PooledScroll.Behaviors.ScreenScroll
{
    [RequireComponent(typeof(RectTransform))]
    public class ScreenScrollBehaviour : ScrollBehaviour, IPoolListener<ObjectHolder>
    {
        public override event Action ContentCalculated;
        public override event Action<ScrollingEventArgs> ScrollStarted;
        public override event Action<ScrollingEventArgs> ScrollEnded;
        public override event Action<ScrollingEventArgs> ScrollRangeChanged;
        public override int ActiveObjects => 1;
        
        private const string ContainerName = "ScreensContainer";

        private IScrollCallbacks _scrollCallbacks;
        private IScrollObjectPool _objectPool;
        private IObjectPool<ObjectHolder> _holderPool;
        
        private ObjectHolder _activeHolder;
        private Transform _contentContainer;
        private int _maxObjectsCount;

        public override void Initialize(int count, IScrollObjectPool objectPool, int startIndex = 0)
        {
            _objectPool = objectPool;
            _maxObjectsCount = count;
            
            _scrollCallbacks = GetComponent<IScrollCallbacks>() ?? NullScrollCallbacks.Instance;
            _holderPool = new ObjectPool<ObjectHolder>(this);
            
            _contentContainer = CreateContentContainer();
            _activeHolder = UpdateHolderData(startIndex);
            
            ContentCalculated?.Invoke();
        }

        public override void Dispose()
        {
            _objectPool.Dispose();
            _holderPool.Dispose();
        }

        public ObjectHolder OnCreate()
        {
            var holder = new ObjectHolder();
            OnGet(holder);
            
            return holder;
        }

        public void OnGet(ObjectHolder holder)
        {
            var poolingObject = _objectPool.Get();
            holder.Object = poolingObject is IScrollableObject scrollableObject
                ? scrollableObject
                : poolingObject;
            
            holder.Transform.SetParent(_contentContainer.transform, false);
        }

        public void OnRelease(ObjectHolder holder)
        {
            _objectPool.Release(holder.Object);
        }

        public void OnDispose(ObjectHolder holder) {}
        
        public override UniTask Show(CancellationToken cancellationToken)
        {
            return _activeHolder.Object is IScrollableObject scenarioObject
                ? _scrollCallbacks.OnOpen(scenarioObject, cancellationToken)
                : base.Show(cancellationToken);
        }
        
        public override UniTask ScrollNext(CancellationToken cancellationToken)
        {
            return ProceedChanging(_activeHolder.Index + 1, cancellationToken);
        }

        public override UniTask ScrollBack(CancellationToken cancellationToken)
        {
            return ProceedChanging(_activeHolder.Index - 1, cancellationToken);
        }
        
        public override UniTask ScrollTo(int index, bool immediately, CancellationToken cancellationToken)
        {
            return ProceedChanging(index, cancellationToken);
        }

        public override int CurrentStartIndex => _activeHolder.Index;
        public override int CurrentEndIndex => _activeHolder.Index;

        private Transform CreateContentContainer()
        {
            var container = new GameObject(ContainerName, typeof(RectTransform));
            var containerRect = (RectTransform)container.transform;
            
            containerRect.transform.SetParent(transform, false);
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = Vector2.zero;

            return containerRect;
        }

        private async UniTask ProceedChanging(int index, CancellationToken cancellationToken)
        {
            var isShort = index < 0 || index >= _maxObjectsCount;
            var isToNext = index > _activeHolder.Index;
            
            await PlayScenario(index, isToNext, isShort, cancellationToken);
        }

        private async UniTask PlayScenario(int index, bool isToNext, bool isShort, CancellationToken cancellationToken)
        {
            if (isShort)
            {
                if (_activeHolder.Object is IScrollableObject scrollableObject)
                    await _scrollCallbacks.OnCantMove(isToNext, scrollableObject, cancellationToken);
            }
            else
            {
                ScrollStarted?.Invoke(new ScrollingEventArgs(_activeHolder.Index, _activeHolder.Index));

                var nextViewHolder = UpdateHolderData(index);
                if (_activeHolder.Object is IScrollableObject scrollableObject && nextViewHolder.Object is IScrollableObject nextScrollableObject)
                    await _scrollCallbacks.OnMove(isToNext, scrollableObject, nextScrollableObject, cancellationToken);
                _holderPool.Release(_activeHolder);
                _activeHolder = nextViewHolder;

                ScrollRangeChanged?.Invoke(new ScrollingEventArgs(_activeHolder.Index, _activeHolder.Index));
                ScrollEnded?.Invoke(new ScrollingEventArgs(_activeHolder.Index, _activeHolder.Index));
            }
        }

        private ObjectHolder UpdateHolderData(int index)
        {
            var holder = _holderPool.Get();
            holder.Object.Transform.SetSiblingIndex(0);
            
            holder.Index = index;
            _objectPool.ReInitialize(holder.Index, holder.Object);
            
            return holder;
        }
    }
}