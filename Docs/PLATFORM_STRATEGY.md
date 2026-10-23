# Island Survival Platform Strategy

Status: architecture audit and safe foundation update. This document does **not** change any locked game-design decision or expand Prototype 0.1 gameplay.

## Version decision

- Current project: **Unity 2022.3.62f1 LTS**, URP 14.0.11.
- Recommended migration target: **Unity 6.3 LTS (`6000.3.x`)**, using the latest patched 6000.3 editor available when the migration spike begins.
- Decision now: **do not upgrade the project in place**. `ProjectVersion.txt` remains 2022.3.62f1.

Unity 6.3 LTS is the preferred target because it is a newer long-term-support line than Unity 6.0 and is the appropriate place to evaluate Unity's improved mobile web runtime, current Safari compatibility, build profiles, URP/SRP performance fixes, and current Apple/Android toolchains. The exact editor patch and current Unity support matrix must be reconfirmed before migration. The Codex cloud proxy returned HTTP 403 for Unity's documentation sites during this audit, so no claim here substitutes for that release-note check.

Useful official pages to reconfirm during the spike:

- `https://unity.com/releases/unity-6`
- `https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-browsercompatibility.html`
- `https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-technical-overview.html`
- `https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-caching.html`

## Migration risks

1. Unity performs one-way project/asset serialization. Migration must happen on a dedicated branch and clean copy, never by opening the only working checkout.
2. URP 14 moves to the Unity 6 URP generation. Renderer/pipeline serialized fields, shader behavior, lighting, shadows, and runtime quality APIs need verification.
3. The current project deliberately creates its placeholder world at runtime. Runtime code is low-risk, but `UrpProjectSetup` uses serialized URP property names and is a specific migration checkpoint.
4. Legacy `UnityEngine.Input` is currently intentional and works for the small touch prototype. Unity 6 compatibility must be checked before deciding whether to adopt the Input System package; input should not be rewritten during the editor upgrade itself.
5. Web player settings, template tokens, compression/server requirements, browser storage behavior, stripping, and exception options can change between editor lines.
6. iOS requires a supported macOS/Xcode toolchain and physical-device signing. Metal rendering and safe areas need device validation, including Dynamic Island devices.
7. Plugins are currently absent, which lowers risk. Future native plugins must be audited separately for iOS, Web, and Android.

### Controlled migration plan

1. Freeze a green 2022.3 baseline after running Edit Mode tests and Play Mode smoke tests in a real Unity installation.
2. Create `migration/unity-6.3-lts`; archive the exact editor/package versions and keep the 2022.3 branch recoverable.
3. Install the selected `6000.3.x` editor with Web, iOS Build Support, and Android modules. iOS/Xcode validation must run on macOS.
4. Open the migration branch, accept serialization/package updates, and review every changed file. Do not mix gameplay changes into this commit.
5. Resolve compile/API/URP issues and run all Edit Mode tests, project validation, Starting Beach Play Mode flow, save migration, and a clean console check.
6. Produce three builds: iOS Xcode, Web mobile test, Android APK/AAB. Test Web via HTTPS on physical iPhone Safari and installed Home Screen mode; test iOS on at least one notch/Dynamic Island phone; retain one Android smoke device.
7. Compare memory, frame time, draw calls, shader variants, load time, touch behavior, layout and save persistence against the 2022.3 baseline. Merge only when no acceptance regression remains.

## Shared-code rule

Gameplay, item/recipe data, scene contracts, world state, UI behavior and almost all assets remain shared. Platform-specific code is restricted to:

- runtime quality/budget selection;
- build configuration and texture import policy;
- persistence backend where the platform requires it;
- web shell/PWA integration and narrowly scoped native integrations.

Gameplay assemblies must not reference Android, iOS, browser JavaScript, or editor-only APIs directly. `PlatformProfile`, `ISaveStore`, and build tooling are the current boundaries.

Unity 2022.3 predates Unity 6's Build Profiles workflow, so the three reproducible profiles are currently code-defined in `PlatformBuildProfiles.cs` and exposed under **Island Survival → Build**. During the Unity 6 migration they can be represented as native Build Profile assets after output equivalence is verified.

## Profile A — iOS Native (primary)

- IL2CPP, device SDK, iPhone/iPad target and Metal-only graphics API.
- Landscape-only, full-screen presentation. Runtime UI uses `Screen.safeArea`; physical testing must cover left/right landscape orientation, notch and Dynamic Island insets.
- 30 FPS is the performance floor. Runtime targets 60 FPS on capable devices with no v-sync assumption; a future device capability/thermal policy may lower this dynamically.
- Native profile uses full prototype vegetation, 1.0 render scale, medium shadows, 42 m shadow distance, 220 m camera range and up to three loaded zones.
- Texture policy uses ASTC 6×6 and a 2048 maximum by default. UI, normals and hero assets can receive reviewed per-asset overrides later.
- Save uses versioned JSON under `Application.persistentDataPath`.

