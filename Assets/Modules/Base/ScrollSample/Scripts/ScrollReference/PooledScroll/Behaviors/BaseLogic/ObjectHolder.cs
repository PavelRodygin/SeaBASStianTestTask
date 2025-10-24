using Core.Pooling.Base.Abstractions;
using Core.Pooling.GameObjectPool;
using UnityEngine;

namespace Core.PooledScroll.Behaviors.BaseLogic
{
    public class ObjectHolder : IPoolingObject
    {
        public int Index { get; set; }
        public Transform Transform => Object.Transform;
        public IPoolingGameObject Object { get; set; }
    }
}
