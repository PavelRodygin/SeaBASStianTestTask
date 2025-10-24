using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.TimerSampleModule.Scripts
{
    /// <summary>
    /// Installer for TimerSample module that registers all dependencies
    /// 
    /// IMPORTANT: This is a timerSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from TimerSampleModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace Modules.Base.TimerSampleModule.Scripts match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
    /// </summary>
    public class TimerSampleModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private TimerSampleView timerSampleView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            // Register main module controller
            builder.Register<TimerSampleModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<TimerSampleModuleModel>(Lifetime.Singleton);
            builder.Register<TimerSamplePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(timerSampleView).As<TimerSampleView>();
        }
    }
}