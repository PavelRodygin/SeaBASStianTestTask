using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PooledScroll.AdditionalFunctionality.Navigation
{
    public class ScrollNavigationBehaviour : BaseScrollNavigationBehaviour
    {
        [SerializeField]
        private BaseScrollMoverBehaviour _mover;
        [SerializeField]
        private float _movingTime;

        private const int Step = 2;
        private INavigationScroll _scroll;

        private void Awake()
        {
            _scroll = GetComponent<INavigationScroll>();
        }

        public override UniTask Next(CancellationToken cancellationToken)
        {
            var endPosition = _scroll.GetScrollEndPosition();
            var indexForEndPosition = _scroll.GetIndexAtPosition(endPosition);
            var nextIndex = indexForEndPosition + Step;

            if (nextIndex >= _scroll.ObjectsCount)
                return UniTask.CompletedTask;
            
            var positionForNextIndex = _scroll.GetPositionByIndex(nextIndex);
            
            return _mover.MoveTo(positionForNextIndex, _movingTime, cancellationToken);
        }

        public override UniTask Back(CancellationToken cancellationToken)
        {
            var startPosition = _scroll.AxisPosition;
            var indexForStartPosition = _scroll.GetIndexAtPosition(startPosition);
            var nextIndex = indexForStartPosition - Step;

            if (nextIndex < 0)
                return UniTask.CompletedTask;

            var positionForNextIndex = _scroll.GetPositionByIndex(nextIndex);
            
            return _mover.MoveTo(positionForNextIndex, _movingTime, cancellationToken);
        }
    }
}