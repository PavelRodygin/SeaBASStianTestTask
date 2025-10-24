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
        
        private readonly ReactiveCommand<Unit> _openConverterCommand = new();
        private readonly ReactiveCommand<Unit> _openTicTacCommand = new();
        private readonly ReactiveCommand<Unit> _openRoguelikeCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<Unit> _secondPopupCommand = new();
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
            _openConverterCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnConverterCommand())
                .AddTo(_disposables);
            _openTicTacCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnTicTacCommand())
                .AddTo(_disposables);
            _openRoguelikeCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnPlayground3DCommand())
                .AddTo(_disposables);
            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupCommand())
                .AddTo(_disposables);
            _secondPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSecondPopupCommand())
                .AddTo(_disposables);
            _toggleSoundCommand.Subscribe(OnToggleSoundCommand).AddTo(_disposables);
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _mainMenuView.HideInstantly();

            var commands = new MainMenuCommands(
                _openConverterCommand,
                _openTicTacCommand,
                _openRoguelikeCommand,
                _settingsPopupCommand,
                _secondPopupCommand,
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

        private void OnConverterCommand() => _openNewModuleCommand.Execute(ModulesMap.Converter);
        private void OnTicTacCommand() => _openNewModuleCommand.Execute(ModulesMap.TicTac);
        private void OnPlayground3DCommand() => _openNewModuleCommand.Execute(ModulesMap.Playground3D);
        private void OnSettingsPopupCommand() => _popupHub.OpenSettingsPopup();
        private void OnSecondPopupCommand() => _popupHub.OpenSecondPopup();
        private void OnToggleSoundCommand(bool isOn) => _audioSystem.SetMusicVolume(isOn ? 1 : 0);
    }
}