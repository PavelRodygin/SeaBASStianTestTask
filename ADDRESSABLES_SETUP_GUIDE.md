# Addressables Setup Guide

## Overview
This guide explains how to configure Addressables for the SeaBASStian Test Task project. The `SceneService` has been updated to use Addressables for scene loading.

## Changes Made to Code

### SceneService.cs
- ✅ Added `using UnityEngine.AddressableAssets`
- ✅ Added `using UnityEngine.ResourceManagement.ResourceProviders`
- ✅ Added `Dictionary<string, SceneInstance>` to track loaded scenes
- ✅ Updated `LoadSceneAsync()` to use `Addressables.LoadSceneAsync()`
- ✅ Updated `UnloadSceneAsync()` to use `Addressables.UnloadSceneAsync()`
- ✅ Added proper exception handling and logging

## Scenes to Add to Addressables

The following scenes need to be added to the Addressables system:

### Module Scenes (Priority: High)
1. **ScrollSample** - `Assets/Scenes/Modules/ScrollSample.unity`
2. **TimerSample** - `Assets/Scenes/Modules/TimerSample.unity`
3. **AnimationSample** - `Assets/Scenes/Modules/AnimationSample.unity`
4. **RequestSample** - `Assets/Scenes/Modules/RequestSample.unity`

### Core Scenes (Priority: High)
5. **MainMenu** - `Assets/Scenes/MainMenu.unity`
6. **Bootstrap** - `Assets/Scenes/Bootstrap.unity`

### Additive Scenes (Priority: Medium)
7. **PopupsManager** - `Assets/Scenes/PopupsManager.unity`
8. **DynamicBackground** - `Assets/Scenes/DynamicBackground.unity` (if exists)
9. **SplashScreen** - `Assets/Scenes/SplashScreen.unity` (if exists)

## Step-by-Step Setup Instructions

### 1. Open Addressables Groups Window
1. Go to `Window → Asset Management → Addressables → Groups`
2. If prompted to initialize Addressables, click "Create Addressables Settings"

### 2. Create or Use Existing Group

You have two options:

#### Option A: Use Existing "Packed Assets" Group
1. The group "Packed Assets" already exists in the project
2. This is recommended for the test task

#### Option B: Create New "Scenes" Group (Optional)
1. In Addressables Groups window, right-click in the groups list
2. Select `Create New Group → Packed Assets Template`
3. Name it "Scenes" or "Module Scenes"
4. This allows separate bundle management for scenes

### 3. Add Scenes to Addressables

For each scene listed above:

#### Method 1: Drag and Drop (Easiest)
1. Open the Addressables Groups window
2. In the Project window, navigate to `Assets/Scenes/Modules/`
3. Select the scene file (e.g., `ScrollSample.unity`)
4. Drag and drop it into the "Packed Assets" group (or your custom group)

#### Method 2: Via Inspector
1. Select the scene in the Project window
2. In the Inspector, check the **"Addressable"** checkbox at the top
3. The scene will be added to the default group
4. (Optional) Drag it to your preferred group in the Addressables Groups window

#### Method 3: Right-Click Menu
1. Right-click the scene in the Project window
2. Select `Addressables → Mark as Addressable`
3. The scene will be added to the default group

### 4. Configure Scene Addresses

For each scene added to Addressables:

