using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
        /// Main controller for ScrollSample module that manages the module lifecycle
        /// and coordinates between Presenter, Model and View
        /// 
        /// IMPORTANT: This is a scrollSample file for ModuleCreator system.
        /// When creating a new module, this file will be copied and modified.
        /// 
        /// Key points for customization:
        /// 1. Change class name from ScrollSampleModuleController to YourModuleNameModuleController
        /// 2. Update namespace Modules.Base.ScrollSampleModule.Scripts match your module location
        /// 3. Customize module lifecycle management if needed
        /// 4. Add specific initialization logic for your module
        /// 5. Implement custom exit conditions if required
    /// </summary>
    public class ScrollSampleModuleController : IModuleController
    {
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly ScrollSampleModuleModel _scrollSampleModuleModel;
        private readonly ScrollSamplePresenter _scrollSamplePresenter;
        private readonly IModuleStateMachine _moduleStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public ScrollSampleModuleController(IModuleStateMachine moduleStateMachine, ScrollSampleModuleModel scrollSampleModuleModel, 
            ScrollSamplePresenter scrollSamplePresenter)
        {
            _scrollSampleModuleModel = scrollSampleModuleModel ?? throw new ArgumentNullException(nameof(scrollSampleModuleModel));
            _scrollSamplePresenter = scrollSamplePresenter ?? throw new ArgumentNullException(nameof(scrollSamplePresenter));
            _moduleStateMachine = moduleStateMachine ?? throw new ArgumentNullException(nameof(moduleStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _scrollSamplePresenter.HideInstantly();
            
            await _scrollSamplePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _scrollSamplePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _scrollSamplePresenter.Dispose();
            
            _scrollSampleModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_scrollSampleModuleModel.ModuleTransitionThrottleDelay))
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
