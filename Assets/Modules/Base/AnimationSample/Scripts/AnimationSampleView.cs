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

namespace Modules.Base.AnimationSampleModule.Scripts
{
    /// <summary>
    /// Commands structure for AnimationSample module UI interactions
    /// </summary>
    public readonly struct AnimationSampleCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;

        public AnimationSampleCommands(ReactiveCommand<Unit> openMainMenuCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
        }
    }
    
    /// <summary>
    /// View for AnimationSample module - displays 5 animated and 100 static UI squares
    /// Optimized with Canvas Static for static elements
    /// </summary>
    public class AnimationSampleView : BaseView
    {
        [Header("UI Elements")]
        [SerializeField] private Button exitButton;
        
        [Header("Animation Containers")]
        [SerializeField] private RectTransform animatedContainer;
        [SerializeField] private RectTransform staticContainer;
        
        [Header("Prefabs")]
        [SerializeField] private AnimatedSquareView animatedSquarePrefab;
        [SerializeField] private Image staticSquarePrefab;
        
        [Header("Settings")]
        [SerializeField] private int animatedCount = 5;
        [SerializeField] private int staticCount = 100;
        [SerializeField] private float squareSize = 50f;
        [SerializeField] private Color[] squareColors = new[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
        
        private InputSystemService _inputSystemService;
        private readonly List<AnimatedSquareView> _animatedSquares = new();
        private readonly List<Image> _staticSquares = new();

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

        public void SetupEventListeners(AnimationSampleCommands commands)
        {
            _inputSystemService.SwitchToUI();
            
            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);
            
            var openMainMenuPerformedObservable =
                _inputSystemService.GetPerformedObservable(_inputSystemService.InputActions.UI.Cancel);

            openMainMenuPerformedObservable
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);
        }

        public override async UniTask Show()
        {
            await base.Show();
            _inputSystemService.SwitchToUI();
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        public async UniTask InitializeSquares()
        {
            ClearSquares();

            // Create animated squares
            for (int i = 0; i < animatedCount; i++)
            {
                var square = Instantiate(animatedSquarePrefab, animatedContainer);
                square.RectTransform.sizeDelta = new Vector2(squareSize, squareSize);
                
                // Random position within container
                var containerRect = animatedContainer.rect;
                var randomPos = new Vector2(
                    Random.Range(-containerRect.width / 2 + squareSize, containerRect.width / 2 - squareSize),
                    Random.Range(-containerRect.height / 2 + squareSize, containerRect.height / 2 - squareSize)
                );
                
                square.Initialize(randomPos);
                square.Image.color = squareColors[i % squareColors.Length];
                
                _animatedSquares.Add(square);
            }

            // Create static squares in a grid
            int columns = 10;
            int rows = Mathf.CeilToInt((float)staticCount / columns);
            float spacing = 10f;
            float totalWidth = columns * (squareSize + spacing) - spacing;
            float totalHeight = rows * (squareSize + spacing) - spacing;
            float startX = -totalWidth / 2 + squareSize / 2;
            float startY = totalHeight / 2 - squareSize / 2;

            for (int i = 0; i < staticCount; i++)
            {
                var square = Instantiate(staticSquarePrefab, staticContainer);
                var rectTransform = square.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(squareSize, squareSize);
                
                int row = i / columns;
                int col = i % columns;
                rectTransform.anchoredPosition = new Vector2(
                    startX + col * (squareSize + spacing),
                    startY - row * (squareSize + spacing)
                );
                
                square.color = squareColors[Random.Range(0, squareColors.Length)];
                _staticSquares.Add(square);
            }

            await UniTask.Yield();
        }

        public async UniTask StartAnimations()
        {
            foreach (var square in _animatedSquares) 
                await square.StartAnimation();
        }

        public void StopAnimations()
        {
            foreach (var square in _animatedSquares.Where(square => square)) square.StopAnimation();
        }

        public void OnScreenEnabled() => _inputSystemService.SetFirstSelectedObject(exitButton);

        private void ClearSquares()
        {
            foreach (var square in _animatedSquares.Where(square => square)) Destroy(square.gameObject);
            _animatedSquares.Clear();

            foreach (var square in _staticSquares.Where(square => square)) Destroy(square.gameObject);
            _staticSquares.Clear();
        }

        private void ValidateUIElements()
        {
            if (!exitButton) 
                Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(AnimationSampleView)}");
            if (!animatedContainer) 
                Debug.LogError($"{nameof(animatedContainer)} is not assigned in {nameof(AnimationSampleView)}");
            if (!staticContainer) 
                Debug.LogError($"{nameof(staticContainer)} is not assigned in {nameof(AnimationSampleView)}");
            if (!animatedSquarePrefab) 
                Debug.LogError($"{nameof(animatedSquarePrefab)} is not assigned in {nameof(AnimationSampleView)}");
            if (!staticSquarePrefab) 
                Debug.LogError($"{nameof(staticSquarePrefab)} is not assigned in {nameof(AnimationSampleView)}");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (!this) return;
            StopAnimations();
            ClearSquares();
        }
    }
}