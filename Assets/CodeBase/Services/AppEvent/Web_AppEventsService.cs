using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CodeBase.Services.AppEvent
{
    public class Web_AppEventsService : MonoBehaviour, IAppEventService
    {
        public event Action<bool> OnApplicationFocusEvent;
        public event Action<bool> OnApplicationPauseEvent;
        public event Action OnApplicationQuitEvent;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void InitAppEvents(string gameObjectName);
#endif

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            InitAppEvents(gameObject.name);
#endif
        }

        public void OnFocus(int focus)
        {
            bool hasFocus = focus == 1;
            OnApplicationFocusEvent?.Invoke(hasFocus);
        }

        public void OnPause(int pause)
        {
            bool isPaused = pause == 1;
            OnApplicationPauseEvent?.Invoke(isPaused);
        }

        public void OnQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }
    }
}