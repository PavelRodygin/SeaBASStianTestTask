using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PooledScroll.AdditionalFunctionality.Move.Abstractions
{
    public abstract class BaseScrollMoverBehaviour : MonoBehaviour, IScrollMover
    {
        public abstract void StopMove();
        public abstract UniTask MoveTo(float point, float time, CancellationToken cancellationToken);
    }
}
