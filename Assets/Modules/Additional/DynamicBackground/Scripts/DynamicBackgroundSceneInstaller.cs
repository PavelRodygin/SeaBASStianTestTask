using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;

namespace Modules.Additional.DynamicBackground.Scripts
{
    public class DynamicBackgroundSceneInstaller : SceneInstaller
    {
        [SerializeField] private DynamicParticleController dynamicParticleController;
        public override void RegisterSceneDependencies(IContainerBuilder builder) => 
            builder.RegisterInstance(dynamicParticleController);
    }
}