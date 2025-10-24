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

        public ScrollItemFactory(ScrollItemView prefab)
        {
            _prefab = prefab;
        }

        public ScrollItemView Create()
        {
            var item = Object.Instantiate(_prefab);
            item.SetActive(false);
            return item;
        }
    }
}

