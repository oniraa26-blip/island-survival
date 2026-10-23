# Island Survival — Prototype 0.1

A touch-first, cross-platform mobile-landscape Unity vertical slice set on Origin Island's Starting Beach. iOS native is the primary target, Web/PWA is the mobile test target, and Android remains compatible. It implements the locked Prototype 0.1 loop:

`Explore → Gather → Survive → Craft → Build → Sleep/Respawn → Continue`

## Requirements

- Unity **6.3 LTS migration candidate (`6000.3.0f1`)** on this branch; Unity Editor validation is still required
- URP 17.3.0 migration candidate (restored by Unity Package Manager from `Packages/manifest.json`)
- Relevant Unity modules: iOS Build Support on macOS, WebGL Build Support, and Android SDK/NDK/OpenJDK for their respective builds

The project contains no paid assets and does not require secrets or external services. Placeholder visuals are generated from simple primitives at runtime. On first editor import, `UrpProjectSetup` creates the local mobile URP renderer/pipeline assets and assigns them automatically. The rollback baseline remains on `milestone/web-pwa-1` at Unity 2022.3. Read [Docs/UNITY_6_MIGRATION.md](Docs/UNITY_6_MIGRATION.md) before opening this branch and [Docs/WEB_DEPLOYMENT.md](Docs/WEB_DEPLOYMENT.md) for the Web owner workflow.

## Open and run

1. In Unity Hub, add this repository folder and open it with Unity 2022.3.62f1.
2. Allow Package Manager and script compilation to finish.
3. Open `Assets/Scenes/Prototype_StartingBeach.unity`.
4. Use **Island Survival → Validate Prototype 0.1**.
5. Enter Play Mode.

The empty scene is intentional: `PrototypeBootstrap` creates the handcrafted prototype slice and narrow runtime services after scene load, keeping all authored content reviewable as text.

## Controls

Touch (the required product UX):

- Lower-left 45%: touch/drag dynamic joystick
- Right-side empty space: drag camera yaw/pitch
- `RUN`: hold to sprint
- Context button: appears only for the current usable target
- `BAG`: inventory, consume, craft, equip, and build recipes
- Placement controls: rotate, confirm, cancel

Development fallback:

- WASD/arrows: move
- Right mouse drag: camera
- Left Shift: sprint
- E: interact
- Tab: inventory
- F5/F8: save/load (Editor or development build)
- F9: trigger death/respawn test (Editor or development build)

## Tests and validation

Edit Mode tests:

```bash
/path/to/Unity -batchmode -nographics -projectPath . \
  -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -quit
```

Project validation:

```bash
/path/to/Unity -batchmode -nographics -projectPath . \
  -executeMethod IslandSurvival.Editor.PrototypeBuildTools.Validate -quit
```

Platform builds are also available from **Island Survival → Build**. Batch methods:

```bash
/path/to/Unity -batchmode -nographics -projectPath . -executeMethod IslandSurvival.Editor.PlatformBuildProfiles.BuildIOS -quit
/path/to/Unity -batchmode -nographics -projectPath . -executeMethod IslandSurvival.Editor.PlatformBuildProfiles.BuildWebPWA -quit
/path/to/Unity -batchmode -nographics -projectPath . -executeMethod IslandSurvival.Editor.PlatformBuildProfiles.BuildAndroid -quit
```

iOS exports to `Builds/iOS`, Web/PWA to `Builds/WebPWA`, and Android to `Builds/Android/IslandSurvivalPrototype.apk` (all ignored by Git). See [Docs/ACCEPTANCE_TESTS.md](Docs/ACCEPTANCE_TESTS.md) for the Prototype 0.1 matrix.

## Architecture

Runtime code is split into narrow domains under `Assets/IslandSurvival`: Core, Input, Player, Camera, Interaction, Items, Inventory, Equipment, Gathering, Crafting, Building, Survival, Time, Save, UI, and World. `ServiceRegistry` is wiring only; behavior remains in each domain. Items and recipes use definitions/models rather than per-item switch statements.

Save data is local JSON (`prototype_0_1_save.json`), starts at schema version 1, and preserves player transform, respawn, inventory, main-hand equipment, survival state, time, and placed camp objects.

## Known limitations

- Unity Editor and Android builds cannot be executed in environments where Unity and its licensed Android modules are absent.
- Art, audio, animation, particles, and haptics are placeholder hooks/primitive feedback only.
- Runtime-generated placeholder world objects are not intended as production content authoring.
- The slice intentionally excludes boats, NPCs, farming, fishing, market, diving, Guardians, romance, multiplayer, other islands, procedural generation, and the final story.
