using Core.PooledScroll.Pool;
using Core.Pooling.Base;
using Core.Pooling.Base.Abstractions;
using Core.Pooling.GameObjectPool;
using Core.Pooling.ReInitPool;
using UnityEngine;

namespace UI.Controls
{
    public class ScrollObjectPool<TPoolingObject> : IScrollObjectPool where TPoolingObject : IPoolingGameObject
    {
        private const string ScrollObjectsPool = "ScrollObjectsPool";
        
        private readonly Transform _poolContainer;
        
        private readonly IReInitPoolListener<TPoolingObject> _listener;
        private readonly IObjectPool<TPoolingObject> _objectPool;
        
        public ScrollObjectPool(IReInitPoolListener<TPoolingObject> listener, Transform poolContainer = null)
        {
            _listener = listener;
            _objectPool = new ObjectPool<TPoolingObject>(listener);

            _poolContainer = poolContainer
                ? poolContainer
                : new GameObject($"{ScrollObjectsPool}:{typeof(TPoolingObject).Name}").transform;
            _poolContainer.gameObject.SetActive(false);
        }

        public void ReInitialize(int index, IPoolingGameObject poolingGameObject)
        {
            _listener.OnReInitialize(index, (TPoolingObject)poolingGameObject);
        }
        
        public IPoolingGameObject Get()
        {
            return _objectPool.Get();
        }
        
        public void Release(IPoolingGameObject poolingGameObject)
        {
            poolingGameObject.Transform.gameObject.SetActive(false);
            poolingGameObject.Transform.SetParent(_poolContainer, false);
            
            _objectPool.Release((TPoolingObject)poolingGameObject);
        }
        
        public void Dispose()
        {
            _objectPool.Dispose();
            if (_poolContainer)
                Object.Destroy(_poolContainer.gameObject);
        }
    }
}