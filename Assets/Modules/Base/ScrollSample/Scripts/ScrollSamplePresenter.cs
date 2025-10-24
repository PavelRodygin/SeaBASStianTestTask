using System;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Presenter for ScrollSample module - manages virtualized scroll list with 1000 items
    /// </summary>
    public class ScrollSamplePresenter : IDisposable
    {
        private readonly ScrollSampleModuleModel _scrollSampleModuleModel;
        private readonly ScrollSampleView _scrollSampleView;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();

        public ScrollSamplePresenter(
            ScrollSampleModuleModel scrollSampleModuleModel,
            ScrollSampleView scrollSampleView)
        {
            _scrollSampleModuleModel = scrollSampleModuleModel ?? throw new ArgumentNullException(nameof(scrollSampleModuleModel));
            _scrollSampleView = scrollSampleView ?? throw new ArgumentNullException(nameof(scrollSampleView));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _scrollSampleView.HideInstantly();

            var commands = new ScrollSampleCommands(_openMainMenuCommand);
            _scrollSampleView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            // Initialize scroll with 1000 items
            _scrollSampleView.InitializeScroll(1000);
            
            await _scrollSampleView.Show();
        }

        public async UniTask Exit()
        {
            await _scrollSampleView.Hide();
        }
        
        public void HideInstantly() => _scrollSampleView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_scrollSampleModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }
    }
}
