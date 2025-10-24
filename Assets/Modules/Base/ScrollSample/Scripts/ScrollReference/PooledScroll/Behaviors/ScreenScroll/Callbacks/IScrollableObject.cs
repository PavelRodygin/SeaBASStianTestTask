using System.Threading;
using Core.Pooling.GameObjectPool;
using Cysharp.Threading.Tasks;
using UI.Controls;

namespace Core.PooledScroll.Behaviors.ScreenScroll.Callbacks
{
    public interface IScrollableObject : IPoolingGameObject
    {
        UniTask Play(string key, CancellationToken cancellationToken);
    }
}
