using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Move.Abstractions
{
    public interface IScrollMover
    {
        void StopMove();
        UniTask MoveTo(float point, float time, CancellationToken cancellationToken);
    }
}
