using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Unit = R3.Unit;

namespace Modules.Base.TimerSampleModule.Scripts
{
    /// <summary>
    /// Commands structure for TimerSample module UI interactions
    /// </summary>
    public readonly struct TimerSampleCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;

        public TimerSampleCommands(ReactiveCommand<Unit> openMainMenuCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
        }
    }
    
    /// <summary>
    /// View for TimerSample module - displays current time with millisecond precision
    /// Updates every frame without memory allocations
    /// </summary>
    public class TimerSampleView : BaseView
    {
        [Header("UI Elements")]
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text timeText;
        
        private InputSystemService _inputSystemService;

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

        public void SetupEventListeners(TimerSampleCommands commands)
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

        public void UpdateTimeText(string timeString)
        {
            if (timeText)
                timeText.text = timeString;
        }

        public void OnScreenEnabled()
        {
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        private void ValidateUIElements()
        {
            if (!exitButton) 
                Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(TimerSampleView)}");
            if (!timeText) 
                Debug.LogError($"{nameof(timeText)} is not assigned in {nameof(TimerSampleView)}");
        }
    }
}