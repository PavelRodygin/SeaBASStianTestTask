using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Unit = R3.Unit;

using System.Runtime.InteropServices;  // Для DllImport

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuPresenter : IDisposable
    {
        private readonly MainMenuModuleModel _mainMenuModuleModel;
        private readonly MainMenuView _mainMenuView;
        private readonly IPopupHub _popupHub;
        private readonly AudioSystem _audioSystem;
        
        private readonly ReactiveCommand<Unit> _openScrollSampleCommand = new();
        private readonly ReactiveCommand<Unit> _openTimerSampleCommand = new();
        private readonly ReactiveCommand<Unit> _openAnimationSampleCommand = new();
        private readonly ReactiveCommand<Unit> _openRequestSampleCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SetPageTitle(string newTitle);
#endif
        
        public MainMenuPresenter(IPopupHub popupHub, MainMenuModuleModel mainMenuModuleModel, 
            MainMenuView mainMenuView, AudioSystem audioSystem)
        {
            _mainMenuModuleModel = mainMenuModuleModel ?? throw new ArgumentNullException(nameof(mainMenuModuleModel));
            _mainMenuView = mainMenuView ?? throw new ArgumentNullException(nameof(mainMenuView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));

            SubscribeToUIUpdates();
        }

        private void SubscribeToUIUpdates()
        {
            _openScrollSampleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnScrollSampleCommand())
                .AddTo(_disposables);
            _openTimerSampleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnTimerSampleCommand())
                .AddTo(_disposables);
            _openAnimationSampleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnAnimationSampleCommand())
                .AddTo(_disposables);
            _openRequestSampleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnRequestSampleCommand())
                .AddTo(_disposables);
            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupCommand())
                .AddTo(_disposables);
            _toggleSoundCommand.Subscribe(OnToggleSoundCommand).AddTo(_disposables);
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _mainMenuView.HideInstantly();

            var commands = new MainMenuCommands(
                _openScrollSampleCommand,
                _openTimerSampleCommand,
                _openAnimationSampleCommand,
                _openRequestSampleCommand,
                _settingsPopupCommand,
                _toggleSoundCommand
            );

            _mainMenuView.SetupEventListeners(commands);
            _mainMenuView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _mainMenuView.Show();
            
            CallSetPageTitle("My Unity WebGL App");
            _audioSystem.PlayMainMenuMelody();
        }
        
        public async UniTask Exit()
        {
            await _mainMenuView.Hide();
            _audioSystem.StopMusic();
        }
        
        public void HideInstantly() => _mainMenuView.HideInstantly();

        public void CallSetPageTitle(string title)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetPageTitle(title);
#endif
        }
      
        public void ReceiveBrowserWidth(int width)
        {
            // Handle browser width for UI adaptation
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _mainMenuView?.Dispose();
            _mainMenuModuleModel?.Dispose();
        }

        private void OnScrollSampleCommand() => _openNewModuleCommand.Execute(ModulesMap.ScrollSample);
        private void OnTimerSampleCommand() => _openNewModuleCommand.Execute(ModulesMap.TimerSample);
        private void OnAnimationSampleCommand() => _openNewModuleCommand.Execute(ModulesMap.AnimationSample);
        private void OnRequestSampleCommand() => _openNewModuleCommand.Execute(ModulesMap.RequestSample);
        private void OnSettingsPopupCommand() => _popupHub.OpenSettingsPopup();
        private void OnToggleSoundCommand(bool isOn) => _audioSystem.SetMusicVolume(isOn ? 1 : 0);
    }
}