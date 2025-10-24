using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions
{
    public interface IScrollNavigation
    {
        UniTask Next(CancellationToken cancellationToken);
        UniTask Back(CancellationToken cancellationToken);
        UniTask To(int index, CancellationToken cancellationToken);
    }
}
