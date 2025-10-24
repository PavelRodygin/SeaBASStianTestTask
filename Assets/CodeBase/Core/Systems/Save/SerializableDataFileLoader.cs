using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeBase.Core.Systems.Save
{
	public class SerializableDataFileLoader : IDataFileLoader
	{
		// Using a temporary file for atomic operation
		private const string SaveFileName = "save.json";
		private const string TempSaveFileName = "temp_save.json";

		private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
		{
			ObjectCreationHandling = ObjectCreationHandling.Replace,
		};
    
		private readonly string _saveFilePath;
		private readonly string _tempSaveFilePath;

		public SerializableDataFileLoader()
		{
			_saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);
			_tempSaveFilePath = Path.Combine(Application.persistentDataPath, TempSaveFileName);
		}

	public async UniTask Write(SerializableDataContainer dataContainer) 
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL doesn't support threading, run synchronously on main thread
		await UniTask.Yield();
		WriteInternal(dataContainer);
#else
		await UniTask.RunOnThreadPool(() => WriteInternal(dataContainer));
#endif
	}

	public async UniTask<SerializableDataContainer> Read() 
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL doesn't support threading, run synchronously on main thread
		await UniTask.Yield();
		return ReadInternal();
#else
		return await UniTask.RunOnThreadPool(ReadInternal);
#endif
	}

		private void WriteInternal(SerializableDataContainer dataContainer) 
		{
			var serializedData = JsonConvert.SerializeObject(dataContainer, Formatting.Indented, 
				_serializerSettings);
			File.WriteAllText(_tempSaveFilePath, serializedData);
			File.Copy(_tempSaveFilePath, _saveFilePath, true);
			File.Delete(_tempSaveFilePath);
		}

		private SerializableDataContainer ReadInternal() 
		{
			try
			{
				if(!File.Exists(_saveFilePath))
				{
					return null;
				}

				var serializedData = File.ReadAllText(_saveFilePath);
				return JsonConvert.DeserializeObject<SerializableDataContainer>(serializedData, _serializerSettings);
			}
			catch(Exception)
			{
				return null;
			}      
		}
	}
}