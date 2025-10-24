# WebGL Optimization Guide for SeaBASStian Test Task

## Current Status Analysis
- Unity 6000.2.7f2
- URP 2D Renderer
- WebGL Quality Level: Low (appropriate)
- Current Memory: 256-2048 MB with geometric growth

## Required Optimizations

### 1. Player Settings (Edit → Project Settings → Player → WebGL Platform)

#### Publishing Settings
- ✅ **Compression Format**: Currently Gzip (1)
  - **Recommendation**: Change to **Brotli** (best compression for WebGL)
  - Location: `Publishing Settings → Compression Format`

- ⚠️ **Exception Support**: Currently set to Full
  - **Recommendation**: Set to **None** for best performance
  - Location: `Publishing Settings → Enable Exceptions`
  - Note: This disables C# exception handling for better performance

- ✅ **Data Caching**: Enabled (good)
- ✅ **Debug Symbols**: Disabled (good)
- ✅ **Name Files As Hashes**: Enabled for caching

#### Other Settings → Configuration
- ⚠️ **Scripting Backend**: Not explicitly set for WebGL
  - **Recommendation**: Ensure **IL2CPP** is selected
  - Location: `Other Settings → Configuration → Scripting Backend`
  - Note: IL2CPP is required for WebGL in modern Unity

- ⚠️ **Managed Stripping Level**: Not set for WebGL
  - **Recommendation**: Set to **Medium** or **High**
  - Location: `Other Settings → Configuration → Managed Stripping Level`
  - Medium: Good balance between size and compatibility
  - High: Maximum size reduction (may break reflection-heavy code)

- ⚠️ **IL2CPP Code Generation**: Not set
  - **Recommendation**: Set to **Faster (smaller) builds**
  - Location: `Other Settings → Configuration → C++ Compiler Configuration`

#### Other Settings → Optimization
- ✅ **Strip Engine Code**: Enabled (good)
- **Recommendation**: Enable **Engine Code Stripping** if not already
- Consider enabling **Vertex Compression** for mesh optimization

#### Memory Settings (Currently Good)
- Initial Memory: 256 MB
- Maximum Memory: 2048 MB  
- Memory Growth: Geometric (good for WebGL)
- Power Preference: High Performance ✅

### 2. Quality Settings (Edit → Project Settings → Quality)

Current WebGL quality level is **Low** which is appropriate. Consider creating a dedicated WebGL profile:

#### Create Optimized WebGL Quality Profile
1. Add new quality level: "WebGL Optimized"
2. Settings:
   - **Pixel Light Count**: 0-1 (minimize lighting calculations)
   - **Texture Quality**: Full (for UI clarity)
   - **Anisotropic Textures**: Disabled
   - **Anti Aliasing**: 0 (use FXAA in URP instead)
   - **Soft Particles**: Disabled
   - **Shadows**: Disabled or Low (for 2D UI not needed)
   - **Shadow Resolution**: Low
   - **Shadow Distance**: Minimal
   - **VSync Count**: 0 (let browser handle it)
   - **Particle Raycast Budget**: 16-64
   - **Async Upload Time Slice**: 2ms
   - **Async Upload Buffer Size**: 16MB
3. Set as default for WebGL platform

### 3. URP 2D Renderer Settings

Check your URP 2D Renderer Asset (`Assets/Settings/...`):

#### Rendering
- **Render Scale**: 1.0 (or 0.9 for performance boost)
- **MSAA**: Disabled (not needed for UI)
- **Depth Texture**: Disabled (not needed for 2D)
- **Opaque Texture**: Disabled

#### Lighting (for 2D UI)
- **HDR**: Disabled
- **Main Light**: Disabled (if only UI)
- **Additional Lights**: Disabled

#### Post-processing
- Enable only necessary effects
- Avoid expensive effects like Bloom, DOF on WebGL

#### 2D Specific
- **Light2D**: Only if needed
- **Use Depth Stencil**: Disabled if not needed

### 4. Build Settings

#### Resolution and Presentation (Player Settings)
- **Default Canvas Width**: 960 ✅
- **Default Canvas Height**: 600 ✅
- **Run In Background**: Disabled ✅ (saves resources)
- **WebGL Template**: Use `Default` or custom minimal template

