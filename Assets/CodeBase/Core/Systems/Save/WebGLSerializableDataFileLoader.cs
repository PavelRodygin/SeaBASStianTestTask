using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeBase.Core.Systems.Save
{
    public class WebGLSerializableDataFileLoader : MonoBehaviour, IDataFileLoader
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        private UniTaskCompletionSource<SerializableDataContainer> _readCompletionSource;
        private UniTaskCompletionSource _writeCompletionSource;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SaveSystem_Init();
        
        [DllImport("__Internal")]
        private static extern void SaveSystem_Write(string data);
        
        [DllImport("__Internal")]
        private static extern void SaveSystem_Read(string gameObjectName, string callbackMethodName);
        
        [DllImport("__Internal")]
        private static extern void SaveSystem_Delete();
#endif

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SaveSystem_Init();
            Debug.Log("[WebGLSerializableDataFileLoader] IndexedDB initialized");
#else
            Debug.LogWarning("[WebGLSerializableDataFileLoader] This component is designed for WebGL only. Use SerializableDataFileLoader for other platforms.");
#endif
        }

        public async UniTask Write(SerializableDataContainer dataContainer)
        {
            Debug.Log("[WebGLSerializableDataFileLoader] Starting write operation");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                var serializedData = JsonConvert.SerializeObject(dataContainer, Formatting.None, _serializerSettings);
                Debug.Log($"[WebGLSerializableDataFileLoader] Serialized data length: {serializedData.Length}");
                
                _writeCompletionSource = new UniTaskCompletionSource();
                SaveSystem_Write(serializedData);
                
                // Give IndexedDB time to complete the operation
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
                _writeCompletionSource?.TrySetResult();
                
                Debug.Log("[WebGLSerializableDataFileLoader] Write completed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebGLSerializableDataFileLoader] Write error: {ex.Message}");
                _writeCompletionSource?.TrySetException(ex);
                throw;
            }
#else
            await UniTask.Yield();
            Debug.LogWarning("[WebGLSerializableDataFileLoader] Write called in non-WebGL build");
#endif
        }

        public async UniTask<SerializableDataContainer> Read()
        {
            Debug.Log("[WebGLSerializableDataFileLoader] Starting read operation");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                _readCompletionSource = new UniTaskCompletionSource<SerializableDataContainer>();
                SaveSystem_Read(gameObject.name, nameof(OnReadComplete));
                
                var result = await _readCompletionSource.Task;
                Debug.Log($"[WebGLSerializableDataFileLoader] Read completed, result is null: {result == null}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebGLSerializableDataFileLoader] Read error: {ex.Message}");
                return null;
            }
#else
            await UniTask.Yield();
            Debug.LogWarning("[WebGLSerializableDataFileLoader] Read called in non-WebGL build");
            return null;
#endif
        }

        public void DeleteAllData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SaveSystem_Delete();
            Debug.Log("[WebGLSerializableDataFileLoader] All data deleted");
#else
            Debug.LogWarning("[WebGLSerializableDataFileLoader] Delete called in non-WebGL build");
#endif
        }

        // Called from JavaScript
        public void OnReadComplete(string jsonData)
        {
            Debug.Log($"[WebGLSerializableDataFileLoader] OnReadComplete called, data length: {jsonData?.Length ?? 0}");
            
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.Log("[WebGLSerializableDataFileLoader] No save data found, returning null");
                    _readCompletionSource?.TrySetResult(null);
                    return;
                }

                var container = JsonConvert.DeserializeObject<SerializableDataContainer>(jsonData, _serializerSettings);
                Debug.Log("[WebGLSerializableDataFileLoader] Data deserialized successfully");
                _readCompletionSource?.TrySetResult(container);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebGLSerializableDataFileLoader] Deserialization error: {ex.Message}");
                _readCompletionSource?.TrySetResult(null);
            }
        }
    }
}

