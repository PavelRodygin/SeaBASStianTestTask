using System.Collections.Generic;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.Save;
using CodeBase.Services.AppEvent;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace CodeBase.Systems.Save
{
	//TODO Setup Dispose for events

	public class SaveSystem : IStartable, ISaveSystem
	{
		private readonly List<ISerializableDataSystem> _serializableDataSystems = new();
		private readonly IDataFileLoader _dataFileLoader;

		private SerializableDataContainer _serializableDataContainer;
		private bool _isLoaded;

		public SaveSystem(IAppEventService appEventsService, IDataFileLoader dataFileLoader)
		{
			_dataFileLoader = dataFileLoader;
			appEventsService.OnApplicationFocusEvent += SaveDataOnApplicationUnfocus;
			
			Debug.Log($"[SaveSystem] Initialized with loader: {dataFileLoader.GetType().Name}");
		}

		public void Start() => Initialize().Forget();

		private async UniTaskVoid Initialize()
		{
			Debug.Log("[SaveSystem] Starting initialization...");
			
			_serializableDataContainer = await _dataFileLoader.Read();
			if (_serializableDataContainer == null)
			{
				Debug.Log("[SaveSystem] No save data found, creating new container");
				_serializableDataContainer = new SerializableDataContainer();
			}
			else
			{
				Debug.Log("[SaveSystem] Save data loaded successfully");
			}

			foreach(var settingsSystem in _serializableDataSystems)
			{
				await settingsSystem.LoadData(_serializableDataContainer);
			}

			_isLoaded = true;
			Debug.Log("[SaveSystem] Initialization complete");
		}

		public void AddSystem(ISerializableDataSystem serializableDataSystem) 
		{
			_serializableDataSystems.Add(serializableDataSystem);
			if (_isLoaded)
			{
				serializableDataSystem.LoadData(_serializableDataContainer).Forget();
			}
		}

		public async UniTaskVoid SaveData()
		{
			Debug.Log($"[SaveSystem] Saving data for {_serializableDataSystems.Count} systems...");
			
			foreach(var serializableDataSystem in _serializableDataSystems) 
				serializableDataSystem.SaveData(_serializableDataContainer);

			await _dataFileLoader.Write(_serializableDataContainer);
			
			Debug.Log("[SaveSystem] Save complete");
		}

		private void SaveDataOnApplicationUnfocus(bool isFocused)
		{
			if(!isFocused)
			{
				Debug.Log("[SaveSystem] Application unfocused, triggering save");
				SaveData().Forget();
			}
		}
	}
}