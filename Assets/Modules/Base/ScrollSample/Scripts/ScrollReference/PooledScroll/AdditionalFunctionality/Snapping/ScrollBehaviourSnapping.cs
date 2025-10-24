using System;
using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Core.PooledScroll.AdditionalFunctionality.Snapping.Abstractions;
using Core.PooledScroll.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.PooledScroll.AdditionalFunctionality.Snapping
{
    public class ScrollBehaviourSnapping : BaseScrollSnappingBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private BaseScrollMoverBehaviour _mover;
        [SerializeField]
        private float _snapVelocityThreshold;
        [SerializeField]
        private float _snappingTime;
        [SerializeField]
        private float _minOffsetForNextStep;

        public override event Action BeforeSnapping;
        public override event Action AfterSnapping;
        
        private ISnappingScroll _scrollBehaviour;
        private bool _isDrag;
        private bool _isSnapping;

        private void Awake()
        {
            _scrollBehaviour = GetComponent<ISnappingScroll>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;

            if (_isSnapping)
            {
                _mover.StopMove();
                _isSnapping = false;
                
                AfterSnapping?.Invoke();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;

            if (_isSnapping || !CanSnapping())
                return;

            SnapToNearest(CancellationToken.None).Forget();
        }

        public override async UniTask SnapToNearest(CancellationToken cancellationToken)
        {
            if (!CanSnapping() || !TryGetAxisPosition(out  var position))
                return;

            BeforeSnapping?.Invoke();
            
            _scrollRect.SetAxisVelocity(0);
            _isSnapping = true;
            
            await _mover.MoveTo(position, _snappingTime, cancellationToken);
            
            _isSnapping = false;
            
            AfterSnapping?.Invoke();
        }

        private bool CanSnapping()
        {
            if (!_isDrag && !_isSnapping)
            {
                if (Mathf.Abs(_scrollRect.GetAxisVelocity()) <= _snapVelocityThreshold && _scrollRect.GetAxisVelocity() != 0)
                    return true;
            }

            return false;
        }
        
        private bool TryGetAxisPosition(out float position)
        {
            position = 0;
            
            var snapPosition = _scrollBehaviour.AxisPosition + (_scrollBehaviour.GetScrollEndPosition() - _scrollBehaviour.AxisPosition) / 2;
            var snapViewIndex = _scrollBehaviour.GetIndexAtPosition(snapPosition);
            
            if (snapViewIndex == 0)
                return false;

            position = _scrollBehaviour.GetPositionByIndex(snapViewIndex);
            position = PositionAdjustment(position, snapViewIndex, _scrollRect.GetAxisVelocity());

            return true;
        }
        
        private float PositionAdjustment(float currentPosition, int currentIndex, float savedVelocity)
        {
            if (_scrollBehaviour.AxisPosition - _minOffsetForNextStep > currentPosition && savedVelocity < 0)
            {
                var newIndex = currentIndex + 1;
                if (newIndex < _scrollBehaviour.ObjectsCount)
                    return _scrollBehaviour.GetPositionByIndex(newIndex);
            }
            if (_scrollBehaviour.AxisPosition + _minOffsetForNextStep < currentPosition && savedVelocity > 0)
            {
                var newIndex = currentIndex - 1;
                if (newIndex > 0)
                    return _scrollBehaviour.GetPositionByIndex(newIndex);
            }

            return currentPosition;
        }
    }
}