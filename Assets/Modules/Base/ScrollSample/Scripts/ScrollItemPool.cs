using CodeBase.Core.Patterns.ObjectCreation;
using CodeBase.Core.Patterns.ObjectCreation.ObjectPool;
using UnityEngine;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Object pool for scroll item views
    /// </summary>
    public class ScrollItemPool : ObjectPool<ScrollItemView>
    {
        public ScrollItemPool(
            IFactory<ScrollItemView> factory,
            Transform inactiveParent = null,
            int initialSize = 0,
            int maxSize = -1) 
            : base(factory, inactiveParent, initialSize, maxSize)
        {
        }

        protected override void OnSpawned(ScrollItemView item)
        {
            item.SetActive(true);
        }

        protected override void OnDespawned(ScrollItemView item)
        {
            item.SetActive(false);
        }
    }
}

