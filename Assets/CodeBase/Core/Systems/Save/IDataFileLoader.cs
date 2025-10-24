using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Systems.Save
{
    public interface IDataFileLoader
    {
        UniTask Write(SerializableDataContainer dataContainer);
        UniTask<SerializableDataContainer> Read();
    }
}

