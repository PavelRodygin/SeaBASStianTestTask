using Core.Pooling.GameObjectPool;
using Core.Pooling.ReInitPool;
using UI.Controls;

namespace Core.PooledScroll.Pool
{
    public interface IScrollObjectPool : IReInitObjectPool<IPoolingGameObject>
    {
    }
}