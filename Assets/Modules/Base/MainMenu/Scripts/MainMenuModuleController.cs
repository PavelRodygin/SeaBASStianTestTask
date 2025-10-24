using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuModuleController : IModuleController
    {
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly MainMenuModuleModel _mainMenuModuleModel;
        private readonly MainMenuPresenter _mainMenuPresenter;
        private readonly IModuleStateMachine _moduleStateMachine;
        
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public MainMenuModuleController(IModuleStateMachine moduleStateMachine, MainMenuModuleModel mainMenuModuleModel, 
            MainMenuPresenter mainMenuPresenter)
        {
            _mainMenuModuleModel = mainMenuModuleModel;
            _mainMenuPresenter = mainMenuPresenter;
            _moduleStateMachine = moduleStateMachine;
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _mainMenuPresenter.HideInstantly();
            
            await _mainMenuPresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _mainMenuPresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _mainMenuPresenter.Dispose();
            _mainMenuModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.ModuleTransitionThrottleDelay))
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