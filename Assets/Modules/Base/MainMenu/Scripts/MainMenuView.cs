﻿using CodeBase.Core.UI.Views;
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
        public readonly ReactiveCommand<Unit> OpenConverterCommand;
        public readonly ReactiveCommand<Unit> OpenTicTacCommand;
        public readonly ReactiveCommand<Unit> OpenRoguelikeCommand;
        public readonly ReactiveCommand<Unit> SettingsPopupCommand;
        public readonly ReactiveCommand<Unit> SecondPopupCommand;
        public readonly ReactiveCommand<bool> SoundToggleCommand;

        public MainMenuCommands(
            ReactiveCommand<Unit> openConverterCommand,
            ReactiveCommand<Unit> openTicTacCommand,
            ReactiveCommand<Unit> openRoguelikeCommand,
            ReactiveCommand<Unit> settingsPopupCommand,
            ReactiveCommand<Unit> secondPopupCommand,
            ReactiveCommand<bool> soundToggleCommand)
        {
            OpenConverterCommand = openConverterCommand;
            OpenTicTacCommand = openTicTacCommand;
            OpenRoguelikeCommand = openRoguelikeCommand;
            SettingsPopupCommand = settingsPopupCommand;
            SecondPopupCommand = secondPopupCommand;
            SoundToggleCommand = soundToggleCommand;
        }
    }
    
    public class MainMenuView : BaseView
    {
        [Header("Open Module Buttons")]
        [SerializeField] private Button converterButton;
        [SerializeField] private Button ticTacButton;
        [SerializeField] private Button roguelikeButton;
        
        [SerializeField] private Button settingsPopupButton;
        [SerializeField] private Button secondPopupButton;
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
            converterButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenConverterCommand.Execute(default))
                .AddTo(this);

            ticTacButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenTicTacCommand.Execute(default))
                .AddTo(this);

            roguelikeButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenRoguelikeCommand.Execute(default))
                .AddTo(this);

            settingsPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SettingsPopupCommand.Execute(default))
                .AddTo(this);

            secondPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SecondPopupCommand.Execute(default))
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
            _inputSystemService.SetFirstSelectedObject(converterButton);
        }
    }
}