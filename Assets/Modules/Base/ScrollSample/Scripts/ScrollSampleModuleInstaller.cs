using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// Installer for ScrollSample module that registers all dependencies
    /// 
    /// IMPORTANT: This is a scrollSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from ScrollSampleModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace Modules.Base.ScrollSampleModule.Scripts match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
    /// </summary>
    public class ScrollSampleModuleInstaller : BaseModuleSceneInstaller
    {
        [Header("View")]
        [SerializeField] private ScrollSampleView scrollSampleView;
        
        [Header("Scroll Item Prefab")]
        [SerializeField] private ScrollItemView scrollItemPrefab;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            // Register main module controller
            builder.Register<ScrollSampleModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<ScrollSampleModuleModel>(Lifetime.Singleton);
            builder.Register<ScrollSamplePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(scrollSampleView).As<ScrollSampleView>();
            
            // Register scroll item factory and pool
            builder.Register<ScrollItemFactory>(Lifetime.Singleton)
                .WithParameter(scrollItemPrefab)
                .AsImplementedInterfaces();
            builder.Register<ScrollItemPool>(Lifetime.Singleton);
        }
    }
}