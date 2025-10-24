using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Input
{
    /// <summary>
    /// A service responsible for creating and managing the InputSystem_Actions instance and controlling Action Maps.
    /// </summary>
    public class InputSystemService : IStartable, IDisposable
    {
        private const string EventSystemObjectName = "EventSystem";

        private static InputSystemUIInputModule _uiInputModule;
        private static EventSystem _eventSystem;

        public InputSystem_Actions InputActions { get; private set; }
        
        public event Action OnSwitchToUI;
        public event Action OnSwitchToPlayerHumanoid;
        public event Action OnSwitchToCrane;
        // public event Action OnSwitchToWhatDoYouWant;
        // Also may be replaced by Action<ActionMap>
        
        public void Start()
        {
            CreateInputSystemActions();
            InitializeEventSystem();
            InputActions.UI.Enable(); 
        }
        
        /// <summary>
        /// Enables only the UI Action Map, keeping it always enabled.
        /// </summary>
        public void SwitchToUI()
        {
            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Disable();
            InputActions.UI.Enable();
            OnSwitchToUI?.Invoke();
        }

        /// <summary>
        /// Enables PlayerHumanoid Action Map, keeping UI enabled.
        /// </summary>
        public void SwitchToPlayerHumanoid()
        {
            InputActions.Crane.Disable();
            InputActions.PlayerHumanoid.Enable();
            InputActions.UI.Enable();
            OnSwitchToPlayerHumanoid?.Invoke();
        }

        /// <summary>
        /// Enables Crane Action Map, keeping UI enabled.
        /// </summary>
        public void SwitchToCrane()
        {
            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Enable();
            InputActions.UI.Enable();
            OnSwitchToCrane?.Invoke();
        }

        /// <summary>
        /// Enables the UI Action Map without affecting other Action Maps (always enabled by default).
        /// </summary>
        public void EnableUI()
        {
            InputActions.UI.Enable();
        }

        /// <summary>
        /// Disables the UI Action Map (now overridden to do nothing to keep UI always enabled).
        /// </summary>
        public void DisableUI()
        {
            Debug.LogWarning("UI Action Map cannot be disabled as per design.");
            InputActions.UI.Enable();
        }

        /// <summary>
        /// Checks if the UI Action Map is enabled.
        /// </summary>
        public bool IsUIInputEnabled() => InputActions.UI.enabled;

        /// <summary>
        /// Checks if the PlayerHumanoid Action Map is enabled.
        /// </summary>
        public bool IsPlayerHumanoidInputEnabled() => InputActions.PlayerHumanoid.enabled;

        /// <summary>
        /// Checks if the Crane Action Map is enabled.
        /// </summary>
        public bool IsCraneInputEnabled() => InputActions.Crane.enabled;
        
        /// <summary>
        /// Sets the first selected object for UI navigation.
        /// </summary>
        /// <param name="selectedObject">The object to be set as the first selected.</param>
        public void SetFirstSelectedObject(Selectable selectedObject)
        {
            if (!_eventSystem)
            {
                Debug.LogWarning("EventSystem is not initialized. Cannot set first selected object.");
                return;
            }

            if (!selectedObject)
            {
                Debug.LogWarning("Selected object is null. Cannot set first selected object.");
                return;
            }

            _eventSystem.SetSelectedGameObject(selectedObject.gameObject);
        }
        
        public string GetStringActionPath(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot get full path.");
                return string.Empty;
            }

            string mapName = action.actionMap?.name ?? "UnknownMap";
            string actionName = action.name;
            return $"{mapName}/{actionName}";
        }
        
        public Observable<Unit> GetStartedObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Started Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.started += h,
                h => action.started -= h
            ).Select(_ => Unit.Default);
        }
        
        public Observable<Unit> GetPerformedObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Performed Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.performed += h,
                h => action.performed -= h
            ).Select(_ => Unit.Default);
        }
        
        public Observable<Unit> GetCanceledObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Canceled Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.canceled += h,
                h => action.canceled -= h
            ).Select(_ => Unit.Default);
        }

        //The reference of getting observables:
        // public static Observable<Unit> AsObservable(this UnityEngine.Events.UnityEvent unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent(h => new UnityAction(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        //
        // public static Observable<T> AsObservable<T>(this UnityEngine.Events.UnityEvent<T> unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent<UnityAction<T>, T>(h => new UnityAction<T>(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        
        public void Dispose()
        {
            if (InputActions == null) return;

            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Disable();
            InputActions.UI.Disable();
            
            InputActions.Disable();
            InputActions.Dispose();
        }

        /// <summary>
        /// Ensures only one EventSystem exists in the scene. Critical for scene switching and WebGL.
        /// </summary>
        public void EnsureSingleEventSystem()
        {
            var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            
            if (allEventSystems.Length == 0)
            {
                Debug.LogWarning("[InputSystemService] No EventSystem found. Creating new one.");
                _eventSystem = CreateEventSystem();
                _uiInputModule.actionsAsset = InputActions.asset;
                Object.DontDestroyOnLoad(_eventSystem.gameObject);
            }
            else if (allEventSystems.Length == 1)
            {
                _eventSystem = allEventSystems[0];
                EnsureEventSystemHasInputModule();
            }
            else
            {
                // Multiple EventSystems found - remove duplicates
                Debug.LogWarning($"[InputSystemService] Found {allEventSystems.Length} EventSystems. Removing duplicates.");
                
                EventSystem persistentEventSystem = null;
                
                // Find the DontDestroyOnLoad EventSystem if exists (WebGL-safe)
                foreach (var es in allEventSystems)
                {
                    if (es == null || es.gameObject == null) continue;
                    
                    try
                    {
                        if (es.gameObject.scene.name == "DontDestroyOnLoad")
                        {
                            persistentEventSystem = es;
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore scene access errors in WebGL
                    }
                }
                
                // If no persistent EventSystem, use the first valid one
                if (!persistentEventSystem)
                    persistentEventSystem = allEventSystems[0];
                
                _eventSystem = persistentEventSystem;
                
                // Destroy all other EventSystems
                foreach (var es in allEventSystems)
                {
                    if (es == null || es == persistentEventSystem) continue;
                    
                    Debug.Log($"[InputSystemService] Destroying duplicate EventSystem: {es.gameObject.name}");
                    Object.Destroy(es.gameObject);
                }
                
                EnsureEventSystemHasInputModule();
            }
        }

        /// <summary>
        /// Ensures the EventSystem has a properly configured InputSystemUIInputModule.
        /// </summary>
        private void EnsureEventSystemHasInputModule()
        {
            if (!_eventSystem) return;

            var inputModule = _eventSystem.GetComponent<InputSystemUIInputModule>();
            
            if (!inputModule)
            {
                Debug.LogWarning("EventSystem missing InputSystemUIInputModule. Adding it.");
                _uiInputModule = _eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                _uiInputModule.actionsAsset = InputActions.asset;
            }
            else
            {
                _uiInputModule = inputModule;
                if (!_uiInputModule.actionsAsset && InputActions != null)
                {
                    _uiInputModule.actionsAsset = InputActions.asset;
                }
            }
        }

        /// <summary>
        /// Initializes the EventSystem, creating a new one if it doesn't exist.
        /// </summary>
        private void InitializeEventSystem() => EnsureSingleEventSystem();

        /// <summary>
        /// Initializes the InputSystem_Actions.
        /// </summary>
        private void CreateInputSystemActions() => InputActions = new InputSystem_Actions();

        /// <summary>
        /// Creates a new EventSystem with an InputSystemUIInputModule.
        /// </summary>
        /// <returns>The created EventSystem.</returns>
        private static EventSystem CreateEventSystem()
        {
            var eventSystem = new GameObject(EventSystemObjectName).AddComponent<EventSystem>();
            _uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            return eventSystem;
        }
    }
}