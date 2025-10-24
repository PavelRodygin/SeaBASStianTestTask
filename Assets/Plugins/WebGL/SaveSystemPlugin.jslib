mergeInto(LibraryManager.library, {
  // Database name and store name
  SaveSystem_Init: function() {
    window.unityIndexedDB = {
      dbName: 'UnityGameSaveData',
      storeName: 'saves',
      db: null
    };
    
    console.log('[SaveSystem] IndexedDB initialized');
  },
  
  SaveSystem_Write: function(dataPtr) {
    var data = UTF8ToString(dataPtr);
    
    console.log('[SaveSystem] Writing data, length: ' + data.length);
    
    return new Promise(function(resolve, reject) {
      var request = indexedDB.open(window.unityIndexedDB.dbName, 1);
      
      request.onerror = function() {
        console.error('[SaveSystem] Database open error');
        reject(request.error);
      };
      
      request.onsuccess = function() {
        console.log('[SaveSystem] Database opened successfully');
        var db = request.result;
        var transaction = db.transaction([window.unityIndexedDB.storeName], 'readwrite');
        var store = transaction.objectStore(window.unityIndexedDB.storeName);
        
        var putRequest = store.put({ id: 'saveData', data: data });
        
        putRequest.onsuccess = function() {
          console.log('[SaveSystem] Data written successfully');
          resolve();
        };
        
        putRequest.onerror = function() {
          console.error('[SaveSystem] Write error: ' + putRequest.error);
          reject(putRequest.error);
        };
      };
      
      request.onupgradeneeded = function(event) {
        console.log('[SaveSystem] Creating object store');
        var db = event.target.result;
        if (!db.objectStoreNames.contains(window.unityIndexedDB.storeName)) {
          db.createObjectStore(window.unityIndexedDB.storeName, { keyPath: 'id' });
        }
      };
    });
  },
  
  SaveSystem_Read: function(callbackGameObjectPtr, callbackMethodPtr) {
    var callbackGameObject = UTF8ToString(callbackGameObjectPtr);
    var callbackMethod = UTF8ToString(callbackMethodPtr);
    
    console.log('[SaveSystem] Reading data...');
    
    var request = indexedDB.open(window.unityIndexedDB.dbName, 1);
    
    request.onerror = function() {
      console.error('[SaveSystem] Database open error: ' + request.error);
      SendMessage(callbackGameObject, callbackMethod, '');
    };
    
    request.onsuccess = function() {
      console.log('[SaveSystem] Database opened for reading');
      var db = request.result;
      
      if (!db.objectStoreNames.contains(window.unityIndexedDB.storeName)) {
        console.log('[SaveSystem] No save data found');
        SendMessage(callbackGameObject, callbackMethod, '');
        return;
      }
      
      var transaction = db.transaction([window.unityIndexedDB.storeName], 'readonly');
      var store = transaction.objectStore(window.unityIndexedDB.storeName);
      var getRequest = store.get('saveData');
      
      getRequest.onsuccess = function() {
        if (getRequest.result && getRequest.result.data) {
          console.log('[SaveSystem] Data read successfully, length: ' + getRequest.result.data.length);
          SendMessage(callbackGameObject, callbackMethod, getRequest.result.data);
        } else {
          console.log('[SaveSystem] No save data found');
          SendMessage(callbackGameObject, callbackMethod, '');
        }
      };
      
      getRequest.onerror = function() {
        console.error('[SaveSystem] Read error: ' + getRequest.error);
        SendMessage(callbackGameObject, callbackMethod, '');
      };
    };
    
    request.onupgradeneeded = function(event) {
      console.log('[SaveSystem] Creating object store on first read');
      var db = event.target.result;
      if (!db.objectStoreNames.contains(window.unityIndexedDB.storeName)) {
        db.createObjectStore(window.unityIndexedDB.storeName, { keyPath: 'id' });
      }
    };
  },
  
  SaveSystem_Delete: function() {
    console.log('[SaveSystem] Deleting all data...');
    
    var request = indexedDB.deleteDatabase(window.unityIndexedDB.dbName);
    
    request.onsuccess = function() {
      console.log('[SaveSystem] Database deleted successfully');
    };
    
    request.onerror = function() {
      console.error('[SaveSystem] Delete error: ' + request.error);
    };
  }
});