1. In the Addressables Groups window, find the scene entry
2. Click on the address name (by default it's the file path)
3. **IMPORTANT**: Rename it to match the `ModulesMap` enum name:
   - `ScrollSample` (not "Assets/Scenes/Modules/ScrollSample.unity")
   - `TimerSample`
   - `AnimationSample`
   - `RequestSample`
   - `MainMenu`
   - `Bootstrap`
   - `PopupsManager`

**Why?** The `SceneService.LoadScenesForModule()` uses `modulesMap.ToString()` as the scene address.

### 5. Configure Group Settings (Recommended)

For the "Packed Assets" or "Scenes" group:

1. Click the group name in Addressables Groups window
2. In the Inspector, verify/set:
   - **Build Path**: `LocalBuildPath` (default)
   - **Load Path**: `LocalLoadPath` (default)
   - **Bundle Mode**: `Pack Together` (for small projects) or `Pack Separately` (for larger)
   - **Bundle Naming**: `Filename` or `Append Hash` (recommended for caching)

### 6. Scene Loading Priority Labels (Optional but Recommended)

You can add labels for better organization:

1. In Addressables Groups window, select a scene
2. Click the label dropdown in the scene entry
3. Add labels like:
   - `scene` - for all scenes
   - `module` - for module scenes
   - `core` - for Bootstrap and MainMenu
   - `additive` - for additive scenes like PopupsManager

### 7. Build Addressables Content

Before testing:

1. Go to `Window → Asset Management → Addressables → Groups`
2. Click `Build → New Build → Default Build Script`
3. Wait for the build to complete
4. You should see "Build completed successfully" in the Console

### 8. Testing

#### In Editor:
1. Set Play Mode Script to `Use Asset Database (fastest)` for quick testing
   - `Window → Asset Management → Addressables → Groups`
   - Top toolbar: `Play Mode Script` dropdown
2. Press Play and test scene transitions

#### In Build:
1. Build the project for WebGL or Standalone
2. Addressables content will be included automatically
3. Test all scene transitions

## Verification Checklist

After setup, verify:

- [ ] All 6+ scenes are visible in Addressables Groups window
- [ ] Each scene has the correct address (matches `ModulesMap` enum name)
- [ ] Addressables content builds without errors
- [ ] Scene transitions work in Play Mode
- [ ] No "Failed to load scene via Addressables" errors in Console
- [ ] Scene transitions work in final build

## Common Issues and Solutions

### Issue: "Failed to load scene via Addressables"

**Cause**: Scene address doesn't match the name used in `SceneService`

**Solution**: 
1. Check the address in Addressables Groups window
2. Ensure it matches the `ModulesMap` enum value exactly (case-sensitive)
3. Rebuild Addressables content

### Issue: "The referenced script on this Behaviour is missing"

**Cause**: Addressables settings need to be rebuilt

**Solution**:
1. `Window → Asset Management → Addressables → Groups`
2. `Build → Clean Build → All`
3. `Build → New Build → Default Build Script`

### Issue: Scene loads but some assets are missing

**Cause**: Not all dependencies are marked as Addressable

**Solution**:
1. In Addressables Groups window, go to `Tools → Analyze`
2. Run "Check Duplicate Bundle Dependencies"
3. Run "Check Scene to Addressable Duplicate Dependencies"
4. Fix any issues found

### Issue: Build size is too large

**Solution**:
1. Enable compression in group settings
2. Use `Pack Separately` for bundle mode
3. Enable "Compress Bundles" in Addressables Settings
4. Consider using LZ4 compression for WebGL

## Performance Recommendations

### For WebGL Builds:
1. **Bundle Compression**: Use `LZ4` (faster) or `LZMA` (smaller)
   - `Edit → Project Settings → Addressables → Compression`
2. **Bundle Mode**: `Pack Separately` for better caching
3. **Strip Unused Code**: Enable in Build Settings
4. **Pre-download**: Consider pre-loading critical scenes

### For Optimal Loading:
1. Keep Bootstrap and MainMenu in a single bundle (small, fast load)
2. Separate module scenes into individual bundles
3. Use labels to organize content
4. Enable "Contiguous Bundles" for faster loading

## Advanced Configuration (Optional)

### Custom Build Script
If you need more control over the build process:
1. Create a custom build script by duplicating `BuildScriptPackedMode`
2. Modify it to suit your needs (e.g., custom compression, bundle naming)
3. Select it in the Addressables Groups window

### Remote Content Delivery
For CDN or remote hosting (not needed for test task):
1. Create a new Profile: `Window → Asset Management → Addressables → Profiles`
2. Set Remote Build Path and Load Path
3. Move scenes to a "Remote" group
4. Build and upload bundles to server

## Integration with Existing Code

The `SceneService` now:
- ✅ Loads scenes via Addressables
- ✅ Stores `SceneInstance` references for proper cleanup
- ✅ Unloads scenes via Addressables
- ✅ Falls back to SceneManager if scene not in Addressables (for compatibility)
- ✅ Logs errors if scene loading fails

No changes needed in:
- `ModuleStateMachine.cs`
- `MainMenuPresenter.cs`
- Module controllers

The integration is transparent - modules don't need to know about Addressables!

## Quick Reference

### Required Addresses:
```
ScrollSample
TimerSample
AnimationSample
RequestSample
MainMenu
Bootstrap
PopupsManager
```

### Scene Paths:
```
Assets/Scenes/Modules/ScrollSample.unity
Assets/Scenes/Modules/TimerSample.unity
Assets/Scenes/Modules/AnimationSample.unity
Assets/Scenes/Modules/RequestSample.unity
Assets/Scenes/MainMenu.unity
Assets/Scenes/Bootstrap.unity
Assets/Scenes/PopupsManager.unity
```

## Additional Resources

- Unity Addressables Documentation: https://docs.unity3d.com/Packages/com.unity.addressables@latest
- Addressables Best Practices: https://unity.com/how-to/simplify-your-content-management-addressables
- WebGL Addressables Guide: https://docs.unity3d.com/Packages/com.unity.addressables@latest/manual/AddressableAssetsAsyncOperationHandle.html

