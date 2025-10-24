using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions;
using Cysharp.Threading.Tasks;

namespace Core.PooledScroll.AdditionalFunctionality.Navigation
{
    public class NullNavigation : IScrollNavigation
    {
        public static readonly NullNavigation Instance = new NullNavigation();
        
        private NullNavigation() { }
        
        public UniTask Next(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public UniTask Back(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public UniTask To(int index, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
