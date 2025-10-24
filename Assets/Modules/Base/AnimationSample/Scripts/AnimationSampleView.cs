using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
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
        public readonly ReactiveCommand<Unit> SettingsPopupCommand;
        public readonly ReactiveCommand<bool> SoundToggleCommand;

        public AnimationSampleCommands(
            ReactiveCommand<Unit> openMainMenuCommand,
            ReactiveCommand<Unit> settingsPopupCommand,
            ReactiveCommand<bool> soundToggleCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
            SettingsPopupCommand = settingsPopupCommand;
            SoundToggleCommand = soundToggleCommand;
        }
    }
    
    /// <summary>
    /// View for AnimationSample module that handles UI interactions and visual representation
    /// 
    /// IMPORTANT: This is a animationSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from AnimationSampleView to YourModuleNameView
    /// 2. Update namespace Modules.Base.AnimationSampleModule.Scripts match your module location
    /// 3. Add your specific UI elements and commands
    /// 4. Customize event handling for your UI
    /// 5. Update validation methods for your UI elements
    /// 6. Add any additional UI functionality your module needs
    /// 
    /// NOTE: Exit button (exitButton) is already configured to return to MainMenuModule
    /// </summary>
    public class AnimationSampleView : BaseView
    {
        [Header("UI Elements")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button settingsPopupButton;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private TMP_Text animationSampleScreenTitle;
        
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

        public void SetupEventListeners(AnimationSampleCommands commands)
        {
            _inputSystemService.SwitchToUI();
            
            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);

            settingsPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SettingsPopupCommand.Execute(default))
                .AddTo(this);

            musicToggle.OnValueChangedAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SoundToggleCommand.Execute(musicToggle.isOn))
                .AddTo(this);

            // Keyboard navigation support - Escape key for exit
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

        public void SetTitle(string title)
        {
            if (animationSampleScreenTitle != null)
                animationSampleScreenTitle.text = title;
            else
                Debug.LogWarning("animationSampleScreenTitle is not assigned in the Inspector.");
        }

        public void InitializeSoundToggle(bool isMusicOn) => musicToggle.SetIsOnWithoutNotify(isMusicOn);

        public void OnScreenEnabled()
        {
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        private void ValidateUIElements()
        {
            if (exitButton == null) Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(AnimationSampleView)}");
            if (settingsPopupButton == null) Debug.LogError($"{nameof(settingsPopupButton)} is not assigned in {nameof(AnimationSampleView)}");
            if (musicToggle == null) Debug.LogError($"{nameof(musicToggle)} is not assigned in {nameof(AnimationSampleView)}");
            if (animationSampleScreenTitle == null) Debug.LogError($"{nameof(animationSampleScreenTitle)} is not assigned in {nameof(AnimationSampleView)}");
        }
    }
}