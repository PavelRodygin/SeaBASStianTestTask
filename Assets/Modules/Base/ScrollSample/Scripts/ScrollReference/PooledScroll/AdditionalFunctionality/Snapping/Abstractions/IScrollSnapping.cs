using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Snapping.Abstractions
{
    public interface IScrollSnapping
    {
        event Action BeforeSnapping;
        event Action AfterSnapping;
        
        UniTask SnapToNearest(CancellationToken cancellationToken);
    }
}