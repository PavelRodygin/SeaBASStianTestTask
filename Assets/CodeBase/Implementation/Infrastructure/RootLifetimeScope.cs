using CodeBase.Core.Systems;
using CodeBase.Core.Systems.Save;
using CodeBase.Services;
using CodeBase.Services.AppEvent;
using CodeBase.Services.EventMediator;
using CodeBase.Services.Input;
using CodeBase.Services.LongInitializationServices;
using CodeBase.Services.SceneInstallerService;
using CodeBase.Systems.Save;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Implementation.Infrastructure
{
    //RootLifeTimeScope where all the dependencies needed for the whole project are registered
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private Web_AppEventsService appEventService;
        [SerializeField] private AudioSystem audioSystem;
        
        private WebGLSerializableDataFileLoader _webGLDataFileLoader;
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterServices(builder);
            RegisterSystems(builder);
                
            builder.Register<ModuleTypeMapper>(Lifetime.Singleton);

            builder.Register<ModuleStateMachine>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void RegisterSystems(IContainerBuilder builder)
        {
            // Register data file loader based on platform
#if UNITY_WEBGL && !UNITY_EDITOR
            // Create WebGLSerializableDataFileLoader GameObject if it doesn't exist
            if (_webGLDataFileLoader == null)
            {
                var loaderGO = new GameObject("WebGLSerializableDataFileLoader");
                loaderGO.transform.SetParent(transform); // Make it a child of RootLifetimeScope
                _webGLDataFileLoader = loaderGO.AddComponent<WebGLSerializableDataFileLoader>();
                DontDestroyOnLoad(loaderGO); // Persist across scenes
            }
            
            builder.RegisterInstance(_webGLDataFileLoader)
                .As<IDataFileLoader>();
#else
            builder.Register<SerializableDataFileLoader>(Lifetime.Singleton)
                .As<IDataFileLoader>();
#endif

            builder.Register<SaveSystem>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(appEventService)
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterInstance(audioSystem)
                .AsImplementedInterfaces()
                .AsSelf();
        }
        
        private void RegisterServices(IContainerBuilder builder)
        {
            RegisterLongInitializationService(builder);

            builder.Register<EventMediator>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.Register<InputSystemService>(Lifetime.Singleton)
                .As<IStartable>()
                .AsSelf();
            
            builder.Register<AudioListenerService>(Lifetime.Singleton)
                .AsSelf();
            
            builder.Register<SceneService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<SceneInstallerService>(Lifetime.Singleton);
            builder.Register<LoadingServiceProvider>(Lifetime.Singleton);
        }

        private static void RegisterLongInitializationService(IContainerBuilder builder)
        {
            builder.Register<FirstLongInitializationService>(Lifetime.Singleton);
            builder.Register<SecondLongInitializationService>(Lifetime.Singleton);
            builder.Register<ThirdLongInitializationService>(Lifetime.Singleton);
        }
    }
}