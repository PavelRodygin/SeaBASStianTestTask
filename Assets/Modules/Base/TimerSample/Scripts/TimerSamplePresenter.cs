using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.TimerSampleModule.Scripts
{
    /// <summary>
    /// Presenter for TimerSample module that handles business logic and coordinates between Model and View
    /// 
    /// IMPORTANT: This is a timerSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from TimerSamplePresenter to YourModuleNamePresenter
    /// 2. Update namespace Modules.Base.TimerSampleModule.Scripts match your module location
    /// 3. Add your specific business logic and commands
    /// 4. Customize module navigation logic
    /// 5. Implement your specific UI event handling
    /// 6. Add any additional services or systems your module needs
    /// 
    /// NOTE: Navigation to MainMenuModule is already implemented via exit button
    /// </summary>
    public class TimerSamplePresenter : IDisposable
    {
        private readonly TimerSampleModuleModel _timerSampleModuleModel;
        private readonly TimerSampleView _timerSampleView;
        private readonly AudioSystem _audioSystem;
        private readonly IPopupHub _popupHub;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();

        public TimerSamplePresenter(
            TimerSampleModuleModel timerSampleModuleModel,
            TimerSampleView timerSampleView,
            AudioSystem audioSystem,
            IPopupHub popupHub)
        {
            _timerSampleModuleModel = timerSampleModuleModel ?? throw new ArgumentNullException(nameof(timerSampleModuleModel));
            _timerSampleView = timerSampleView ?? throw new ArgumentNullException(nameof(timerSampleView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _timerSampleView.HideInstantly();

            var commands = new TimerSampleCommands(
                _openMainMenuCommand,
                _settingsPopupCommand,
                _toggleSoundCommand
            );

            _timerSampleView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            _timerSampleView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _timerSampleView.Show();
            
            _audioSystem.PlayMainMenuMelody();
        }

        public async UniTask Exit()
        {
            await _timerSampleView.Hide();
        }
        
        public void HideInstantly() => _timerSampleView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_timerSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_timerSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupButtonClicked())
                .AddTo(_disposables);

            _toggleSoundCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_timerSampleModuleModel.CommandThrottleDelay))
                .Subscribe(OnSoundToggled)
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }

        private void OnSettingsPopupButtonClicked()
        {
            _popupHub.OpenSettingsPopup();
        }

        private void OnSoundToggled(bool isOn)
        {
            _audioSystem.SetMusicVolume(isOn ? 1f : 0f);
        }
    }
}
