# Island Survival — Prototype 0.1

A touch-first, mobile-landscape Unity vertical slice set on Origin Island's Starting Beach. It implements the locked Prototype 0.1 loop:

`Explore → Gather → Survive → Craft → Build → Sleep/Respawn → Continue`

## Requirements

- Unity **2022.3.62f1 LTS** with Android Build Support for APK builds
- URP 14.0.11 (restored by Unity Package Manager from `Packages/manifest.json`)
- Android SDK/NDK/OpenJDK installed through Unity Hub for Android builds

The project contains no paid assets and does not require secrets or external services. Placeholder visuals are generated from simple primitives at runtime. On first editor import, `UrpProjectSetup` creates the local mobile URP renderer/pipeline assets and assigns them automatically.

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

Android development build:

```bash
/path/to/Unity -batchmode -nographics -projectPath . \
  -executeMethod IslandSurvival.Editor.PrototypeBuildTools.BuildAndroidDevelopment -quit
```

The APK is written to `Builds/Android/IslandSurvivalPrototype.apk` (ignored by Git). See [Docs/ACCEPTANCE_TESTS.md](Docs/ACCEPTANCE_TESTS.md) for the complete test matrix and the checks that still require Unity, a device, or the Android toolchain.

## Architecture

Runtime code is split into narrow domains under `Assets/IslandSurvival`: Core, Input, Player, Camera, Interaction, Items, Inventory, Equipment, Gathering, Crafting, Building, Survival, Time, Save, UI, and World. `ServiceRegistry` is wiring only; behavior remains in each domain. Items and recipes use definitions/models rather than per-item switch statements.

Save data is local JSON (`prototype_0_1_save.json`), starts at schema version 1, and preserves player transform, respawn, inventory, main-hand equipment, survival state, time, and placed camp objects.

## Known limitations

- Unity Editor and Android builds cannot be executed in environments where Unity and its licensed Android modules are absent.
- Art, audio, animation, particles, and haptics are placeholder hooks/primitive feedback only.
- Runtime-generated placeholder world objects are not intended as production content authoring.
- The slice intentionally excludes boats, NPCs, farming, fishing, market, diving, Guardians, romance, multiplayer, other islands, procedural generation, and the final story.
