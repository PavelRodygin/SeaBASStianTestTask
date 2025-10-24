using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.AnimationSampleModule.Scripts
{
    /// <summary>
    /// Installer for AnimationSample module that registers all dependencies
    /// 
    /// IMPORTANT: This is a animationSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from AnimationSampleModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace Modules.Base.AnimationSampleModule.Scripts match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
    /// </summary>
    public class AnimationSampleModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private AnimationSampleView animationSampleView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            // Register main module controller
            builder.Register<AnimationSampleModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<AnimationSampleModuleModel>(Lifetime.Singleton);
            builder.Register<AnimationSamplePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(animationSampleView).As<AnimationSampleView>();
        }
    }
}