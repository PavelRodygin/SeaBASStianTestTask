using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.RequestSampleModule.Scripts
{
    /// <summary>
    /// Installer for RequestSample module that registers all dependencies
    /// 
    /// IMPORTANT: This is a requestSample file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from RequestSampleModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace Modules.Base.RequestSampleModule.Scripts match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
    /// </summary>
    public class RequestSampleModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private RequestSampleView requestSampleView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            // Register main module controller
            builder.Register<RequestSampleModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<RequestSampleModuleModel>(Lifetime.Singleton);
            builder.Register<RequestSamplePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(requestSampleView).As<RequestSampleView>();
        }
    }
}