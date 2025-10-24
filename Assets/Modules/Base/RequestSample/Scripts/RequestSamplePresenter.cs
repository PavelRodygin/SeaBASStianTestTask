using System;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.RequestSampleModule.Scripts
{
    /// <summary>
    /// Presenter for RequestSample module - coordinates between Model and View
    /// </summary>
    public class RequestSamplePresenter : IDisposable
    {
        private readonly RequestSampleModuleModel _model;
        private readonly RequestSampleView _view;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        private readonly ReactiveCommand<Unit> _makeRequestCommand = new();
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly CompositeDisposable _disposables = new();
        
        public RequestSamplePresenter(
            RequestSampleModuleModel requestSampleModuleModel,
            RequestSampleView requestSampleView)
        {
            _model = requestSampleModuleModel ?? throw new ArgumentNullException(nameof(requestSampleModuleModel));
            _view = requestSampleView ?? throw new ArgumentNullException(nameof(requestSampleView));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _view.HideInstantly();

            var commands = new RequestSampleCommands(_openMainMenuCommand, _makeRequestCommand);
            _view.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            _view.SetUrl(_model.RequestUrl);
            _view.SetStatus("Ready to make request", true);
            _view.SetResponse("Click 'Make Request' button to send HTTP GET request");
            
            await _view.Show();
        }

        public async UniTask Exit()
        {
            await _view.Hide();
        }
        
        public void HideInstantly() => _view.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_model.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            _makeRequestCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_model.CommandThrottleDelay))
                .Subscribe(_ => OnMakeRequestButtonClicked().Forget())
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked() => 
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);

        private async UniTaskVoid OnMakeRequestButtonClicked()
        {
            _view.SetLoading(true);
            _view.SetStatus("Loading...", true);
            _view.SetResponse("Sending request...");

            try
            {
                var response = await _model.MakeRequestAsync();

                if (response.IsSuccess)
                {
                    _view.SetStatus($"Success! Status: {response.StatusCode}", true);
                    
                    var displayText = response.ResponseText;
                    if (displayText.Length > 2000)
                        displayText = displayText.Substring(0, 2000) + "\n\n... (truncated)";
                    
                    _view.SetResponse(displayText);
                }
                else
                {
                    _view.SetStatus($"Error: {response.Error}", false);
                    _view.SetResponse(response.ResponseText ?? "No response body");
                }
            }
            catch (Exception ex)
            {
                _view.SetStatus($"Exception: {ex.Message}", false);
                _view.SetResponse(ex.ToString());
            }
            finally
            {
                _view.SetLoading(false);
            }
        }
    }
}
