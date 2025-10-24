using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.ScrollSampleModule.Scripts
{
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