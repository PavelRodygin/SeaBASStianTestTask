namespace Core.PooledScroll.Events
{
    public class ScrollingEventArgs
    {
        public int StartIndex { get; }
        public int EndIndex { get; }

        public ScrollingEventArgs(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}
