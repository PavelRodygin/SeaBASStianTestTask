using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.Behaviors.ScreenScroll.Callbacks
{
    public class NullScrollCallbacks : IScrollCallbacks
    {
        public static readonly NullScrollCallbacks Instance = new NullScrollCallbacks();
        
        private NullScrollCallbacks () { }

        public UniTask OnOpen(IScrollableObject scrollableObject, CancellationToken cancellationToken) => UniTask.CompletedTask;
        public UniTask OnMove(bool toNext, IScrollableObject activeScrollableObject, IScrollableObject nextScrollableObject,
            CancellationToken cancellationToken) => UniTask.CompletedTask;
        public UniTask OnCantMove(bool toNext, IScrollableObject activeScrollableObject, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