#### Graphics (Project Settings → Graphics)
- **Always Included Shaders**: Only include used shaders
- **Preloaded Shaders**: Empty (unless specific shaders needed at start)
- **Shader Stripping**: Enable all appropriate options

### 5. Scripting Define Symbols

Current WebGL symbols: `TextMeshPro;DOTWEEN;UNITASK_DOTWEEN_SUPPORT;MIRROR;...`

Consider:
- Keep only required symbols for WebGL build
- Remove unused features (Mirror, Edgegap if not used in WebGL)
- Add `WEBGL_OPTIMIZED` for conditional compilation

### 6. Canvas and UI Optimization (Already Applied)

For each Canvas in scenes:
- ✅ **Render Mode**: Screen Space - Overlay (most performant)
- ✅ **Pixel Perfect**: Disabled (unless needed)
- ✅ Use **Canvas Groups** for batch hiding/showing
- ✅ Mark static UI as **Canvas Static**
- ✅ Use **Rect Mask 2D** instead of Mask
- ✅ Disable **Raycast Target** on non-interactive elements

### 7. TextMeshPro Optimization

- Use **Bitmap fonts** instead of SDF for small text
- **Atlas Compression**: Enabled
- **Clear Dynamic Data On Build**: Enabled
- Minimize **font assets** in build

### 8. Audio Optimization

- **Audio Compression**: Vorbis for WebGL
- **Load Type**: Compressed in Memory for small files, Streaming for large
- **Sample Rate**: 22050 Hz or lower for SFX

### 9. Asset Optimization

#### Textures
- **Max Size**: 2048 for WebGL (smaller if possible)
- **Compression**: Automatic or ASTC for WebGL
- **Mip Maps**: Disabled for UI textures
- **Read/Write**: Disabled

#### Meshes
- **Read/Write**: Disabled
- **Optimize Mesh**: Enabled
- **Index Format**: Auto (use 16-bit when possible)

### 10. Build Process

#### Before Build
1. Clean build folder
2. Run **Project Auditor** (Window → Analysis → Project Auditor)
3. Check build size report

#### Build Settings
- **Development Build**: Disabled for production
- **Autoconnect Profiler**: Disabled
- **Deep Profiling**: Disabled
- **Script Debugging**: Disabled

## Testing Checklist

After applying optimizations:
- [ ] Build for WebGL
- [ ] Check build size (target: <50MB first load)
- [ ] Test loading time
- [ ] Test frame rate (target: 60 FPS on mid-range hardware)
- [ ] Test memory usage (shouldn't exceed initial allocation)
- [ ] Test on different browsers (Chrome, Firefox, Edge)
- [ ] Check browser console for warnings/errors

## Performance Targets for Test Task

- **Build Size**: <50 MB (first load)
- **Loading Time**: <10 seconds on average connection
- **Frame Rate**: Stable 60 FPS
- **Memory Usage**: <512 MB
- **Scroll Performance**: Smooth 60 FPS with 1000 items
- **Animation Performance**: 60 FPS with 5 animated + 100 static squares

## Quick Implementation Steps

1. Open `Edit → Project Settings → Player`
2. Select **WebGL** tab (left sidebar)
3. Apply **Publishing Settings** changes (Brotli, Exceptions: None)
4. Apply **Configuration** changes (IL2CPP, Stripping: Medium, Code Gen: Faster)
5. Open `Edit → Project Settings → Quality`
6. Verify WebGL uses **Low** profile or create **WebGL Optimized**
7. Locate URP 2D Renderer asset and apply **URP Settings**
8. Build and test

## Monitoring Performance

Use built-in WebGL profiler:
```javascript
// In browser console
UnityLoader.SystemInfo.graphicsDeviceName
UnityLoader.SystemInfo.processorType  
UnityLoader.SystemInfo.systemMemorySize
```

Enable Unity Profiler in Development builds:
- `Edit → Project Settings → Player → WebGL → Publishing Settings`
- Enable **Development Build**
- Enable **Autoconnect Profiler**

## Additional Resources

- Unity WebGL Optimization Guide: https://docs.unity3d.com/Manual/webgl-building.html
- URP Optimization: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- WebAssembly Performance: https://emscripten.org/docs/optimizing/Optimizing-Code.html

