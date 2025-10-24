using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeBase.Services.LongInitializationServices
{
    public abstract class LongInitializationService
    {
        private bool _isInitialized;
        protected static int DelayTime = 1;

        public async Task Init()   
        {
            if (!_isInitialized)
                await InitializeAsync();
            else
                Console.WriteLine("LongInitializationService is already initialized.");
        }

        private async Task InitializeAsync()
        {
            // Use UniTask.Delay instead of Task.Delay for better WebGL compatibility
            await UniTask.Delay(TimeSpan.FromSeconds(DelayTime));
            _isInitialized = true;
        }
    }
}