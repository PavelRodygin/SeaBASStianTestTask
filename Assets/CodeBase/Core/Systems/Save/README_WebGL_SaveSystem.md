# WebGL Save System

## Overview

This system provides cross-platform save/load functionality with specialized support for WebGL using IndexedDB.

## Architecture

### Components

1. **IDataFileLoader** - Interface for data persistence
2. **SerializableDataFileLoader** - Standard file system implementation (PC, Mobile, Editor)
3. **WebGLSerializableDataFileLoader** - IndexedDB implementation for WebGL
4. **SaveSystemPlugin.jslib** - JavaScript bridge to IndexedDB API

## Platform Support

- **PC/Mobile/Editor**: Uses `System.IO.File` for JSON file storage
- **WebGL**: Uses browser's `IndexedDB` for persistent storage

## Features

✅ Automatic platform detection  
✅ Asynchronous operations  
✅ Type-safe data container  
✅ Auto-save on focus loss  
✅ Detailed logging for debugging

## Setup

### 1. RootLifetimeScope Configuration

Add a reference to `WebGLSerializableDataFileLoader` component:

```csharp
[Header("Save System")]
[SerializeField] private WebGLSerializableDataFileLoader webGLDataFileLoader;
```

The system automatically selects the correct loader based on build platform.

### 2. Implementing Saveable Systems

Any system that needs to save data should implement `ISerializableDataSystem`:

```csharp
public class MySystem : ISerializableDataSystem
{
    public MySystem(ISaveSystem saveSystem)
    {
        saveSystem.AddSystem(this);
    }

    public UniTask LoadData(SerializableDataContainer dataContainer)
    {
        myValue = dataContainer.TryGet(nameof(myValue), out int value) ? value : defaultValue;
        return UniTask.CompletedTask;
    }

    public void SaveData(SerializableDataContainer dataContainer)
    {
        dataContainer.SetData(nameof(myValue), myValue);
    }
}
```

## WebGL Storage Details

### IndexedDB Database Structure

- **Database Name**: `UnityGameSaveData`
- **Store Name**: `saves`
- **Key**: `saveData`

### Browser Console Logs

Monitor save/load operations in browser console (F12):
- `[SaveSystem]` - C# side logs
- `[SaveSystem] Initializing...` - IndexedDB initialization
- `[SaveSystem] Writing data, length: X` - Save operation
- `[SaveSystem] Reading data...` - Load operation

## Testing

### In Editor
- System will log: `[RootLifetimeScope] Registering standard data file loader`
- Uses file system at `Application.persistentDataPath/save.json`

### In WebGL Build
- System will log: `[RootLifetimeScope] Registering WebGL data file loader`
- Uses browser's IndexedDB
- Check browser console for detailed JavaScript logs

### Testing Scenarios

1. **Save on Focus Loss**: Switch browser tab → check logs
2. **Persistence**: Reload page → verify data loads
3. **Multiple Systems**: Add multiple saveable systems → verify all save/load

## Browser DevTools

To inspect IndexedDB data:
1. Open DevTools (F12)
2. Go to "Application" or "Storage" tab
3. Expand "IndexedDB"
4. Find "UnityGameSaveData" → "saves" → "saveData"

## Troubleshooting

### No logs in browser console
- Check that WebGL build is being tested (not Editor)
- Verify `SaveSystemPlugin.jslib` is in `Assets/Plugins/WebGL/`
- Check browser console for JavaScript errors

### Data not persisting
- Verify browser allows IndexedDB (not in private/incognito mode)
- Check browser storage quota
- Clear site data and test fresh

### Save not triggering
- Verify systems implement `ISerializableDataSystem`
- Check that systems are registered via `AddSystem()`
- Look for `[SaveSystem]` logs in console

## Performance Notes

- IndexedDB operations are asynchronous
- Save operations use 100ms delay for IndexedDB completion
- No threading is used in WebGL (not supported)
- JSON serialization happens on main thread

## Future Improvements

- [ ] Add compression for large save files
- [ ] Implement save versioning/migration
- [ ] Add cloud save sync option
- [ ] Implement save slot system

