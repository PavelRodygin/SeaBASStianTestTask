using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CodeBase.Shared
{
    public static class ColorNameCache
    {
        private const int InitialCacheCapacity = 100;
        private const byte MinLightColor = 128;
        
        private static readonly Dictionary<int, Color32> ColorCache = new(InitialCacheCapacity);

        public static void ClearCache()
        {
            ColorCache.Clear();
        }
        
        public static Color32 GetRandomColor(string typeName)
        {
            var hash = typeName.GetHashCode();
            if (ColorCache.TryGetValue(hash, out var color))
                return color;
            
            var rgbaBytes = BitConverter.GetBytes(hash);
            rgbaBytes[0] = (byte)(rgbaBytes[0] % MinLightColor + MinLightColor);
            rgbaBytes[1] = (byte)(rgbaBytes[1] % MinLightColor + MinLightColor);
            rgbaBytes[2] = (byte)(rgbaBytes[2] % MinLightColor + MinLightColor);
            color = new Color32(rgbaBytes[0], rgbaBytes[1], rgbaBytes[2], byte.MaxValue);
            
            ColorCache[hash] = color;
            return color;
        }
        
        public static string GetRandomHexColor(string typeName)
        {
            var color = GetRandomColor(typeName);
            
            // var hexColor = ColorUtility.ToHtmlStringRGB(color);
            var hexColor = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
            return hexColor;
        }
    }
}