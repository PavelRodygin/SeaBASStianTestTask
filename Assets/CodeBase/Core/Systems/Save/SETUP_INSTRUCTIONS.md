# WebGL Save System - Quick Setup Guide

## 1. Unity Scene Setup

### Add WebGL Save Loader Component

1. Open scene with `RootLifetimeScope` GameObject
2. Add component: `WebGLSerializableDataFileLoader`
3. In `RootLifetimeScope` inspector:
   - Find "Save System" section
   - Drag the same GameObject (or the component) to `Web GL Data File Loader` field

![Setup Example](https://via.placeholder.com/600x200?text=RootLifetimeScope+Setup)

## 2. Build Settings

### WebGL Build
- Switch platform to WebGL
- Build and test in browser
- Open browser console (F12) to see logs

### Testing in Editor
- No additional setup needed
- Uses standard file system
- Use menu: `Tools > Save System > Log Save Data Path`

## 3. Verification

### Check Logs
When game starts, you should see:
```
[RootLifetimeScope] Registering WebGL data file loader  (WebGL build)
[RootLifetimeScope] Registering standard data file loader  (Editor/PC)
[SaveSystem] Initialized with loader: ...
[SaveSystem] Starting initialization...
```

### Test Save/Load
1. Change some settings (volume, graphics)
2. Switch browser tab (triggers auto-save)
3. Check console: `[SaveSystem] Saving data for X systems...`
4. Reload page
5. Verify settings persisted

## 4. Browser DevTools (WebGL Only)

### View Saved Data
1. F12 → Application/Storage tab
2. IndexedDB → `UnityGameSaveData` → `saves` → `saveData`
3. You'll see JSON with your game data

### Clear Data for Testing
```javascript
// In browser console:
indexedDB.deleteDatabase('UnityGameSaveData');
```

## 5. Common Issues

### "Cannot find WebGLSerializableDataFileLoader"
- Make sure component is added to the scene
- Check it's the same GameObject as RootLifetimeScope or a child

### No logs in WebGL build
- Verify you're testing WebGL build, not Editor
- Check `SaveSystemPlugin.jslib` exists in `Assets/Plugins/WebGL/`
- Look for JavaScript errors in browser console

### Data doesn't persist
- Don't use browser incognito/private mode
- Check browser storage settings
- Some browsers block IndexedDB in file:// protocol - use local server

## 6. Editor Tools

Available in Unity menu bar:

- **Tools > Save System > Clear Save Data** - Delete local save files
- **Tools > Save System > Open Save Data Folder** - Opens persistent data folder
- **Tools > Save System > Log Save Data Path** - Shows current save file content

## 7. Adding New Saveable Data

```csharp
public class MyNewSystem : ISerializableDataSystem
{
    private int _myValue;
    
    public MyNewSystem(ISaveSystem saveSystem)
    {
        saveSystem.AddSystem(this);  // Register for save/load
    }
    
    public UniTask LoadData(SerializableDataContainer container)
    {
        _myValue = container.TryGet(nameof(_myValue), out int value) 
            ? value 
            : 100; // default
        return UniTask.CompletedTask;
    }
    
    public void SaveData(SerializableDataContainer container)
    {
        container.SetData(nameof(_myValue), _myValue);
    }
}
```

Register in VContainer:
```csharp
builder.Register<MyNewSystem>(Lifetime.Singleton)
    .AsImplementedInterfaces();
```

## Done! 🎉

Your save system is now WebGL-ready with IndexedDB support!