## Profile B — Web/PWA mobile test

- Purpose: a smaller Starting Beach test build, not full-quality shipping content.
- Runtime targets 30 FPS, 0.8 render scale, low shadows, 18 m shadow distance, 125 m camera range, 0.65 LOD bias, one mip level reduction, approximately 58% decorative vegetation, one loaded zone and a lower active-object budget.
- The existing touch controls and safe-area HUD are shared. The template uses `viewport-fit=cover`, safe-area CSS, a portrait rotation overlay, a landscape manifest and an attempted orientation lock. Safari can refuse programmatic orientation lock; the overlay remains the fallback.
- Prototype save switches through `ISaveStore` to `PlayerPrefs`, which Unity's Web build persists through its browser/IndexedDB backend. It explicitly calls `PlayerPrefs.Save`. Save durability, quota, private-browsing behavior, origin changes and cache clearing must be tested on Safari; browser storage is not cloud save.
- `Assets/WebGLTemplates/IslandSurvivalPWA` supplies a manifest, full-screen shell and service worker. Hosting must use HTTPS. Safari's **Add to Home Screen** is user-driven and cannot be forced. A reviewed PNG app icon, launch imagery and final cache-version/release policy are still required before public deployment.
- Brotli plus decompression fallback is configured for test hosting flexibility. Production hosting should send correct `Content-Encoding` and MIME headers. Threaded WebAssembly is not assumed, avoiding an early dependency on cross-origin isolation.
- Web quality uses a 1024 texture ceiling. Only the Starting Beach and its immediate dependencies should ship/load for this milestone.
- Mobile Safari WebGL/WebAssembly memory, audio unlock, background suspension, context loss and service-worker updates remain platform risks. A browser build is not expected to match native iOS quality or sustained performance.

## Profile C — Android Native

- Shared gameplay remains compatible and uses IL2CPP/ARM64.
- 30 FPS baseline, 60 FPS target where sustainable.
- Medium shadows, 0.9 render scale, 180 m camera range, 85% vegetation and at most two loaded zones.
- ETC2 RGBA is the conservative default texture policy. Device-tier overrides or ASTC can be added after hardware profiling rather than becoming gameplay dependencies.
- Android remains a smoke-build target, not the current primary testing platform.

## Quality comparison

| Budget | iOS native | Web/PWA test | Android native |
|---|---:|---:|---:|
| Target / baseline FPS | 60 / 30 | 30 / 30 | 60 / 30 |
| Render scale | 1.0 | 0.8 | 0.9 |
| Shadow resolution/distance | Medium / 42 m | Low / 18 m | Medium / 32 m |
| Camera far clip | 220 m | 125 m | 180 m |
| LOD bias | 1.0 | 0.65 | 0.85 |
| Texture mip reduction | 0 | 1 | 0 |
| Decorative vegetation | 100% | 58% | 85% |
| Loaded-zone budget | 3 | 1 | 2 |
| Default texture policy | ASTC 6×6, 2048 | Automatic compressed, 1024 | ETC2 RGBA, 2048 |

These are prototype starting values, not promises. Device profiling owns final tuning.

## Asset, memory and world architecture

- Materials created by the placeholder world are cached and GPU-instancing enabled. Test palms have an `LODGroup` and platform density budget.
- `GameObjectPool` provides a bounded reusable-object primitive; systems should adopt it when repeated transient prefabs appear, not pre-emptively wrap every object.
- `ZoneStreamingService` provides bounded asynchronous additive loading. Prototype 0.1 remains one slice; future zones should be separate scenes/bundles and should not load the archipelago at once.
- Long-term content delivery should evaluate Addressables after the Unity 6 migration. Adding it now would create package/migration churn before a real second zone exists.
- Texture import defaults are platform-specific. Atlas design, mesh combining, modular prefab dependencies and shader-variant budgets should be reviewed with real art.
- The architecture separates three concerns:
  - **Persistent Data:** authoritative, versioned facts that save/load without scene objects.
  - **Background Simulation:** low-frequency logical advancement for unloaded zones, with no renderer or live GameObject requirement.
  - **Active Rendered World:** pooled/LOD-controlled objects belonging only to currently loaded zones.
- Active scene objects must not become the sole source of persistent truth. This is essential for later background simulation and streaming.

## Not testable in the Codex cloud environment

- Unity script compilation, Edit Mode tests, Play Mode and URP rendering (Unity Editor is absent).
- Unity 6 project migration or package resolution.
- iOS Xcode generation, signing, Metal behavior, device safe areas, thermal behavior and native save persistence (requires macOS/Xcode/physical iPhone).
- Web build generation, HTTPS hosting, Safari WebGL/WebAssembly behavior, service worker lifecycle, IndexedDB persistence, landscape/A2HS behavior and touch concurrency on iPhone.
- Android Gradle build and device performance.
- Actual FPS, memory, draw calls, texture residency, loading spikes, context loss and browser storage limits.

Static source checks in cloud are useful but do not turn any of these into a PASS.
