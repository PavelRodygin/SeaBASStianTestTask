using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.Behaviors.ScreenScroll.Callbacks
{
    public interface IScrollCallbacks
    {
        UniTask OnOpen(IScrollableObject scrollableObject, CancellationToken cancellationToken);
        UniTask OnMove(bool toNext, IScrollableObject activeScrollableObject, IScrollableObject nextScrollableObject,
            CancellationToken cancellationToken);
        UniTask OnCantMove(bool toNext, IScrollableObject activeScrollableObject, CancellationToken cancellationToken);
    }
}
