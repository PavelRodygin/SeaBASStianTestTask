using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Presenter for ScrollSample module that handles business logic and coordinates between Model and View
    /// 
    /// IMPORTANT: This is a scrollSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from ScrollSamplePresenter to YourModuleNamePresenter
    /// 2. Update namespace Modules.Base.ScrollSampleModule.Scripts match your module location
    /// 3. Add your specific business logic and commands
    /// 4. Customize module navigation logic
    /// 5. Implement your specific UI event handling
    /// 6. Add any additional services or systems your module needs
    /// 
    /// NOTE: Navigation to MainMenuModule is already implemented via exit button
    /// </summary>
    public class ScrollSamplePresenter : IDisposable
    {
        private readonly ScrollSampleModuleModel _scrollSampleModuleModel;
        private readonly ScrollSampleView _scrollSampleView;
        private readonly AudioSystem _audioSystem;
        private readonly IPopupHub _popupHub;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();

        public ScrollSamplePresenter(
            ScrollSampleModuleModel scrollSampleModuleModel,
            ScrollSampleView scrollSampleView,
            AudioSystem audioSystem,
            IPopupHub popupHub)
        {
            _scrollSampleModuleModel = scrollSampleModuleModel ?? throw new ArgumentNullException(nameof(scrollSampleModuleModel));
            _scrollSampleView = scrollSampleView ?? throw new ArgumentNullException(nameof(scrollSampleView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _scrollSampleView.HideInstantly();

            var commands = new ScrollSampleCommands(
                _openMainMenuCommand,
                _settingsPopupCommand,
                _toggleSoundCommand
            );

            _scrollSampleView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            _scrollSampleView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _scrollSampleView.Show();
            
            _audioSystem.PlayMainMenuMelody();
        }

        public async UniTask Exit()
        {
            await _scrollSampleView.Hide();
        }
        
        public void HideInstantly() => _scrollSampleView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_scrollSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_scrollSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupButtonClicked())
                .AddTo(_disposables);

            _toggleSoundCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_scrollSampleModuleModel.CommandThrottleDelay))
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
