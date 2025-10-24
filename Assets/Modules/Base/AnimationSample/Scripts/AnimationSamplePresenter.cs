using System;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.AnimationSampleModule.Scripts
{
    /// <summary>
    /// Presenter for AnimationSample module - manages animated and static UI squares
    /// </summary>
    public class AnimationSamplePresenter : IDisposable
    {
        private readonly AnimationSampleModuleModel _animationSampleModuleModel;
        private readonly AnimationSampleView _animationSampleView;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();

        public AnimationSamplePresenter(
            AnimationSampleModuleModel animationSampleModuleModel,
            AnimationSampleView animationSampleView)
        {
            _animationSampleModuleModel = animationSampleModuleModel ?? throw new ArgumentNullException(nameof(animationSampleModuleModel));
            _animationSampleView = animationSampleView ?? throw new ArgumentNullException(nameof(animationSampleView));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _animationSampleView.HideInstantly();

            var commands = new AnimationSampleCommands(_openMainMenuCommand);
            _animationSampleView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            // Initialize squares (5 animated + 100 static)
            await _animationSampleView.InitializeSquares();
            
            await _animationSampleView.Show();
            
            // Start animations after view is shown
            await _animationSampleView.StartAnimations();
        }

        public async UniTask Exit()
        {
            _animationSampleView.StopAnimations();
            await _animationSampleView.Hide();
        }
        
        public void HideInstantly()
        {
            _animationSampleView.StopAnimations();
            _animationSampleView.HideInstantly();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_animationSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }
    }
}
