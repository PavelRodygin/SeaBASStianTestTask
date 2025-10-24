using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Infrastructure
{
    public interface IModuleStateMachine
    {
        public IModuleController CurrentModuleController { get; }
        
        UniTaskVoid RunModule(ModulesMap modulesMap, bool splashScreenRequired = false,
            object param = null);
    }
    
    public static class ScreenStateMachineExtension
    {
        public static UniTaskVoid RunModule(this IModuleStateMachine self, bool splashScreenRequired,
            ModulesMap modulesMap) 
            => self.RunModule(modulesMap);
    }
}