using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.TimerSampleModule.Scripts
{
    /// <summary>
    /// Controller for TimerSample module - manages timer update loop
    /// </summary>
    public class TimerSampleModuleController : IModuleController
    {
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly TimerSampleModuleModel _timerSampleModuleModel;
        private readonly TimerSamplePresenter _timerSamplePresenter;
        private readonly IModuleStateMachine _moduleStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public TimerSampleModuleController(IModuleStateMachine moduleStateMachine, TimerSampleModuleModel timerSampleModuleModel, 
            TimerSamplePresenter timerSamplePresenter)
        {
            _timerSampleModuleModel = timerSampleModuleModel ?? throw new ArgumentNullException(nameof(timerSampleModuleModel));
            _timerSamplePresenter = timerSamplePresenter ?? throw new ArgumentNullException(nameof(timerSamplePresenter));
            _moduleStateMachine = moduleStateMachine ?? throw new ArgumentNullException(nameof(moduleStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();
            SubscribeToUpdateLoop();

            _timerSamplePresenter.HideInstantly();
            
            await _timerSamplePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _timerSamplePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _timerSamplePresenter.Dispose();
            
            _timerSampleModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_timerSampleModuleModel.ModuleTransitionThrottleDelay))
                .Subscribe(RunNewModule)
                .AddTo(_disposables);
        }

        private void SubscribeToUpdateLoop()
        {
            // Update timer every frame
            Observable.EveryUpdate()
                .Subscribe(_ => _timerSamplePresenter.Update())
                .AddTo(_disposables);
        }

        private void RunNewModule(ModulesMap screen)
        {
            _moduleCompletionSource.TrySetResult();
            _moduleStateMachine.RunModule(screen);
        }
    }
}
