namespace Core.PooledScroll.AdditionalFunctionality
{
    public interface IFunctionalityScrollBehaviour
    {
        int ObjectsCount { get; }
        float AxisPosition { get; }

        float GetScrollEndPosition();
        int GetIndexAtPosition(float position);
        float GetPositionByIndex(int index);
    }
}