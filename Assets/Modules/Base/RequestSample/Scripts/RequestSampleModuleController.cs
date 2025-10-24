using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.RequestSampleModule.Scripts
{
    public class RequestSampleModuleController : IModuleController
    {
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly RequestSampleModuleModel _requestSampleModuleModel;
        private readonly RequestSamplePresenter _requestSamplePresenter;
        private readonly IModuleStateMachine _moduleStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public RequestSampleModuleController(IModuleStateMachine moduleStateMachine, RequestSampleModuleModel requestSampleModuleModel, 
            RequestSamplePresenter requestSamplePresenter)
        {
            _requestSampleModuleModel = requestSampleModuleModel ?? throw new ArgumentNullException(nameof(requestSampleModuleModel));
            _requestSamplePresenter = requestSamplePresenter ?? throw new ArgumentNullException(nameof(requestSamplePresenter));
            _moduleStateMachine = moduleStateMachine ?? throw new ArgumentNullException(nameof(moduleStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _requestSamplePresenter.HideInstantly();
            
            await _requestSamplePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _requestSamplePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _requestSamplePresenter.Dispose();
            
            _requestSampleModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_requestSampleModuleModel.ModuleTransitionThrottleDelay))
                .Subscribe(RunNewModule)
                .AddTo(_disposables);
        }

        private void RunNewModule(ModulesMap screen)
        {
            _moduleCompletionSource.TrySetResult();
            _moduleStateMachine.RunModule(screen);
        }
    }
}
