using System.Collections.Generic;
using Core.Pooling.GameObjectPool;
using UI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Core.PooledScroll.Behaviors.BaseLogic
{
    public class ScrollCell : MonoBehaviour, IPoolingGameObject
    {
        [SerializeField]
        private float _spacing;
        [SerializeField]
        private TextAnchor _childAlignment;

        public int Index { get; set; }
        
        public Transform Transform => transform;
        public float Spacing => _spacing;
        public IReadOnlyList<IPoolingGameObject> Children => _children;
        public IPoolingGameObject this[int index] => _children[index];
        
        private Vector2 _cellSize;
        private bool _vertical;
        
        private readonly List<IPoolingGameObject> _children = new List<IPoolingGameObject>();

        public void Initialize(bool vertical, Vector2 cellSize)
        {
            _vertical = vertical;
            _cellSize = cellSize;
            
            CreateGroup();
            CreateLayoutElement();
        }

        public void AddChild(IPoolingGameObject child)
        {
            child.Transform.SetParent(Transform, false);
            child.Transform.gameObject.SetActive(true);
            
            _children.Add(child);
        }

        public void Clear()
        {
            _children.Clear();
        }

        private void CreateGroup()
        {
            HorizontalOrVerticalLayoutGroup group;
            
            if (_vertical)
                group = gameObject.AddComponent<HorizontalLayoutGroup>();
            else
                group = gameObject.AddComponent<VerticalLayoutGroup>();
            
            group.spacing = _spacing;
            group.childAlignment = _childAlignment;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;
        }

        private void CreateLayoutElement()
        {
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = _cellSize.x;
            layoutElement.preferredHeight = _cellSize.y;
        }
    }
}
