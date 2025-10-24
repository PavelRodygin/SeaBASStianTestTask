using System;
using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Snapping.Abstractions;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Snapping
{
    public class NullSnapping : IScrollSnapping
    {
        public static readonly NullSnapping Instance = new NullSnapping();
        
        public event Action BeforeSnapping;
        public event Action AfterSnapping;
        
        private NullSnapping() { }
        
        public UniTask SnapToNearest(CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}