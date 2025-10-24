using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.PooledScroll.AdditionalFunctionality.Move.Abstractions;
using Core.PooledScroll.AdditionalFunctionality.Navigation;
using Core.PooledScroll.AdditionalFunctionality.Navigation.Abstractions;
using Core.PooledScroll.Behaviors.BaseLogic;
using Core.PooledScroll.Events;
using Core.PooledScroll.Extensions;
using Core.PooledScroll.Pool;
using Core.Pooling.Base.Abstractions;
using Core.Pooling.GameObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.PooledScroll.Behaviors.GridScroll
{
    [RequireComponent(typeof(ScrollRect))]
    public class GridScrollBehaviour : ScrollBehaviour, INavigationScroll, IMovableScroll, IPoolListener<ScrollCell>, IBeginDragHandler
    {
        [SerializeField] private HorizontalOrVerticalLayoutGroup contentGroup;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ScrollCell cell;
        [SerializeField] private Vector2 objectSize;

        public override event Action ContentCalculated;
        public override event Action<ScrollingEventArgs> ScrollStarted;
        public override event Action<ScrollingEventArgs> ScrollEnded;
        public override event Action<ScrollingEventArgs> ScrollRangeChanged;
        public override int ActiveObjects => _activeCells.Count * _objectsInCell;

        public int ObjectsCount => _offsetsInfos.Count * _objectsInCell;
        public float AxisPosition => _scrollPosition;

        private const string FirstPlaceholderName = "FirstPlaceholder";
        private const string LastPlaceholderName = "LastPlaceholder";
        
        private IScrollNavigation _navigation;
        private IScrollObjectPool _objectPool;
        private IObjectPool<ScrollCell> _cellsPool;
        
        private LayoutElement _firstPlaceholder;
        private LayoutElement _lastPlaceholder;
        private RectTransform _scrollRectTransform;
        
        private Vector2 _cellSize;
        private int _maxObjectsCount;
        private int _currentStartIndex;
        private int _currentEndIndex;
        private int _objectsInCell;
        private (int startIndex, int endIndex) _objectsRange;
        private float _scrollPosition;
        private bool _velocityChanged;
        
        private readonly List<ScrollCell> _activeCells = new List<ScrollCell>();
        private readonly List<OffsetInfo> _offsetsInfos = new List<OffsetInfo>();
        

        private void Start()
        {
            /*if (_currentStartIndex == _currentEndIndex)
            {
                RecalculateContent();
                ScrollValueChanged(Vector2.zero);
                ContentCalculated?.Invoke();
            }*/
        }

        private void Update()
        {
            if (!_velocityChanged && scrollRect.GetAxisVelocity() == 0)
            {
                _velocityChanged = false;
                ScrollEnded?.Invoke(new ScrollingEventArgs(_objectsRange.startIndex, _objectsRange.endIndex));
            }
        }

        public override void CalculateMainContent(int maxElementsCount)
        {
            if (_maxObjectsCount == 0)
            {
                _scrollRectTransform = (RectTransform)scrollRect.transform;
                _navigation = GetComponent<IScrollNavigation>() ?? NullNavigation.Instance;
                _cellsPool = new GameObjectPool<ScrollCell>(this, _scrollRectTransform, contentGroup.transform);
            }
            _maxObjectsCount = maxElementsCount;

            CalculateContent(); 
        }

        public override void Initialize(int count, IScrollObjectPool objectPool, int startIndex = 0)
        {
            _objectPool = objectPool;

            _firstPlaceholder = CreatePlaceholder(FirstPlaceholderName);
            _lastPlaceholder = CreatePlaceholder(LastPlaceholderName);
            
            //CalculateContent();
            UpdateVisibleObjects();
            scrollRect.onValueChanged.AddListener(ScrollValueChanged);
            if (_currentStartIndex != _currentEndIndex)
                ContentCalculated?.Invoke();
        }
        
        public override void Reinitialize(int count)
        {
            foreach (var cell in _activeCells)
                _cellsPool.Release(cell);
            _activeCells.Clear();
    
            _currentStartIndex = 0;
            _currentEndIndex = 0;
            _offsetsInfos.Clear();
    
            if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition = 1f;
            else
                scrollRect.horizontalNormalizedPosition = 0f;
    
            RecalculateContent();
            ScrollValueChanged(Vector2.zero);
            ContentCalculated?.Invoke();
        }
        
        public override int GetMaxVisibleObjects()
        {
            var columnsNumber = GetIndexAtPosition(GetScrollEndPosition()) + 1;
            return (columnsNumber + 1) * _objectsInCell;
        }
        
        private void CalculateContent()
        {
            CalculateObjectsInCell(scrollRect.viewport.rect, objectSize);
            CalculateCellSize();
            CalculateOffsetsInfos(_maxObjectsCount, objectSize);
        }
        
        private void RecalculateContent()
        {
            CalculateContent();
            UpdateVisibleObjects();
        }
        
        public override void Dispose()
        {
            scrollRect.onValueChanged.RemoveListener(ScrollValueChanged);
            
            _objectPool.Dispose();
            _cellsPool.Dispose();
        }
        
        public override UniTask ScrollNext(CancellationToken cancellationToken) => 
            _navigation.Next(cancellationToken);

        public override UniTask ScrollBack(CancellationToken cancellationToken) => 
            _navigation.Back(cancellationToken);

        public override int CurrentStartIndex => _currentStartIndex;
        public override int CurrentEndIndex => _currentEndIndex;

        public ScrollCell OnCreate()
        {
            var cell = Instantiate(this.cell);
            cell.Initialize(scrollRect.vertical, _cellSize);
            
            OnGet(cell);
            return cell;
        }

        public void OnGet(ScrollCell cell) { }

        public void OnRelease(ScrollCell cell)
        {
            foreach (var child in cell.Children)
                _objectPool.Release(child);
            
            cell.Clear();
        }

        public void OnDispose(ScrollCell cell) {}

        public int GetIndexAtPosition(float position) => 
            GetIndexAtPosition(position, 0, _offsetsInfos.Count - 1);

        public float GetScrollEndPosition()
        {
            var rect = _scrollRectTransform.rect;
            return _scrollPosition + (scrollRect.vertical ? rect.height : rect.width);
        }

        public float GetPositionByIndex(int index)
        {
            var elementInfo = _offsetsInfos[index - 1];
            var rect = scrollRect.viewport.rect;
            var viewportAxisSize = scrollRect.vertical ? rect.height : rect.width;
            var viewportAxisHalfSize = viewportAxisSize / 2;
            var padding = scrollRect.vertical ? contentGroup.padding.top : contentGroup.padding.left;
            var elementHalfSizeWithSpacing = (_offsetsInfos[index].Size + contentGroup.spacing) / 2;
            var pos = elementInfo.Offset - viewportAxisHalfSize + padding + elementHalfSizeWithSpacing;

            pos = Mathf.Clamp(pos, 0, GetScrollSize());

            return pos;
        }

        public void UpdateAxisPosition(float position)
        {
            _scrollPosition = position;

            if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition  = 1f - AxisPosition / GetScrollSize();
            else
                scrollRect.horizontalNormalizedPosition  = AxisPosition / GetScrollSize();
        }
        
        private void FillCell(ScrollCell cell)
        {
            var startObjectIndex = cell.Index * _objectsInCell;
            var endObjectIndex = startObjectIndex + _objectsInCell - 1;
            
            for (var i = startObjectIndex; i <= endObjectIndex; ++i)
            {
                if (i >= _maxObjectsCount)
                    break;
                
                var poolingObject = _objectPool.Get();
                cell.AddChild(poolingObject);
            }
        }

        private LayoutElement CreatePlaceholder(string placeholderName)
        {
            var instance = new GameObject(placeholderName, typeof(RectTransform), typeof(LayoutElement));
            instance.transform.SetParent(contentGroup.transform, false);
            
            return instance.GetComponent<LayoutElement>();
        }
        
        private void CalculateObjectsInCell(Rect viewport, Vector2 objectSize)
        {
            var padding = scrollRect.vertical ? contentGroup.padding.horizontal : contentGroup.padding.vertical;
            var viewportAxisSize = (scrollRect.vertical ? viewport.width : viewport.height) - padding;
            
            var objectAxisSize = (scrollRect.vertical ? objectSize.x : objectSize.y) + cell.Spacing;
            _objectsInCell = Mathf.FloorToInt(viewportAxisSize / objectAxisSize);
        }
        
        private void CalculateCellSize()
        {
            float width;
            float height;
            
            if (scrollRect.vertical)
            {
                width = _objectsInCell * (objectSize.x + cell.Spacing) - cell.Spacing;
                height = objectSize.y;
            }
            else
            {
                width = objectSize.x;
                height = _objectsInCell * (objectSize.y + cell.Spacing) - cell.Spacing;
            }

            _cellSize = new Vector2(width, height);
        }
        
        private void CalculateOffsetsInfos(int count, Vector2 objectSize)
        {
            var cellsCount = Mathf.CeilToInt(count / (float)_objectsInCell);
            var offset = 0f;

            for (var i = 0; i < cellsCount; ++i)
            {
                var size = scrollRect.vertical ? objectSize.y : objectSize.x;
                var sizeWithSpacing = size + (i == 0 ? 0 : contentGroup.spacing);
                offset += sizeWithSpacing;

                var info = new OffsetInfo(sizeWithSpacing, offset);
                _offsetsInfos.Add(info);
            }
        }

        private void UpdateVisibleObjects()
        {
            CalculateVisibleObjectsRange(out var startIndex, out var endIndex);
            
            var i = 0;
            var remainingObjectsIndices = new List<int>();
            
            while (i < _activeCells.Count)
            {
                if (_activeCells[i].Index < startIndex || _activeCells[i].Index > endIndex)
                {
                    var cell = _activeCells[i];
                    
                    _activeCells.Remove(cell);
                    _cellsPool.Release(cell);
                    cell.Index = 0;
                }
                else
                {
                    remainingObjectsIndices.Add(_activeCells[i].Index);
                    i++;
                }
            }
            
            if (remainingObjectsIndices.Count == 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                {
                    AddCell(i, InsertType.Last);
                }
            }
            else
            {
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingObjectsIndices.First())
                    {
                        AddCell(i, InsertType.First);
                    }
                }
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingObjectsIndices.Last())
                    {
                        AddCell(i, InsertType.Last);
                    }
                }
            }
            
            _currentStartIndex = startIndex;
            _currentEndIndex = endIndex;
            
            UpdateCells();
        }
        
        private void CalculateVisibleObjectsRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;

            startIndex = GetIndexAtPosition(_scrollPosition);
            endIndex = GetIndexAtPosition(GetScrollEndPosition());
        }
        
        private void AddCell(int index, InsertType insertType)
        {
            if (_offsetsInfos.Count == 0)
                return;

            var cell = _cellsPool.Get();
            cell.Index = index;
            FillCell(cell);

            switch (insertType)
            {
                case InsertType.First:
                    _activeCells.Insert(0, cell);
                    cell.Transform.SetSiblingIndex(1);
                    break;
                case InsertType.Last:
                    _activeCells.Add(cell);
                    cell.Transform.SetSiblingIndex(contentGroup.transform.childCount - 2);
                    break;
            }
            
            UpdateCell(cell);
        }

        private void UpdateCell(ScrollCell cell)
        {
            var startObjectIndex = cell.Index * _objectsInCell;
            var endObjectIndex = startObjectIndex + cell.Children.Count;
            
            if (endObjectIndex >= _maxObjectsCount)
                DeactivateNonActualObjects(endObjectIndex, cell);

            UpdateActualObjects(startObjectIndex, endObjectIndex, cell);
        }

        private void DeactivateNonActualObjects(int endViewIndex, ScrollCell cell)
        {
            var lastChildrenIndex = cell.Children.Count - 1;
            var unloadViewsCount = endViewIndex - _maxObjectsCount;
                
            for (var i = lastChildrenIndex; i > lastChildrenIndex - unloadViewsCount; i--)
                _objectPool.Release(cell[i]);
        }
        
        private void UpdateActualObjects(int startViewIndex, int endViewIndex, ScrollCell cell)
        {
            var actualEndViewIndex = endViewIndex > _maxObjectsCount - 1 ? _maxObjectsCount : endViewIndex;
            var viewIndexInCell = 0;
            
            for (var i = startViewIndex; i < actualEndViewIndex; ++i)
            {
                _objectPool.ReInitialize(i, cell[viewIndexInCell]);
                viewIndexInCell++;
            }

            _objectsRange = (startViewIndex, endViewIndex);
        }
        
        private void UpdateCells()
        {
            if (_offsetsInfos.Count == 0)
                return;

            var firstSize = _offsetsInfos[_currentStartIndex].Offset - _offsetsInfos[_currentStartIndex].Size;
            var lastSize = _offsetsInfos.Last().Offset - _offsetsInfos[_currentEndIndex].Offset;
            
            if (scrollRect.vertical)
            {
                _firstPlaceholder.minHeight = firstSize;
                _firstPlaceholder.gameObject.SetActive(_firstPlaceholder.minHeight > 0);

                _lastPlaceholder.minHeight = lastSize;
                _lastPlaceholder.gameObject.SetActive(_lastPlaceholder.minHeight > 0);
            }
            else
            {
                _firstPlaceholder.minWidth = firstSize;
                _firstPlaceholder.gameObject.SetActive(_firstPlaceholder.minWidth > 0);

                _lastPlaceholder.minWidth = lastSize;
                _lastPlaceholder.gameObject.SetActive(_lastPlaceholder.minWidth > 0);
            }
        }

        private int GetIndexAtPosition(float position, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
                return startIndex;
        
            var middleIndex = (startIndex + endIndex) / 2;
            var padding = scrollRect.vertical ? contentGroup.padding.top : contentGroup.padding.left;
            
            return _offsetsInfos[middleIndex].Offset + padding >= position
                ? GetIndexAtPosition(position, startIndex, middleIndex)
                : GetIndexAtPosition(position, middleIndex + 1, endIndex);
        }
        
        private float GetScrollSize()
        {
            return scrollRect.vertical
                ? Mathf.Max(scrollRect.content.rect.height - _scrollRectTransform.rect.height, 0)
                : Mathf.Max(scrollRect.content.rect.width - _scrollRectTransform.rect.width, 0);
        }

        private void ScrollValueChanged(Vector2 value)
        {
            _velocityChanged = true;
            _scrollPosition = scrollRect.vertical ? (1f - value.y) * GetScrollSize() : value.x * GetScrollSize();
            
            CalculateVisibleObjectsRange(out var startIndex, out var endIndex);
            
            if (startIndex == _currentStartIndex && endIndex == _currentEndIndex)
                return;
            
            UpdateVisibleObjects();
            ScrollRangeChanged?.Invoke(new ScrollingEventArgs(_objectsRange.startIndex, _objectsRange.endIndex));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ScrollStarted?.Invoke(new ScrollingEventArgs(_objectsRange.startIndex, _objectsRange.endIndex));
        }
        
        private struct OffsetInfo
        {
            public float Size { get; }
            public float Offset { get; }

            public OffsetInfo(float size, float offset)
            {
                Size = size;
                Offset = offset;
            }
        }
    }
}