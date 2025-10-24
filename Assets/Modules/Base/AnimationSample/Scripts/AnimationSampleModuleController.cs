using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.AnimationSampleModule.Scripts
{
    /// <summary>
        /// Main controller for AnimationSample module that manages the module lifecycle
        /// and coordinates between Presenter, Model and View
        /// 
        /// IMPORTANT: This is a animationSample file for ModuleCreator system.
        /// When creating a new module, this file will be copied and modified.
        /// 
        /// Key points for customization:
        /// 1. Change class name from AnimationSampleModuleController to YourModuleNameModuleController
        /// 2. Update namespace Modules.Base.AnimationSampleModule.Scripts match your module location
        /// 3. Customize module lifecycle management if needed
        /// 4. Add specific initialization logic for your module
        /// 5. Implement custom exit conditions if required
    /// </summary>
    public class AnimationSampleModuleController : IModuleController
    {
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly AnimationSampleModuleModel _animationSampleModuleModel;
        private readonly AnimationSamplePresenter _animationSamplePresenter;
        private readonly IModuleStateMachine _moduleStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public AnimationSampleModuleController(IModuleStateMachine moduleStateMachine, AnimationSampleModuleModel animationSampleModuleModel, 
            AnimationSamplePresenter animationSamplePresenter)
        {
            _animationSampleModuleModel = animationSampleModuleModel ?? throw new ArgumentNullException(nameof(animationSampleModuleModel));
            _animationSamplePresenter = animationSamplePresenter ?? throw new ArgumentNullException(nameof(animationSamplePresenter));
            _moduleStateMachine = moduleStateMachine ?? throw new ArgumentNullException(nameof(moduleStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _animationSamplePresenter.HideInstantly();
            
            await _animationSamplePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _animationSamplePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _animationSamplePresenter.Dispose();
            
            _animationSampleModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_animationSampleModuleModel.ModuleTransitionThrottleDelay))
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
