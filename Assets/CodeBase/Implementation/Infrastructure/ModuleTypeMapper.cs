using System;
using System.Collections.Generic;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using CodeBase.Core.Patterns.Architecture.MVP;
using Modules.Base.AnimationSampleModule.Scripts;
using Modules.Base.Bootstrap.Scripts;
using Modules.Base.MainMenu.Scripts;
using Modules.Base.RequestSampleModule.Scripts;
using Modules.Base.ScrollSampleModule.Scripts;
using Modules.Base.TimerSampleModule.Scripts;
using VContainer;

namespace CodeBase.Implementation.Infrastructure
{
    ///<summary> 
    /// Responsible for resolving (or instantiating) the appropriate module controller for the
    /// specified ModulesMap, using a dependency injection container provided by sceneLifetimeScope
    ///</summary>
    public class ModuleTypeMapper
    {
        private readonly Dictionary<ModulesMap, Type> _map;

        public ModuleTypeMapper()
        {
            _map = new Dictionary<ModulesMap, Type> 
            {
                { ModulesMap.Bootstrap, typeof(BootstrapModuleController) },
                { ModulesMap.MainMenu, typeof(MainMenuModuleController) },
                { ModulesMap.ScrollSample, typeof(ScrollSampleModuleController) },
                { ModulesMap.TimerSample, typeof(TimerSampleModuleController) },
                { ModulesMap.AnimationSample, typeof(AnimationSampleModuleController) },
                { ModulesMap.RequestSample, typeof(RequestSampleModuleController) },
            };
        }

        public IPresenter Resolve(ModulesMap modulesMap, IObjectResolver objectResolver) => 
            (IPresenter)objectResolver.Resolve(_map[modulesMap]);
        
        public IModuleController ResolveModuleController(ModulesMap modulesMap, IObjectResolver objectResolver) =>
        (IModuleController)objectResolver.Resolve(_map[modulesMap]);
    }
}