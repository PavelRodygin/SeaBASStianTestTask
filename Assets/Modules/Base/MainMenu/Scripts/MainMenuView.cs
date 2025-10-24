using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Modules.Base.MainMenu.Scripts
{
    public readonly struct MainMenuCommands
    {
        public readonly ReactiveCommand<Unit> OpenScrollSampleCommand;
        public readonly ReactiveCommand<Unit> OpenTimerSampleCommand;
        public readonly ReactiveCommand<Unit> OpenAnimationSampleCommand;
        public readonly ReactiveCommand<Unit> OpenRequestSampleCommand;
        public readonly ReactiveCommand<Unit> SettingsPopupCommand;
        public readonly ReactiveCommand<bool> SoundToggleCommand;

        public MainMenuCommands(
            ReactiveCommand<Unit> openScrollSampleCommand,
            ReactiveCommand<Unit> openTimerSampleCommand,
            ReactiveCommand<Unit> openAnimationSampleCommand,
            ReactiveCommand<Unit> openRequestSampleCommand,
            ReactiveCommand<Unit> settingsPopupCommand,
            ReactiveCommand<bool> soundToggleCommand)
        {
            OpenScrollSampleCommand = openScrollSampleCommand;
            OpenTimerSampleCommand = openTimerSampleCommand;
            OpenAnimationSampleCommand = openAnimationSampleCommand;
            OpenRequestSampleCommand = openRequestSampleCommand;
            SettingsPopupCommand = settingsPopupCommand;
            SoundToggleCommand = soundToggleCommand;
        }
    }
    
    public class MainMenuView : BaseView
    {
        [Header("Test Task Module Buttons")]
        [SerializeField] private Button scrollSampleButton;
        [SerializeField] private Button timerSampleButton;
        [SerializeField] private Button animationSampleButton;
        [SerializeField] private Button requestSampleButton;
        
        [Header("UI Controls")]
        [SerializeField] private Button settingsPopupButton;
        [SerializeField] private Toggle musicToggle;

        private InputSystemService _inputSystemService;
        
        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;   
        }
        
        protected override void Awake()
        {
            base.Awake();
        }

        public void SetupEventListeners(MainMenuCommands commands)
        {
            scrollSampleButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenScrollSampleCommand.Execute(default))
                .AddTo(this);

            timerSampleButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenTimerSampleCommand.Execute(default))
                .AddTo(this);

            animationSampleButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenAnimationSampleCommand.Execute(default))
                .AddTo(this);

            requestSampleButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenRequestSampleCommand.Execute(default))
                .AddTo(this);

            settingsPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SettingsPopupCommand.Execute(default))
                .AddTo(this);

            musicToggle.OnValueChangedAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SoundToggleCommand.Execute(musicToggle.isOn))
                .AddTo(this);
        }

        public override async UniTask Show()
        {
            OnScreenEnabled();
            await base.Show();
        }

        public void InitializeSoundToggle(bool isMusicOn) => musicToggle.SetIsOnWithoutNotify(isMusicOn);

        public void OnScreenEnabled()
        {
            _inputSystemService.SetFirstSelectedObject(scrollSampleButton);
        }
    }
}