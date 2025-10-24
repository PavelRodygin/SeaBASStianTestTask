using System;
using UnityEngine;

namespace CodeBase.Services.AppEvent
{
	public class Andoird_PC_AppEventService: MonoBehaviour, IAppEventService
	{
		public event Action<bool> OnApplicationFocusEvent;
		public event Action<bool> OnApplicationPauseEvent;
		public event Action OnApplicationQuitEvent;
		
		private void OnApplicationFocus(bool focus)
		{
			OnApplicationFocusEvent?.Invoke(focus);
		}

		private void OnApplicationPause(bool pause)
		{
			OnApplicationPauseEvent?.Invoke(pause);
		}

		private void OnApplicationQuit()
		{
			OnApplicationQuitEvent?.Invoke();
		}
	}
}