using System.Text.RegularExpressions;
using UnityEngine;

namespace Shared.Logging
{
    public class RichTextColorize
    {
        public static readonly RichTextColorize Red = new(Color.red);
        public static readonly RichTextColorize Yellow = new(Color.yellow);
        public static readonly RichTextColorize Green = new(Color.green);
        public static readonly RichTextColorize Blue = new(Color.blue);
        public static readonly RichTextColorize Cyan = new(Color.cyan);
        public static readonly RichTextColorize Magenta = new(Color.magenta);

        public static readonly Color BracketColorDefault = Color.HSVToRGB(0.45f, 0.9f, 0.8f);
        public static readonly Color AsteriskColorDefault = Color.HSVToRGB(0.82f, 0.6f, 0.75f);
        
        private const bool KeepMarkersDefault = true;

        private readonly string _prefix;
        private const string Suffix = "</color>";

        private RichTextColorize(Color color)
        {
            _prefix = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
        }

        private RichTextColorize(string hexColor)
        {
            _prefix = $"<color={hexColor}>";
        }

        public static string operator %(string text, RichTextColorize color)
        {
            return color._prefix + text + Suffix;
        }

        /// <summary>
        /// Автоматически раскрашивает текст с [] и *...* маркерами
        /// </summary>
        public static string AutoColorize(string text, bool keepMarkers = KeepMarkersDefault)
        {
            return AutoColorize(text, BracketColorDefault, AsteriskColorDefault, keepMarkers);
        }

        /// <summary>
        /// Автоматически раскрашивает текст с кастомными настройками
        /// </summary>
        public static string AutoColorize(string text, Color bracketColor, Color asteriskColor, bool keepMarkers)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var bracketHex = $"#{ColorUtility.ToHtmlStringRGB(bracketColor)}";
            var asteriskHex = $"#{ColorUtility.ToHtmlStringRGB(asteriskColor)}";

            text = ProcessSingleAsterisks(text, asteriskHex, keepMarkers);
            text = ProcessBrackets(text, bracketHex, keepMarkers);

            return text;
        }

        private static string ProcessSingleAsterisks(string text, string colorHex, bool keepMarkers)
        {
            var pattern = @"(?<!\*)\*(?!\*)(.*?)(?<!\*)\*(?!\*)";
            return Regex.Replace(text, pattern, match =>
            {
                var content = match.Groups[1].Value;
                return keepMarkers
                    ? $"<color={colorHex}>*{content}*</color>"
                    : $"<color={colorHex}>{content}</color>";
            });
        }

        private static string ProcessBrackets(string text, string colorHex, bool keepMarkers)
        {
            var pattern = @"\[(.*?)\]";
            return Regex.Replace(text, pattern, match =>
            {
                var content = match.Groups[1].Value;
                return keepMarkers
                    ? $"<color={colorHex}>[{content}]</color>"
                    : $"<color={colorHex}>{content}</color>";
            });
        }
    }
}