using System.Collections.Generic;
using System.Linq;
using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Unit = R3.Unit;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Commands structure for ScrollSample module UI interactions
    /// </summary>
    public readonly struct ScrollSampleCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;

        public ScrollSampleCommands(ReactiveCommand<Unit> openMainMenuCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
        }
    }
    
    /// <summary>
    /// View for ScrollSample module - virtualized scroll with 1000 items using object pooling
    /// </summary>
    public class ScrollSampleView : BaseView
    {
        [Header("UI Elements")]
        [SerializeField] private Button exitButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private ScrollItemView itemPrefab;
        
        [Header("Scroll Settings")]
        [SerializeField] private float itemHeight = 100f;
        [SerializeField] private float spacing = 10f;
        
        private InputSystemService _inputSystemService;
        private ScrollItemPool _itemPool;
        private readonly Dictionary<int, ScrollItemView> _activeItems = new();
        private int _totalItemCount;
        private int _visibleItemCount;
        private float _viewportHeight;

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
            ValidateUIElements();
            #endif
        }

        public void SetupEventListeners(ScrollSampleCommands commands)
        {
            _inputSystemService.SwitchToUI();
            
            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);

            // Keyboard navigation - Escape key for exit
            var openMainMenuPerformedObservable =
                _inputSystemService.GetPerformedObservable(_inputSystemService.InputActions.UI.Cancel);

            openMainMenuPerformedObservable
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);
            
            // Subscribe to scroll events for virtualization
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        public override async UniTask Show()
        {
            await base.Show();
            _inputSystemService.SwitchToUI();
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        public void InitializeScroll(int totalItems)
        {
            _totalItemCount = totalItems;
            
            // Create object pool
            var factory = new ScrollItemFactory(itemPrefab, content);
            _itemPool = new ScrollItemPool(factory, content, initialSize: 20, maxSize: 30);
            
            // Calculate viewport height and visible items
            _viewportHeight = scrollRect.viewport.rect.height;
            _visibleItemCount = Mathf.CeilToInt(_viewportHeight / (itemHeight + spacing)) + 2; // +2 buffer
            
            // Set content height
            float totalHeight = totalItems * (itemHeight + spacing) - spacing;
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
            
            // Initial population
            UpdateVisibleItems();
        }

        public void OnScreenEnabled() => 
            _inputSystemService.SetFirstSelectedObject(exitButton);

        private void OnScrollValueChanged(Vector2 scrollPosition) => UpdateVisibleItems();

        private void UpdateVisibleItems()
        {
            if (_itemPool == null || _totalItemCount == 0) return;

            // Calculate visible range
            float scrollPosition = content.anchoredPosition.y;
            int firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(scrollPosition / (itemHeight + spacing)));
            int lastVisibleIndex = Mathf.Min(_totalItemCount - 1, firstVisibleIndex + _visibleItemCount);

            // Despawn items outside visible range
            var itemsToRemove = new List<int>();
            foreach (var kvp in _activeItems.Where(kvp => kvp.Key < firstVisibleIndex || kvp.Key > lastVisibleIndex))
            {
                _itemPool.Despawn(kvp.Value);
                itemsToRemove.Add(kvp.Key);
            }
            foreach (var index in itemsToRemove)
                _activeItems.Remove(index);

            // Spawn items in visible range
            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                if (_activeItems.ContainsKey(i)) continue;
                
                var item = _itemPool.Spawn();
                if (!item) continue;
                    
                item.Initialize(i);
                item.RectTransform.SetParent(content, false);
                item.RectTransform.anchoredPosition = new Vector2(0, -i * (itemHeight + spacing));
                item.RectTransform.sizeDelta = new Vector2(content.rect.width, itemHeight);
                _activeItems[i] = item;
            }
        }

        private void ValidateUIElements()
        {
            if (exitButton == null) 
                Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(ScrollSampleView)}");
            if (scrollRect == null) 
                Debug.LogError($"{nameof(scrollRect)} is not assigned in {nameof(ScrollSampleView)}");
            if (content == null) 
                Debug.LogError($"{nameof(content)} is not assigned in {nameof(ScrollSampleView)}");
            if (itemPrefab == null) 
                Debug.LogError($"{nameof(itemPrefab)} is not assigned in {nameof(ScrollSampleView)}");
        }

        public override void Dispose()
        {
            base.Dispose();
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            _itemPool?.Clear();
        }
    }
}