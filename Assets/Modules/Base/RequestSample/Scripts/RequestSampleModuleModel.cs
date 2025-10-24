using System;
using CodeBase.Core.Patterns.Architecture.MVP;
using CodeBase.Services;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Base.RequestSampleModule.Scripts
{
    /// <summary>
    /// Model for RequestSample module that contains business logic and data
    /// </summary>
    public class RequestSampleModuleModel : IModel
    {
        private readonly HttpRequestService _httpRequestService;
        private RequestConfig _config;

        public int CommandThrottleDelay => 300;
        public int ModuleTransitionThrottleDelay => 500;
        
        public string RequestUrl => _config?.requestUrl ?? "https://example.com";

        public RequestSampleModuleModel()
        {
            _httpRequestService = new HttpRequestService();
            LoadConfiguration();
        }

        /// <summary>
        /// Loads configuration from Resources/RequestConfig.json
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                var configText = Resources.Load<TextAsset>("RequestConfig");
                if (configText)
                {
                    _config = JsonConvert.DeserializeObject<RequestConfig>(configText.text);
                    Debug.Log($"[RequestSampleModuleModel] Config loaded: {_config.requestUrl}");
                }
                else
                {
                    Debug.LogWarning("[RequestSampleModuleModel] RequestConfig.json not found in Resources, using defaults");
                    _config = new RequestConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RequestSampleModuleModel] Failed to load config: {ex.Message}");
                _config = new RequestConfig();
            }
        }

        /// <summary>
        /// Makes HTTP GET request to configured URL
        /// </summary>
        public async UniTask<HttpResponse> MakeRequestAsync()
        {
            if (_config == null)
            {
                return new HttpResponse 
                { 
                    IsSuccess = false, 
                    Error = "Configuration not loaded" 
                };
            }

            return await _httpRequestService.GetAsync(_config.requestUrl, _config.timeout);
        }

        public void Dispose() { }
    }
}