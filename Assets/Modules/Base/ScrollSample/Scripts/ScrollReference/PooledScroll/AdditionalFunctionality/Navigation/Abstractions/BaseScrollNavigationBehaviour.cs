using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions
{
    public abstract class BaseScrollNavigationBehaviour : MonoBehaviour, IScrollNavigation
    {
        public abstract UniTask Next(CancellationToken cancellationToken);
        public abstract UniTask Back(CancellationToken cancellationToken);
        public virtual UniTask To(int index, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
