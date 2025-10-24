using System;
using System.Threading;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.Services.SceneInstallerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Implementation.Infrastructure
{
    public class ModuleStateMachine : IModuleStateMachine, IStartable
    {
        [Inject] private readonly AudioListenerService _audioListenerService;
        [Inject] private readonly SceneInstallerService _sceneInstallerService;
        [Inject] private readonly ModuleTypeMapper _moduleTypeMapper;
        [Inject] private readonly SceneService _sceneService;
        [Inject] private readonly InputSystemService _inputSystemService;
        [Inject] private readonly IObjectResolver _resolver;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL doesn't support threading, using a simple flag
        private bool _isModuleSwitching;
#else
        // Reducing the number of threads to one
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1); 
#endif
        
        public ModulesMap CurrentModulesMap { get; private set; } = ModulesMap.None;
        public IModuleController CurrentModuleController { get; set; }
        
        public void Start() => RunModule(SceneManager.GetActiveScene().name);

        private void RunModule(string sceneName, object param = null)
        {
            ModulesMap? moduleControllerMap = SceneNameToEnum(sceneName);
            
            if (moduleControllerMap != null)
                RunModule((ModulesMap)moduleControllerMap, splashScreenRequired: false, param).Forget(); 
            else
            {
                _sceneService.AddModuleActiveScene(sceneName);
                _sceneInstallerService.
                    CombineScenes(LifetimeScope.Find<RootLifetimeScope>(), true);
            }
        }

        /// <summary>
        /// Launches a new screen state (only after the previous state finishes execution).
        /// </summary>
        /// <param name="modulesMap">Type of the screen.</param>
        /// <param name="param">Parameters to pass to Presenter.</param>
        public async UniTaskVoid RunModule(ModulesMap modulesMap, bool splashScreenRequired = false, 
            object param = null)
        {
            if (CheckIsSameModule(modulesMap))
                return;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: Wait until previous module finishes instead of ignoring the call
            await UniTask.WaitUntil(() => !_isModuleSwitching);
            _isModuleSwitching = true;
#else
            await _semaphoreSlim.WaitAsync(); //Asynchronously waits to enter the SemaphoreSlim.
#endif
            try
            {
                Debug.Log($"[ModuleStateMachine] LoadScenesForModule START: {modulesMap}");
                await _sceneService.LoadScenesForModule(modulesMap);
                Debug.Log($"[ModuleStateMachine] LoadScenesForModule COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] UnloadUnusedScenesAsync START");
                await _sceneService.UnloadUnusedScenesAsync();
                Debug.Log($"[ModuleStateMachine] UnloadUnusedScenesAsync COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] SetActiveScene START");
                var moduleScene = SceneManager.GetSceneByName(modulesMap.ToString());
                if (moduleScene.IsValid())
                    SceneManager.SetActiveScene(moduleScene);
                Debug.Log($"[ModuleStateMachine] SetActiveScene COMPLETE");

                Debug.Log($"[ModuleStateMachine] EnsureSingleEventSystem START");
                // Ensure only one EventSystem exists after scene load (critical for UI and WebGL)
                _inputSystemService.EnsureSingleEventSystem();
                Debug.Log($"[ModuleStateMachine] EnsureSingleEventSystem COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] CombineScenes START");
                // creates children for the root installer
                var sceneLifetimeScope =
                    _sceneInstallerService.CombineScenes(LifetimeScope.Find<RootLifetimeScope>(), true);
                Debug.Log($"[ModuleStateMachine] CombineScenes COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] ResolveModuleController START");
                CurrentModuleController = _moduleTypeMapper.ResolveModuleController(modulesMap, sceneLifetimeScope.Container);
                CurrentModulesMap = modulesMap;
                Debug.Log($"[ModuleStateMachine] ResolveModuleController COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] EnsureAudioListenerExists START");
                _audioListenerService.EnsureAudioListenerExists(sceneLifetimeScope.Container);
                Debug.Log($"[ModuleStateMachine] EnsureAudioListenerExists COMPLETE");

                Debug.Log($"[ModuleStateMachine] Controller.Enter START");
                await CurrentModuleController.Enter(param);
                Debug.Log($"[ModuleStateMachine] Controller.Enter COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] Controller.Execute START");
                await CurrentModuleController.Execute();
                Debug.Log($"[ModuleStateMachine] Controller.Execute COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] Controller.Exit START");
                await CurrentModuleController.Exit();
                Debug.Log($"[ModuleStateMachine] Controller.Exit COMPLETE");

                Debug.Log($"[ModuleStateMachine] Controller.Dispose START");
                CurrentModuleController.Dispose();
                Debug.Log($"[ModuleStateMachine] Controller.Dispose COMPLETE");
                
                Debug.Log($"[ModuleStateMachine] sceneLifetimeScope.Dispose START");
                sceneLifetimeScope.Dispose(); // only children lifeTimeScopes are destroyed
                Debug.Log($"[ModuleStateMachine] sceneLifetimeScope.Dispose COMPLETE");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModuleStateMachine] ERROR in RunModule: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL: Release flag AFTER everything is done
                _isModuleSwitching = false;
#else
                _semaphoreSlim.Release();
#endif
            }
        }
        
        /// <summary>
        /// Checks if the requested screen is already active.
        /// </summary>
        private bool CheckIsSameModule(ModulesMap screenViewModelMap) => 
            screenViewModelMap == CurrentModulesMap;

        /// <summary>
        /// Tries to convert screen name in string to its name in enum. Can return null if the sceneName is not found
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private static ModulesMap? SceneNameToEnum(string sceneName)
        {
            if (Enum.TryParse(sceneName, out ModulesMap result)) return result;
            return null;
        }
    }
}
