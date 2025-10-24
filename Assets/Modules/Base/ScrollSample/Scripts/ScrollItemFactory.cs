using CodeBase.Core.Patterns.ObjectCreation;
using UnityEngine;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Factory for creating scroll item views
    /// </summary>
    public class ScrollItemFactory : IFactory<ScrollItemView>
    {
        private readonly ScrollItemView _prefab;
        private readonly Transform _parent;

        public ScrollItemFactory(ScrollItemView prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public ScrollItemView Create()
        {
            var item = Object.Instantiate(_prefab, _parent);
            item.SetActive(false);
            return item;
        }
    }
}

