using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Move
{
    public class NullScrollMover : IScrollMover
    {
        public static readonly NullScrollMover Instance = new NullScrollMover();
        
        private NullScrollMover() { }

        public void StopMove() { }
        public UniTask MoveTo(float point, float time, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}