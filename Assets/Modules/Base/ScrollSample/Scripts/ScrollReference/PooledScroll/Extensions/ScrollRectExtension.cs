using UnityEngine;
using UnityEngine.UI;

namespace Core.PooledScroll.Extensions
{
    public static class ScrollRectExtension
    {
        public static float GetAxisVelocity(this ScrollRect scrollRect)
        {
            return scrollRect.vertical ? scrollRect.velocity.y : scrollRect.velocity.x;
        }

        public static void SetAxisVelocity(this ScrollRect scrollRect, float velocity)
        {
            scrollRect.velocity = scrollRect.vertical ? new Vector2(0, velocity) : new Vector2(velocity, 0);
        }
    }
}
