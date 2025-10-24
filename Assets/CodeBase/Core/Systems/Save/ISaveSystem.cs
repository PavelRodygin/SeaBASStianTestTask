using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Systems.Save
{
    public interface ISaveSystem
    {
        void AddSystem(ISerializableDataSystem serializableDataSystem);
        UniTaskVoid SaveData();
    }
}