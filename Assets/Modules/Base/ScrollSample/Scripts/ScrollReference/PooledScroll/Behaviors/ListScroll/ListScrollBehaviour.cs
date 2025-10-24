using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Core.PooledScroll.AdditionalFunctionality.Navigation;
using Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions;
using Core.PooledScroll.AdditionalFunctionality.Snapping;
using Core.PooledScroll.AdditionalFunctionality.Snapping.Abstractions;
using Core.PooledScroll.Behaviors.BaseLogic;
using Core.PooledScroll.Events;
using Core.PooledScroll.Extensions;
using Core.PooledScroll.Pool;
using Core.Pooling.Base;
using Core.Pooling.Base.Abstractions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.PooledScroll.Behaviors.ListScroll
{
    [RequireComponent(typeof(ScrollRect))]
    public class ListScrollBehaviour : ScrollBehaviour, ISnappingScroll, INavigationScroll, IMovableScroll,
        IPoolListener<ObjectHolder>, IBeginDragHandler
    {
        [SerializeField] private HorizontalOrVerticalLayoutGroup _contentGroup;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Vector2 _objectSize;

        public override event Action ContentCalculated;
        public override event Action<ScrollingEventArgs> ScrollStarted;
        public override event Action<ScrollingEventArgs> ScrollEnded;
        public override event Action<ScrollingEventArgs> ScrollRangeChanged;
        public override int ActiveObjects => _activeHolders.Count;
        
        public int ObjectsCount => _offsetsInfos.Count;
        public float AxisPosition => _scrollPosition;
        
        private const string FirstPlaceholderName = "FirstPlaceholder";
        private const string LastPlaceholderName = "LastPlaceholder";

        private IScrollNavigation _navigationInternal;
        private IScrollSnapping _snappingInternal;
        private IScrollObjectPool _objectPool;
        private IObjectPool<ObjectHolder> _holderPool;
        private RectTransform _scrollRectTransform;
        
        private LayoutElement _firstPlaceholder;
        private LayoutElement _lastPlaceholder;

        private RectTransform ScrollRectTransform
        {
            get
            {
                if (_scrollRectTransform == null)
                    _scrollRectTransform = (RectTransform)_scrollRect.transform;
                
                return _scrollRectTransform;
            }
        }
        private IScrollSnapping Snapping => _snappingInternal ??= GetComponent<IScrollSnapping>() ?? NullSnapping.Instance;
        private IScrollNavigation Navigation => _navigationInternal ??= GetComponent<IScrollNavigation>() ?? NullNavigation.Instance;

        private int _currentStartIndex;
        private int _currentEndIndex;
        private float _scrollPosition;
        private bool _inertial;
        private bool _velocityChanged;
        
        private readonly List<ObjectHolder> _activeHolders = new List<ObjectHolder>();
        private readonly List<OffsetInfo> _offsetsInfos = new List<OffsetInfo>();
        
        private async void Start()
        {
            await UniTask.DelayFrame(1);
            if (_currentStartIndex != _currentEndIndex)
                return;
            ScrollValueChanged(Vector2.zero);
            ContentCalculated?.Invoke();
        }

        private void Update()
        {
            if (!_velocityChanged)
                return;
            if (_scrollRect.GetAxisVelocity() == 0)
            {
                _velocityChanged = false;
                ScrollEnded?.Invoke(new ScrollingEventArgs(_currentStartIndex, _currentEndIndex));
            }
        }

        public override void Initialize(int count, IScrollObjectPool objectPool, int startIndex = 0)
        {
            _objectPool = objectPool;
            _holderPool = new ObjectPool<ObjectHolder>(this);
            InitializeAdditionalFunctionality();
            _firstPlaceholder = CreatePlaceholder(FirstPlaceholderName);
            _lastPlaceholder = CreatePlaceholder(LastPlaceholderName);
            CalculateObjectsSizes(count, _objectSize);
            UpdateVisibleObjects();
            _scrollRect.onValueChanged.AddListener(ScrollValueChanged);
            Snapping.SnapToNearest(CancellationToken.None).Forget();
            if (_currentStartIndex != _currentEndIndex)
                ContentCalculated?.Invoke();
        }

        public override int CurrentStartIndex => _currentStartIndex;

        public override int CurrentEndIndex => _currentEndIndex;

        public override void Reinitialize(int count)
        {
            foreach (var holder in _activeHolders)
                _holderPool.Release(holder);
            _activeHolders.Clear();
            _currentStartIndex = 0;
            _currentEndIndex = 0;
            _offsetsInfos.Clear();
            if (_scrollRect.vertical)
                _scrollRect.verticalNormalizedPosition = 1f;
            else
                _scrollRect.horizontalNormalizedPosition = 0f;
            CalculateObjectsSizes(count, _objectSize);
            UpdateVisibleObjects();
            ContentCalculated?.Invoke();
            ScrollValueChanged(Vector2.zero);
        }

        public override void Dispose()
        {
            Snapping.AfterSnapping -= OnAfterSnapping;
            Snapping.BeforeSnapping -= OnBeforeSnapping;
            _scrollRect.onValueChanged.RemoveListener(ScrollValueChanged);
            _holderPool.Dispose();
            _objectPool.Dispose();
            _activeHolders.Clear();
            _offsetsInfos.Clear();
        }
        
        public override int GetMaxVisibleObjects()
        {
            var viewportSize = _scrollRect.vertical ? _scrollRect.viewport.rect.height : _scrollRect.viewport.rect.width;
            var padding = _scrollRect.vertical 
                ? _contentGroup.padding.top + _contentGroup.padding.bottom 
                : _contentGroup.padding.left + _contentGroup.padding.right;
            viewportSize -= padding;
            var totalSize = 0f;
            var maxVisibleObjects = 0;
            var objectSizeWithSpacing = _scrollRect.vertical ? _objectSize.y + _contentGroup.spacing : _objectSize.x + _contentGroup.spacing;

            while (totalSize + objectSizeWithSpacing <= viewportSize)
            {
                totalSize += objectSizeWithSpacing;
                maxVisibleObjects++;
            }
            
            if (viewportSize - totalSize > 0)
                maxVisibleObjects++;

            return maxVisibleObjects;
        }

        public override UniTask ScrollNext(CancellationToken cancellationToken)
            => Navigation.Next(cancellationToken);

        public override UniTask ScrollBack(CancellationToken cancellationToken)
            => Navigation.Back(cancellationToken);

        public void UpdateAxisPosition(float position)
        {
            _scrollPosition = position;
            if (_scrollRect.vertical)
                _scrollRect.verticalNormalizedPosition  = 1f - AxisPosition / GetScrollSize();
            else
                _scrollRect.horizontalNormalizedPosition  = AxisPosition / GetScrollSize();
        }

        public float GetScrollEndPosition()
        {
            var rect = ScrollRectTransform.rect;
            return _scrollPosition + (_scrollRect.vertical ? rect.height : rect.width);
        }
        
        public float GetPositionByIndex(int index)
        {
            var elementInfo = _offsetsInfos[index - 1];
            var rect = _scrollRect.viewport.rect;
            var viewportAxisSize = _scrollRect.vertical ? rect.height : rect.width;
            var viewportAxisHalfSize = viewportAxisSize / 2;
            var padding = _scrollRect.vertical ? _contentGroup.padding.top : _contentGroup.padding.left;
            var elementHalfSizeWithSpacing = (_offsetsInfos[index].Size + _contentGroup.spacing) / 2;
            var pos = elementInfo.Offset - viewportAxisHalfSize + padding + elementHalfSizeWithSpacing;
            pos = Mathf.Clamp(pos, 0, GetScrollSize());
            return pos;
        }
        
        public int GetIndexAtPosition(float position) =>
            GetIndexAtPosition(position, 0, _offsetsInfos.Count - 1);

        public ObjectHolder OnCreate()
        {
            var holder = new ObjectHolder();
            OnGet(holder);
            return holder;
        }

        public void OnGet(ObjectHolder holder)
        {
            var poolingObject = _objectPool.Get();
            holder.Object = poolingObject;
            holder.Transform.SetParent(_contentGroup.transform, false);
            holder.Transform.gameObject.SetActive(true);
        }

        public void OnRelease(ObjectHolder holder) => 
            _objectPool.Release(holder.Object);

        public void OnDispose(ObjectHolder holder) {}

        public void OnBeginDrag(PointerEventData eventData) => 
            ScrollStarted?.Invoke(new ScrollingEventArgs(_currentStartIndex, _currentEndIndex));

        public override UniTask ScrollTo(int index, bool immediately = false, CancellationToken cancellationToken = default)
        {
            UpdateAxisPosition(GetPositionByIndex(index));
            return UniTask.CompletedTask;
        }

        private void InitializeAdditionalFunctionality()
        {
            Snapping.AfterSnapping += OnAfterSnapping;
            Snapping.BeforeSnapping += OnBeforeSnapping;
        }
        
        private LayoutElement CreatePlaceholder(string placeholderName)
        {
            var instance = new GameObject(placeholderName, typeof(RectTransform), typeof(LayoutElement));
            instance.transform.SetParent(_contentGroup.transform, false);
            return instance.GetComponent<LayoutElement>();
        }

        private void CalculateObjectsSizes(int count, Vector2 elementSize)
        {
            var offset = 0f;
            for (var i = 0; i < count; ++i)
            {
                var size = _scrollRect.vertical ? elementSize.y : elementSize.x;
                var sizeWithSpacing = size + (i == 0 ? 0 : _contentGroup.spacing);
                offset += sizeWithSpacing;
                var info = new OffsetInfo(sizeWithSpacing, offset);
                _offsetsInfos.Add(info);
            }
        }
        
        private void UpdateVisibleObjects()
        {
            CalculateVisibleObjectsRange(out var startIndex, out var endIndex);
            var i = 0;
            var remainingObjectsIndices = new List<int>();
            while (i < _activeHolders.Count)
            {
                if (_activeHolders[i].Index < startIndex || _activeHolders[i].Index > endIndex)
                {
                    var holder = _activeHolders[i];
                    _activeHolders.Remove(holder);
                    _holderPool.Release(holder);
                    holder.Index = 0;
                }
                else
                {
                    remainingObjectsIndices.Add(_activeHolders[i].Index);
                    i++;
                }
            }
            if (remainingObjectsIndices.Count == 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                    AddObject(i, InsertType.Last);
            }
            else
            {
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingObjectsIndices.First())
                        AddObject(i, InsertType.First);
                }
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingObjectsIndices.Last())
                        AddObject(i, InsertType.Last);
                }
            }
            
            _currentStartIndex = startIndex;
            _currentEndIndex = endIndex;
            UpdatePlaceholders();
        }
        
        private void CalculateVisibleObjectsRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;
            startIndex = GetIndexAtPosition(_scrollPosition);
            endIndex = GetIndexAtPosition(GetScrollEndPosition());
        }
        
        private void AddObject(int index, InsertType insertType)
        {
            if (_offsetsInfos.Count == 0)
                return;
            var holder = _holderPool.Get();
            holder.Index = index;
            switch (insertType)
            {
                case InsertType.First:
                    _activeHolders.Insert(0, holder);
                    holder.Transform.SetSiblingIndex(1);
                    break;
                case InsertType.Last:
                    _activeHolders.Add(holder);
                    holder.Transform.SetSiblingIndex(_contentGroup.transform.childCount - 2);
                    break;
            }
            _objectPool.ReInitialize(holder.Index, holder.Object);
        }
        
        private void UpdatePlaceholders()
        {
            if (_offsetsInfos.Count == 0) return;
            var firstSize = _offsetsInfos[_currentStartIndex].Offset - _offsetsInfos[_currentStartIndex].Size;
            var lastSize = _offsetsInfos.Last().Offset - _offsetsInfos[_currentEndIndex].Offset;
            if (_scrollRect.vertical)
            {
                _firstPlaceholder.minHeight = firstSize;
                _firstPlaceholder.gameObject.SetActive(_firstPlaceholder.minHeight > 0);
                _lastPlaceholder.minHeight = lastSize;
                _lastPlaceholder.gameObject.SetActive(_lastPlaceholder.minHeight > 0);
            }
            else
            {
                _firstPlaceholder.minWidth = firstSize;
                _firstPlaceholder.gameObject.SetActive(_firstPlaceholder.minWidth > 0);
                _lastPlaceholder.minWidth = lastSize;
                _lastPlaceholder.gameObject.SetActive(_lastPlaceholder.minWidth > 0);
            }
        }
        
        private int GetIndexAtPosition(float position, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
                return startIndex;
            var middleIndex = (startIndex + endIndex) / 2;
            var padding = _scrollRect.vertical ? _contentGroup.padding.top : _contentGroup.padding.left;
            return _offsetsInfos[middleIndex].Offset + padding >= position
                ? GetIndexAtPosition(position, startIndex, middleIndex)
                : GetIndexAtPosition(position, middleIndex + 1, endIndex);
        }

        private float GetScrollSize()
        {
            return _scrollRect.vertical
                ? Mathf.Max(_scrollRect.content.rect.height - ScrollRectTransform.rect.height, 0)
                : Mathf.Max(_scrollRect.content.rect.width - ScrollRectTransform.rect.width, 0);
        }

        private void ScrollValueChanged(Vector2 value)
        {
            _velocityChanged = true;
            _scrollPosition = _scrollRect.vertical
                ? (1f - value.y) * GetScrollSize()
                : value.x * GetScrollSize();
            CalculateVisibleObjectsRange(out var startIndex, out var endIndex);
            Snapping.SnapToNearest(CancellationToken.None).Forget();
            if (startIndex == _currentStartIndex && endIndex == _currentEndIndex)
                return;
            UpdateVisibleObjects();
            ScrollRangeChanged?.Invoke(new ScrollingEventArgs(_currentStartIndex, _currentEndIndex));
        }

        private void OnBeforeSnapping()
        {
            _inertial = _scrollRect.inertia;
            _scrollRect.inertia = false;
        }

        private void OnAfterSnapping() => _scrollRect.inertia = _inertial;

        private struct OffsetInfo
        {
            public float Size { get; }
            public float Offset { get; }

            public OffsetInfo(float size, float offset)
            {
                Size = size;
                Offset = offset;
            }
        }
    }
}