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
        private Transform _contentParent;

        public ScrollItemPool(IFactory<ScrollItemView> factory) 
            : base(factory, inactiveObjectsParent: null, initialSize: 10, maxSize: 50)
        {
        }

        /// <summary>
        /// Initialize pool with content parent transform
        /// </summary>
        public void Initialize(Transform contentParent)
        {
            _contentParent = contentParent;
        }

        protected override void OnSpawned(ScrollItemView item)
        {
            if (_contentParent != null && item.RectTransform.parent != _contentParent)
                item.RectTransform.SetParent(_contentParent, false);
            
            item.SetActive(true);
        }

        protected override void OnDespawned(ScrollItemView item)
        {
            item.SetActive(false);
        }
    }
}

