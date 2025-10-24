using System;
using System.Threading.Tasks;
using CodeBase.Core.Patterns.Architecture.MVP;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Modules.Additional.SplashScreen.Scripts
{
    public class SplashPresenter : IPresenter
    {
        private readonly SplashView _splashView;
        private readonly SplashScreenModel _splashScreenModel;
        private readonly TaskCompletionSource<bool> _screenCompletionSource;
        
        private readonly Subject<Unit> _servicesLoaded = new();
        private readonly ReactiveProperty<string> _progressStatus = new(string.Empty);
        private readonly ReactiveProperty<float> _exponentialProgress = new(0f);

        private ReadOnlyReactiveProperty<string> ProgressStatus => 
            _progressStatus.ToReadOnlyReactiveProperty();
        private ReadOnlyReactiveProperty<float> ExponentialProgress => 
            _exponentialProgress.ToReadOnlyReactiveProperty();
       
        public Observable<Unit> ServicesLoaded => _servicesLoaded;

        public SplashPresenter(SplashView splashView, SplashScreenModel splashScreenModel)
        {
            Debug.Log("SplashScreenPresenter was born");
            _splashView = splashView;
            _splashScreenModel = splashScreenModel;
            _screenCompletionSource = new TaskCompletionSource<bool>();
        }
        
        public async UniTask Enter(object param)
        {
            Debug.Log("SplashScreenPresenter entered");
            InitializeUI();
            await _splashView.Show();
            await _splashScreenModel.WaitForTheEndOfRegistration();
            Debug.Log("Registration Complete");
            await LoadDataForServices();
        }


        public async UniTask Exit() => await _splashView.Hide();

        private void InitializeUI()
        {
            //_splashView.HideInstantly();
            _splashView.SetupEventListeners(ProgressStatus, ExponentialProgress);
        }
        
        private async UniTask LoadDataForServices()
        {
            var timing = 1f / _splashScreenModel.Commands.Count;
            var currentTiming = timing;
            
            foreach (var (serviceName, initFunction) in _splashScreenModel.Commands)
            {
                _progressStatus.Value = $"Loading: {serviceName}";
                _exponentialProgress.Value = CalculateExponentialProgress(currentTiming);
                currentTiming += timing;
                
                await initFunction.Invoke();
            }
            
            _screenCompletionSource.SetResult(true);
            _servicesLoaded.OnNext(Unit.Default);
        }

        private static float CalculateExponentialProgress(float progress)
        {
            var expValue = Math.Exp(progress);
            var minExp = Math.Exp(0);
            var maxExp = Math.Exp(1);
            return (float)((expValue - minExp) / (maxExp - minExp));
        }

        public void Dispose()
        {
            
        }
    }
}