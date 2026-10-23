using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace IslandSurvival.Localization
{
    [Serializable]
    public sealed class LocalizationEntry
    {
        public string key;
        [TextArea] public string value;
    }

    [Serializable]
    public sealed class LocalizationCatalog
    {
        public string locale;
        public string languageName;
        public LocalizationEntry[] entries;
    }

    public static class LocalizationValidation
    {
        public static List<string> Validate(IList<LocalizationCatalog> catalogs, IList<string> requiredLocales)
        {
            var errors = new List<string>();
            var keysByLocale = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            var allKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var catalog in catalogs)
            {
                if (catalog == null || string.IsNullOrWhiteSpace(catalog.locale))
                {
                    errors.Add("Catalog has no locale code.");
                    continue;
                }
                if (keysByLocale.ContainsKey(catalog.locale))
                {
                    errors.Add("Duplicate locale catalog: " + catalog.locale);
                    continue;
                }

                var keys = new HashSet<string>(StringComparer.Ordinal);
                keysByLocale.Add(catalog.locale, keys);
                if (catalog.entries == null) continue;
                foreach (var entry in catalog.entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    {
                        errors.Add(catalog.locale + " contains an empty localization key.");
                        continue;
                    }
                    if (!keys.Add(entry.key)) errors.Add(catalog.locale + " contains duplicate key: " + entry.key);
                    allKeys.Add(entry.key);
                    if (string.IsNullOrWhiteSpace(entry.value)) errors.Add(catalog.locale + " has an empty translation for: " + entry.key);
                }
            }

            foreach (var locale in requiredLocales)
            {
                HashSet<string> keys;
                if (!keysByLocale.TryGetValue(locale, out keys))
                {
                    errors.Add("Missing required locale catalog: " + locale);
                    continue;
                }
                foreach (var key in allKeys)
                    if (!keys.Contains(key)) errors.Add(locale + " is missing translation: " + key);
            }
            return errors;
        }
    }

    public sealed class LocalizationService
    {
        public const string DefaultLocale = "vi-VN";
        public const string EnglishLocale = "en-US";
        public const string SettingKey = "island_survival.settings.language";
        public static readonly string[] SupportedLocales = { DefaultLocale, EnglishLocale };

        readonly Dictionary<string, Dictionary<string, string>> tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        readonly HashSet<string> reportedMissingKeys = new HashSet<string>(StringComparer.Ordinal);
        CultureInfo culture = CultureInfo.InvariantCulture;

        public string CurrentLocale { get; private set; }
        public event Action LanguageChanged;

        public LocalizationService(IList<LocalizationCatalog> catalogs, string initialLocale)
        {
            var errors = LocalizationValidation.Validate(catalogs, SupportedLocales);
            if (errors.Count > 0) throw new InvalidOperationException("Invalid localization catalogs:\n" + string.Join("\n", errors));

            foreach (var catalog in catalogs)
            {
                var table = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var entry in catalog.entries) table.Add(entry.key, entry.value);
                tables.Add(catalog.locale, table);
            }
            SetLocale(IsSupported(initialLocale) ? initialLocale : DefaultLocale, false);
        }

        public static LocalizationService LoadDefault()
        {
            var catalogs = new List<LocalizationCatalog>();
            foreach (var locale in SupportedLocales)
            {
                var asset = Resources.Load<TextAsset>("Localization/" + locale);
                if (asset == null) throw new InvalidOperationException("Missing localization resource: " + locale);
                var catalog = JsonUtility.FromJson<LocalizationCatalog>(asset.text);
                catalogs.Add(catalog);
            }
            var savedLocale = PlayerPrefs.GetString(SettingKey, DefaultLocale);
            return new LocalizationService(catalogs, savedLocale);
        }

        public bool IsSupported(string locale) { return !string.IsNullOrEmpty(locale) && tables.ContainsKey(locale); }

        public bool SetLocale(string locale, bool persist = true)
        {
            if (!IsSupported(locale)) return false;
            if (CurrentLocale == locale)
            {
                if (persist) SaveLocale(locale);
                return true;
            }
            CurrentLocale = locale;
            try { culture = CultureInfo.GetCultureInfo(locale); }
            catch (CultureNotFoundException) { culture = CultureInfo.InvariantCulture; }
            if (persist) SaveLocale(locale);
            if (LanguageChanged != null) LanguageChanged();
            return true;
        }

        static void SaveLocale(string locale)
        {
            PlayerPrefs.SetString(SettingKey, locale);
            PlayerPrefs.Save();
        }

        public string Get(string key)
        {
            Dictionary<string, string> table;
            string value;
            if (tables.TryGetValue(CurrentLocale, out table) && table.TryGetValue(key, out value)) return value;
            if (tables.TryGetValue(DefaultLocale, out table) && table.TryGetValue(key, out value)) return value;
            if (reportedMissingKeys.Add(key)) Debug.LogError("Missing localization key: " + key);
            return "[" + key + "]";
        }

        public string Format(string key, params object[] arguments)
        {
            return string.Format(culture, Get(key), arguments);
        }
    }
}
