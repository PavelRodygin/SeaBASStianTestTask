using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PooledScroll.AdditionalFunctionality.Move
{
    public class ScrollMoverBehaviour : BaseScrollMoverBehaviour
    {
        private IMovableScroll _scroll;
        private bool _isStopped;

        private void Awake()
        {
            _scroll = GetComponent<IMovableScroll>();
        }

        public override void StopMove()
        {
            _isStopped = true;
        }

        public override UniTask MoveTo(float point, float time, CancellationToken cancellationToken)
        {
            _isStopped = false;
            return MoveToPoint(point, time, cancellationToken);
        }

        private async UniTask MoveToPoint(float point, float time, CancellationToken cancellationToken)
        {
            var tweenTimeLeft = 0f;
            var startPosition = _scroll.AxisPosition;

            while (tweenTimeLeft < time)
            {
                if (_isStopped)
                    break;

                var newScrollPosition = Mathf.Lerp(startPosition, point, tweenTimeLeft / time);
                _scroll.UpdateAxisPosition(newScrollPosition);

                tweenTimeLeft += Time.unscaledDeltaTime;

                await UniTask.NextFrame(cancellationToken);
            }

            _scroll.UpdateAxisPosition(point);
        }
    }
}