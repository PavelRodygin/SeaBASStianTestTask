using System;

namespace CodeBase.Services.AppEvent
{
    public interface IAppEventService
    {
        public event Action<bool> OnApplicationFocusEvent;
        public event Action<bool> OnApplicationPauseEvent;
        public event Action OnApplicationQuitEvent;
    }
}