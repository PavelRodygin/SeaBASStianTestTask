using System;
using System.Text;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Unit = R3.Unit;

namespace Modules.Base.TimerSampleModule.Scripts
{
    /// <summary>
    /// Presenter for TimerSample module - updates time display every frame without allocations
    /// Uses StringBuilder to avoid string allocation on each update
    /// </summary>
    public class TimerSamplePresenter : IDisposable
    {
        private readonly TimerSampleModuleModel _timerSampleModuleModel;
        private readonly TimerSampleView _timerSampleView;
        
        private readonly CompositeDisposable _disposables = new();
        private readonly StringBuilder _timeStringBuilder = new StringBuilder(12); // "HH:mm:ss.fff" = 12 chars
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        
        private bool _isActive;

        public TimerSamplePresenter(
            TimerSampleModuleModel timerSampleModuleModel,
            TimerSampleView timerSampleView)
        {
            _timerSampleModuleModel = timerSampleModuleModel ?? throw new ArgumentNullException(nameof(timerSampleModuleModel));
            _timerSampleView = timerSampleView ?? throw new ArgumentNullException(nameof(timerSampleView));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _timerSampleView.HideInstantly();

            var commands = new TimerSampleCommands(_openMainMenuCommand);
            _timerSampleView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            await _timerSampleView.Show();
            
            _isActive = true;
        }

        public async UniTask Exit()
        {
            _isActive = false;
            await _timerSampleView.Hide();
        }
        
        public void HideInstantly()
        {
            _isActive = false;
            _timerSampleView.HideInstantly();
        }

        public void Update()
        {
            if (!_isActive) return;
            
            UpdateTimeDisplay();
        }

        public void Dispose()
        {
            _isActive = false;
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_timerSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }

        private void UpdateTimeDisplay()
        {
            var now = DateTime.Now;
            
            // Clear StringBuilder without allocation
            _timeStringBuilder.Clear();
            
            // Build time string: HH:mm:ss.fff
            AppendTwoDigits(_timeStringBuilder, now.Hour);
            _timeStringBuilder.Append(':');
            AppendTwoDigits(_timeStringBuilder, now.Minute);
            _timeStringBuilder.Append(':');
            AppendTwoDigits(_timeStringBuilder, now.Second);
            _timeStringBuilder.Append('.');
            AppendThreeDigits(_timeStringBuilder, now.Millisecond);
            
            _timerSampleView.UpdateTimeText(_timeStringBuilder.ToString());
        }

        private static void AppendTwoDigits(StringBuilder sb, int value)
        {
            if (value < 10)
                sb.Append('0');
            sb.Append(value);
        }

        private static void AppendThreeDigits(StringBuilder sb, int value)
        {
            if (value < 10)
            {
                sb.Append("00");
            }
            else if (value < 100)
            {
                sb.Append('0');
            }
            sb.Append(value);
        }
    }
}
