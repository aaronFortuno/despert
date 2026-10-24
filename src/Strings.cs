// Original code, MIT. UI language follows Windows, with English fallback.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Despert
{
    public static class Strings
    {
        private static readonly string[] Languages = { "en", "ca", "es", "eu", "gl" };
        private static readonly Dictionary<string, string[]> Texts = Translations.Create();

        public static string LanguageFor(CultureInfo culture)
        {
            string language = culture == null ? "en" : culture.TwoLetterISOLanguageName;
            return Array.IndexOf(Languages, language) >= 0 ? language : "en";
        }

        public static string GetFor(CultureInfo culture, string key)
        {
            string[] values;
            if (!Texts.TryGetValue(key, out values)) throw new KeyNotFoundException("Unknown translation key: " + key);
            int index = Array.IndexOf(Languages, LanguageFor(culture));
            return index < values.Length && !string.IsNullOrEmpty(values[index]) ? values[index] : values[0];
        }

        public static string Get(string key) { return GetFor(CultureInfo.CurrentUICulture, key); }
        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.CurrentUICulture, Get(key), args);
        }
    }
}
