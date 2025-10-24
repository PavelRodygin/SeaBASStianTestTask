using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PooledScroll.AdditionalFunctionality.Snapping.Abstractions
{
    public abstract class BaseScrollSnappingBehaviour : MonoBehaviour, IScrollSnapping
    {
        public abstract event Action BeforeSnapping;
        public abstract event Action AfterSnapping;
        
        public abstract UniTask SnapToNearest(CancellationToken cancellationToken);
    }
}