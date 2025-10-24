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
            Debug.Log($"[Web_AppEventsService] Starting initialization on GameObject: {gameObject.name}");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            InitAppEvents(gameObject.name);
            Debug.Log($"[Web_AppEventsService] InitAppEvents called for: {gameObject.name}");
#else
            Debug.Log("[Web_AppEventsService] Running in Editor, WebGL events disabled");
#endif
        }

        public void OnFocus(int focus)
        {
            bool hasFocus = focus == 1;
            Debug.Log($"[Web_AppEventsService] OnFocus: {hasFocus}");
            OnApplicationFocusEvent?.Invoke(hasFocus);
        }

        public void OnPause(int pause)
        {
            bool isPaused = pause == 1;
            Debug.Log($"[Web_AppEventsService] OnPause: {isPaused}");
            OnApplicationPauseEvent?.Invoke(isPaused);
        }

        public void OnQuit()
        {
            Debug.Log("[Web_AppEventsService] OnQuit");
            OnApplicationQuitEvent?.Invoke();
        }
    }
}