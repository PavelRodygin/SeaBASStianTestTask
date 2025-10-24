using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.RequestSampleModule.Scripts
{
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