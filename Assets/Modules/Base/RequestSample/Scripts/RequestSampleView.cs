using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Unit = R3.Unit;

namespace Modules.Base.RequestSampleModule.Scripts
{
    /// <summary>
    /// Commands structure for RequestSample module UI interactions
    /// </summary>
    public readonly struct RequestSampleCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;
        public readonly ReactiveCommand<Unit> MakeRequestCommand;

        public RequestSampleCommands(
            ReactiveCommand<Unit> openMainMenuCommand,
            ReactiveCommand<Unit> makeRequestCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
            MakeRequestCommand = makeRequestCommand;
        }
    }
    
    /// <summary>
    /// View for RequestSample module - makes HTTP request and displays response
    /// </summary>
    public class RequestSampleView : BaseView
    {
        [Header("UI Elements")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button makeRequestButton;
        
        [Header("Display")]
        [SerializeField] private TMP_Text urlText;
        [SerializeField] private TMP_Text responseText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject loadingIndicator;
        
        private InputSystemService _inputSystemService;

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
            ValidateUIElements();
            #endif
        }

        public void SetupEventListeners(RequestSampleCommands commands)
        {
            _inputSystemService.SwitchToUI();
            
            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);

            makeRequestButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.MakeRequestCommand.Execute(default))
                .AddTo(this);
            
            var openMainMenuPerformedObservable =
                _inputSystemService.GetPerformedObservable(_inputSystemService.InputActions.UI.Cancel);

            openMainMenuPerformedObservable
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenMainMenuCommand.Execute(default))
                .AddTo(this);
        }

        public override async UniTask Show()
        {
            await base.Show();
            _inputSystemService.SwitchToUI();
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        public void SetUrl(string url)
        {
            if (urlText)
                urlText.text = $"URL: {url}";
        }

        public void SetResponse(string response)
        {
            if (responseText)
                responseText.text = response;
        }

        public void SetStatus(string status, bool isSuccess)
        {
            if (!statusText) return;
            
            statusText.text = status;
            statusText.color = isSuccess ? Color.green : Color.red;
        }

        public void SetLoading(bool isLoading)
        {
            if (loadingIndicator)
                loadingIndicator.SetActive(isLoading);
            
            if (makeRequestButton)
                makeRequestButton.interactable = !isLoading;
        }

        public void OnScreenEnabled() => _inputSystemService.SetFirstSelectedObject(makeRequestButton);

        private void ValidateUIElements()
        {
            if (!exitButton) 
                Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(RequestSampleView)}");
            if (!makeRequestButton) 
                Debug.LogError($"{nameof(makeRequestButton)} is not assigned in {nameof(RequestSampleView)}");
            if (!urlText) 
                Debug.LogError($"{nameof(urlText)} is not assigned in {nameof(RequestSampleView)}");
            if (!responseText) 
                Debug.LogError($"{nameof(responseText)} is not assigned in {nameof(RequestSampleView)}");
            if (!statusText) 
                Debug.LogError($"{nameof(statusText)} is not assigned in {nameof(RequestSampleView)}");
        }
    }
}