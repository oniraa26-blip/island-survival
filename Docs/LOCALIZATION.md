# Localization foundation

## Supported languages

- `vi-VN` — Tiếng Việt — first-run default
- `en-US` — English

The current locale is stored in Unity `PlayerPrefs` under `island_survival.settings.language`. WebGL persists this setting through its IndexedDB-backed PlayerPrefs implementation. This preference is intentionally separate from the Prototype 0.1 world save so changing or deleting a save does not unexpectedly change the UI language.

In Play Mode, open **CÀI ĐẶT / SETTINGS**, then select **Tiếng Việt** or **English**. `LocalizationService.LanguageChanged` refreshes the current HUD, inventory, interaction label, time phase, and settings screen without restarting.

## Data and code rules

Catalogs are UTF-8 JSON files:

- `Assets/IslandSurvival/Resources/Localization/vi-VN.json`
- `Assets/IslandSurvival/Resources/Localization/en-US.json`

Player-facing text must be represented by a stable localization key. Do not translate internal IDs, C# identifiers, filenames, save fields, or asset IDs. For example, the item ID remains `stone_axe`; its display key is `item.stone_axe.name`.

Use namespaced keys so future content can be added without changing gameplay systems:

- `item.*`, `recipe.*`, `building.*`
- `crop.*`, `fish.*`, `animal.*`
- `npc.*`, `dialogue.*`, `quest.*`, `journal.*`
- `tutorial.*`, `location.*`, `story.*`, `market.*`
- `ui.*`, `interaction.*`, `toast.*`

When adding a key:

1. Add it exactly once to every supported catalog.
2. Provide a non-empty Vietnamese and English translation.
3. Keep Vietnamese diacritics; never replace them with unaccented text.
4. Use `ServiceRegistry.Localization.Get("key")` for plain text.
5. Use `ServiceRegistry.Localization.Format("key", arguments)` for placeholders such as `{0}`.
6. Run **Island Survival → Validate Localization** and the Edit Mode tests.

The validator reports duplicate keys, missing catalogs, missing keys, empty translations, and a missing Vietnamese-capable font.

## Font

Runtime UI uses the bundled `NotoSans-Regular.ttf`, loaded from `Resources/Fonts`. Noto Sans includes Vietnamese Latin characters and tone-mark combinations. It replaces the previous dependency on Unity's built-in `LegacyRuntime.ttf`, whose glyph coverage is not guaranteed consistently across WebGL, iOS, and Android.

The font is distributed under the SIL Open Font License 1.1; its license is stored at `Assets/IslandSurvival/ThirdParty/NotoSans/OFL.txt`.

Correct import and visual rendering still require validation in Unity 6.3.27f1 and on target devices. Static catalog validation cannot prove that every glyph renders correctly in a built player.
