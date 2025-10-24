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
        [SerializeField] private VerticalLayoutGroup contentLayoutGroup;
        
        [Header("Scroll Settings")]
        [SerializeField] private float itemHeight = 100f;
        [SerializeField] private float spacing = 10f;
        [SerializeField] private int bufferItemCount = 5; // Extra items above/below viewport
        
        private InputSystemService _inputSystemService;
        private ScrollItemPool _itemPool;
        private readonly Dictionary<int, ScrollItemView> _activeItems = new();
        private int _totalItemCount;
        private int _visibleItemCount;
        private float _viewportHeight;
        private float _lastScrollPosition = -1f;
        private const float ScrollThreshold = 10f; // Minimum scroll distance to trigger update
        
        // Placeholders for optimization
        private LayoutElement _topPlaceholder;
        private LayoutElement _bottomPlaceholder;

        [Inject]
        private void Construct(InputSystemService inputSystemService, ScrollItemPool scrollItemPool)
        {
            _inputSystemService = inputSystemService;
            _itemPool = scrollItemPool;
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
            
            // Setup LayoutGroup if available
            if (contentLayoutGroup != null)
            {
                contentLayoutGroup.spacing = spacing;
                contentLayoutGroup.childControlHeight = true;
                contentLayoutGroup.childControlWidth = true;
                contentLayoutGroup.childForceExpandHeight = false;
                contentLayoutGroup.childForceExpandWidth = true;
            }
            
            // Create placeholders for scroll optimization
            CreatePlaceholders();
            
            // Initialize pool with content parent
            _itemPool.Initialize(content);
            
            // Calculate viewport height and visible items
            _viewportHeight = scrollRect.viewport.rect.height;
            
            // Calculate how many items fit in viewport + buffer on both sides
            int itemsInViewport = Mathf.CeilToInt(_viewportHeight / (itemHeight + spacing));
            _visibleItemCount = itemsInViewport + (bufferItemCount * 2); // Buffer above and below
            
            // Set content height (will be managed by layout group + placeholders)
            float totalHeight = totalItems * (itemHeight + spacing);
            if (contentLayoutGroup == null)
                totalHeight -= spacing; // Remove last spacing only if no layout group
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
            
            // Initial population
            UpdateVisibleItems();
        }
        
        private void CreatePlaceholders()
        {
            // Top placeholder
            var topPlaceholderObj = new GameObject("TopPlaceholder", typeof(RectTransform), typeof(LayoutElement));
            topPlaceholderObj.transform.SetParent(content, false);
            _topPlaceholder = topPlaceholderObj.GetComponent<LayoutElement>();
            _topPlaceholder.minHeight = 0;
            _topPlaceholder.transform.SetAsFirstSibling();
            
            // Bottom placeholder
            var bottomPlaceholderObj = new GameObject("BottomPlaceholder", typeof(RectTransform), typeof(LayoutElement));
            bottomPlaceholderObj.transform.SetParent(content, false);
            _bottomPlaceholder = bottomPlaceholderObj.GetComponent<LayoutElement>();
            _bottomPlaceholder.minHeight = 0;
            _bottomPlaceholder.transform.SetAsLastSibling();
        }

        public void OnScreenEnabled() => 
            _inputSystemService.SetFirstSelectedObject(exitButton);

        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            // Throttle updates - only update if scrolled enough
            float currentScroll = content.anchoredPosition.y;
            if (_lastScrollPosition < 0 || Mathf.Abs(currentScroll - _lastScrollPosition) >= ScrollThreshold)
            {
                _lastScrollPosition = currentScroll;
                UpdateVisibleItems();
            }
        }

        private void UpdateVisibleItems()
        {
            if (_itemPool == null || _totalItemCount == 0) return;

            // Calculate visible range with buffer
            float scrollPosition = content.anchoredPosition.y;
            int centerIndex = Mathf.FloorToInt(scrollPosition / (itemHeight + spacing));
            
            // Add buffer around center
            int firstVisibleIndex = Mathf.Max(0, centerIndex - bufferItemCount);
            int lastVisibleIndex = Mathf.Min(_totalItemCount - 1, centerIndex + _visibleItemCount - bufferItemCount);
            
            // Ensure we have enough items
            if (lastVisibleIndex - firstVisibleIndex < _visibleItemCount && lastVisibleIndex < _totalItemCount - 1)
            {
                lastVisibleIndex = Mathf.Min(_totalItemCount - 1, firstVisibleIndex + _visibleItemCount);
            }

            // Temporarily disable LayoutGroup for batch operations
            bool layoutWasEnabled = false;
            if (contentLayoutGroup != null)
            {
                layoutWasEnabled = contentLayoutGroup.enabled;
                contentLayoutGroup.enabled = false;
            }

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
                
                // Set size and position based on whether LayoutGroup is used
                if (contentLayoutGroup == null)
                {
                    // Manual positioning
                    item.RectTransform.anchoredPosition = new Vector2(0, -i * (itemHeight + spacing));
                    item.RectTransform.sizeDelta = new Vector2(content.rect.width, itemHeight);
                }
                else
                {
                    // LayoutGroup will manage positioning, configure LayoutElement
                    if (item.LayoutElement != null)
                        item.LayoutElement.preferredHeight = itemHeight;
                }
                
                // Insert item between placeholders
                int siblingIndex = i - firstVisibleIndex + 1; // +1 for top placeholder
                item.RectTransform.SetSiblingIndex(siblingIndex);
                
                _activeItems[i] = item;
            }
            
            // Update placeholders to offset content correctly
            UpdatePlaceholders(firstVisibleIndex, lastVisibleIndex);

            // Re-enable LayoutGroup and force rebuild once
            if (contentLayoutGroup != null && layoutWasEnabled)
            {
                contentLayoutGroup.enabled = true;
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }
        }
        
        private void UpdatePlaceholders(int firstVisibleIndex, int lastVisibleIndex)
        {
            // Top placeholder height (for all items above visible range)
            float topHeight = firstVisibleIndex * (itemHeight + spacing);
            _topPlaceholder.minHeight = topHeight;
            _topPlaceholder.gameObject.SetActive(topHeight > 0);
            
            // Bottom placeholder height (for all items below visible range)
            float bottomHeight = (_totalItemCount - 1 - lastVisibleIndex) * (itemHeight + spacing);
            _bottomPlaceholder.minHeight = bottomHeight;
            _bottomPlaceholder.gameObject.SetActive(bottomHeight > 0);
        }

        private void ValidateUIElements()
        {
            if (exitButton == null) 
                Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(ScrollSampleView)}");
            if (scrollRect == null) 
                Debug.LogError($"{nameof(scrollRect)} is not assigned in {nameof(ScrollSampleView)}");
            if (content == null) 
                Debug.LogError($"{nameof(content)} is not assigned in {nameof(ScrollSampleView)}");
            if (contentLayoutGroup == null) 
                Debug.LogWarning($"{nameof(contentLayoutGroup)} is not assigned in {nameof(ScrollSampleView)} - will use manual positioning");
        }

        public override void Dispose()
        {
            base.Dispose();
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            
            // Clear active items
            foreach (var item in _activeItems.Values)
                _itemPool?.Despawn(item);
            _activeItems.Clear();
            
            // Clear pool
            _itemPool?.Clear();
            
            // Destroy placeholders
            if (_topPlaceholder != null) 
                Destroy(_topPlaceholder.gameObject);
            if (_bottomPlaceholder != null) 
                Destroy(_bottomPlaceholder.gameObject);
        }
    }
}