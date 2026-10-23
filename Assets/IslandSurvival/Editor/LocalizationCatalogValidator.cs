using System.Collections.Generic;
using IslandSurvival.Localization;
using UnityEditor;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class LocalizationCatalogValidator
    {
        [MenuItem("Island Survival/Validate Localization")]
        public static void ValidateMenu()
        {
            var failures = ValidateResources();
            if (failures > 0) throw new System.InvalidOperationException("Localization validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("Localization catalogs and Vietnamese font validation passed.");
        }

        public static int ValidateResources()
        {
            var catalogs = new List<LocalizationCatalog>();
            var failures = 0;
            foreach (var locale in LocalizationService.SupportedLocales)
            {
                var asset = Resources.Load<TextAsset>("Localization/" + locale);
                if (asset == null)
                {
                    Debug.LogError("Missing localization catalog resource: " + locale);
                    failures++;
                    continue;
                }
                var catalog = JsonUtility.FromJson<LocalizationCatalog>(asset.text);
                if (catalog == null)
                {
                    Debug.LogError("Could not parse localization catalog: " + locale);
                    failures++;
                    continue;
                }
                catalogs.Add(catalog);
            }

            foreach (var error in LocalizationValidation.Validate(catalogs, LocalizationService.SupportedLocales))
            {
                Debug.LogError(error);
                failures++;
            }
            if (Resources.Load<Font>("Fonts/NotoSans-Regular") == null)
            {
                Debug.LogError("Missing Noto Sans font resource required for Vietnamese text.");
                failures++;
            }
            return failures;
        }
    }
}
