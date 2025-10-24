using Cysharp.Threading.Tasks;

namespace CodeBase.Services
{
    /// <summary>
    /// Interface for loading and unloading scenes
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>
        /// Load scene additively
        /// </summary>
        UniTask LoadSceneAsyncAdditive(string sceneName);
        
        /// <summary>
        /// Unload scene
        /// </summary>
        UniTask UnloadSceneAsync(string sceneName);
    }
}

