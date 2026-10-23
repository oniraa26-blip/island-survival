using System.Collections.Generic;
using IslandSurvival.Localization;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class LocalizationTests
    {
        [Test]
        public void DefaultCatalogsAreCompleteAndVietnameseKeepsDiacritics()
        {
            var catalogs = LoadCatalogs();
            Assert.That(LocalizationValidation.Validate(catalogs, LocalizationService.SupportedLocales), Is.Empty);

            var localization = new LocalizationService(catalogs, "unsupported-locale");
            Assert.AreEqual(LocalizationService.DefaultLocale, localization.CurrentLocale);
            Assert.AreEqual("Tiếng Việt", localization.Get("language.vi"));
            Assert.AreEqual("Sức khỏe", localization.Get("stat.health"));
            Assert.AreEqual("Rìu Đá", localization.Get("item.stone_axe.name"));
            Assert.AreEqual("NHẢY", localization.Get("ui.jump"));
            Assert.NotNull(Resources.Load<Font>("Fonts/NotoSans-Regular"));
        }

        [Test]
        public void LanguageChangesImmediatelyWithoutRestart()
        {
            var localization = new LocalizationService(LoadCatalogs(), LocalizationService.DefaultLocale);
            var changed = false;
            localization.LanguageChanged += () => changed = true;

            Assert.IsTrue(localization.SetLocale(LocalizationService.EnglishLocale, false));

            Assert.IsTrue(changed);
            Assert.AreEqual("Stone Axe", localization.Get("item.stone_axe.name"));
        }

        [Test]
        public void ValidationFindsDuplicateAndMissingTranslations()
        {
            var catalogs = new List<LocalizationCatalog>
            {
                new LocalizationCatalog { locale = "vi-VN", entries = new[] { Entry("same", "Một"), Entry("same", "Hai") } },
                new LocalizationCatalog { locale = "en-US", entries = new LocalizationEntry[0] }
            };

            var errors = LocalizationValidation.Validate(catalogs, LocalizationService.SupportedLocales);

            var combined = string.Join("\n", errors);
            Assert.That(combined, Does.Contain("duplicate key"));
            Assert.That(combined, Does.Contain("missing translation"));
        }

        static List<LocalizationCatalog> LoadCatalogs()
        {
            var catalogs = new List<LocalizationCatalog>();
            foreach (var locale in LocalizationService.SupportedLocales)
            {
                var asset = Resources.Load<TextAsset>("Localization/" + locale);
                Assert.NotNull(asset, "Missing catalog resource: " + locale);
                catalogs.Add(JsonUtility.FromJson<LocalizationCatalog>(asset.text));
            }
            return catalogs;
        }

        static LocalizationEntry Entry(string key, string value) { return new LocalizationEntry { key = key, value = value }; }
    }
}
