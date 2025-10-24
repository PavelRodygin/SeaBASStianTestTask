using System;
using System.Collections.Generic;
using CodeBase.Core.Patterns.Architecture.MVP;
using CodeBase.Services;
using Cysharp.Threading.Tasks;

namespace Modules.Additional.SplashScreen.Scripts
{
    public class SplashScreenModel : IModel
    {
        private readonly LoadingServiceProvider _loadingServiceProvider;
        
        public Dictionary<string, Func<UniTask>> Commands = new();

        public SplashScreenModel(LoadingServiceProvider loadingServiceProvider) => 
            _loadingServiceProvider = loadingServiceProvider;

        public async UniTask WaitForTheEndOfRegistration()
        {
            await _loadingServiceProvider.WaitForRegistration;
            Commands = _loadingServiceProvider.Commands;
        }

        public void Dispose() { }
    }
}