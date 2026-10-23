# Unity 6.3 LTS Migration

## Status

### STATIC MIGRATION COMPLETE

The repository has a dedicated migration branch: `migration/unity-6.3-lts`.

Static migration work completed on this branch:

- changed the project marker from Unity 2022.3.62f1 to the owner-confirmed Editor version `6000.3.27f1`;
- changed URP from 14.0.11 to the 17.3.0 migration candidate;
- added explicit UGUI 2.0.0 and updated Test Framework to 1.3.9;
- replaced Unity APIs known to be obsolete across this migration boundary;
- explicitly retained legacy Input Manager support for the existing touch implementation;
- preserved the only Web scene, all Prototype 0.1 gameplay and all three platform profiles;
- parsed and audited every C# source file present in the checkout.

The request referenced 43 C# files. The actual migration checkout contains **39** `.cs` files. All 39 are listed/audited below; no four files were silently omitted.

### UNITY EDITOR VALIDATION REQUIRED

This is not a confirmed Unity migration. The Codex cloud environment does not contain Unity 6.3, Web Build Support, Package Manager resolution results or a licensed build environment. Unity's release/package endpoints also returned HTTP 403 through the cloud proxy. Therefore:

- URP 17.3.0, Test Framework 1.3.9 and UGUI 2.0.0 are migration pins that Unity 6.3.27f1 Package Manager must resolve and validate;
- Project Settings serialization has not been upgraded by Unity;
- URP assets have not been generated/imported;
- C# has not been compiled against Unity 6.3 assemblies;
- no tests, Play Mode, Web build or Safari run has occurred.

Do not merge this branch over the 2022.3 rollback baseline until the Editor validation checklist passes.

## Rollback safety

- Unity 2022.3 Web/PWA baseline: branch `milestone/web-pwa-1`, commit `cca1a46`.
- Cross-platform foundation: branch `feature/cross-platform-foundation`, commit `8d3fa46`.
- Unity 6 migration work: branch `migration/unity-6.3-lts` only.

If Unity 6 import fails, close Unity and return to the 2022.3 branch in a separate clean checkout. Do not open the 2022.3 checkout with Unity 6 and do not copy its upgraded `ProjectSettings` or `Library` folder back.

## Static changes made

| Area | Unity 2022.3 | Migration candidate | Reason |
|---|---|---|---|
| Editor | 2022.3.62f1 | 6000.3.27f1 | Owner-confirmed Unity 6.3 LTS editor |
| URP | 14.0.11 | 17.3.0 | Unity 6.3-aligned SRP generation candidate |
| UGUI | implicit/missing | 2.0.0 explicit | Runtime and Editor asmdefs reference `Unity.ugui` |
| Test Framework | 1.1.33 | 1.3.9 | Unity 6-compatible candidate with existing NUnit tests |
| Built-in font | `Arial.ttf` | `LegacyRuntime.ttf` | Unity 6 removed the old built-in Arial resource name |
| Object lookup | `FindObjectOfType` | `FindFirstObjectByType` | Current Unity object-query API |
| Texture mip limit | `masterTextureLimit` | `globalTextureMipmapLimit` | Current quality API |
| Default render pipeline | `renderPipelineAsset` | `defaultRenderPipeline` | Current graphics setting API |
| Build target API | `BuildTargetGroup` for backend | `NamedBuildTarget` | Current PlayerSettings API where confidently known |
| Input | implicit legacy input | `activeInputHandler: 0` | Preserve current `UnityEngine.Input` touch/UI behavior |

No gameplay values, recipes, survival rules, inventory rules, buildables or world scope changed.

## Audit of all 39 C# files

### Runtime gameplay — no Unity 6-specific incompatibility found statically

- `Building/BuildSystem.cs`, `Building/PlacedBuildable.cs`
- `Camera/ThirdPersonCamera.cs`
- `Crafting/CraftingSystem.cs`, `Crafting/RecipeDefinition.cs`
- `Equipment/EquipmentModel.cs`
- `Gathering/PickupResource.cs`, `Gathering/TreeGatherable.cs`
- `Interaction/IInteractable.cs`, `Interaction/PlayerInteractor.cs`
- `Inventory/InventoryModel.cs`
- `Items/ItemDatabase.cs`, `Items/ItemDefinition.cs`
- `Player/PlayerController.cs`
- `Pooling/GameObjectPool.cs`
- `Survival/SurvivalModel.cs`
- `Time/TimeOfDayController.cs`

These use stable C# collections, Unity math/physics, CharacterController and MonoBehaviour APIs. They still require real compilation and Play Mode regression testing.

### Bootstrap, input and UI — migrated, device validation required

- `Core/PrototypeBootstrap.cs`: object lookup migrated; still creates only Starting Beach runtime services.
- `Core/PrototypeTuning.cs`, `Core/ServiceRegistry.cs`: plain runtime data/service references; no platform dependency found.
- `Input/MobileInput.cs`: retains `UnityEngine.Input.touches`, simultaneous left/right finger IDs and desktop fallback. Legacy Input Manager is explicitly selected. Must be tested on iPhone Safari.
- `UI/PrototypeHUD.cs`: built-in font migrated to `LegacyRuntime.ttf`; dynamic `Screen.safeArea` updates remain. UGUI package is now explicit.

`StandaloneInputModule` remains intentional because the gameplay currently uses legacy `UnityEngine.Input`. Do not mix an Input System rewrite into this migration.

### Platform, save and streaming — compatible design, runtime validation required

- `Platform/PlatformProfile.cs`: `UNITY_WEBGL`, `UNITY_IOS` and `UNITY_ANDROID` branches remain isolated from gameplay.
- `Platform/PlatformQualityController.cs`: mip-limit API migrated; URP 17 property behavior must compile and be profiled.
- `Save/SaveStore.cs`: Web continues to use `PlayerPrefs`/IndexedDB, native platforms use a file store.
- `Save/SaveSystem.cs`: JSON schema remains version 1; no save reset was introduced.
- `Streaming/WorldLayerContracts.cs`, `Streaming/ZoneStreamingService.cs`: additive async scene APIs remain, but only Starting Beach is included for this milestone.
- `World/WorldFactory.cs`: requests `Universal Render Pipeline/Lit`, uses instanced materials and LOD; shader discovery/stripping must be checked in a Web build.

### Editor/build code — highest validation risk

- `Editor/MobileTexturePolicy.cs`: ASTC/ETC2/Web texture import settings need an import check.
- `Editor/PlatformBuildProfiles.cs`: backend calls migrated to `NamedBuildTarget`; WebGL PlayerSettings, mobile multithreaded-rendering setters and BuildPipeline output require Unity 6.3 compilation.
- `Editor/PrototypeBuildTools.cs`: validates landscape, PWA files and exactly one enabled Starting Beach scene.
- `Editor/UrpProjectSetup.cs`: uses URP 17 types and current default-pipeline property. Its serialized URP fields (`m_RendererDataList`, `m_DefaultRendererIndex`) are the most important first-import check.

Unity 6 native Build Profile assets were not fabricated without the Editor. The existing deterministic menu/batch profiles remain the migration-safe source of truth. They can be converted to native Build Profiles after output equivalence is demonstrated.

### Tests — syntax valid, execution outstanding

- `Tests/EditMode/CraftingTests.cs`
- `Tests/EditMode/EquipmentTests.cs`
- `Tests/EditMode/InventoryTests.cs`
- `Tests/EditMode/PlatformProfileTests.cs`
- `Tests/EditMode/SaveStoreTests.cs`
- `Tests/EditMode/SurvivalTests.cs`

The test asmdef still targets Editor only and references `IslandSurvival.Runtime` plus NUnit. Test discovery requires the resolved Unity Test Framework package.

## Other migration checks

- **Asmdefs:** Runtime explicitly references UGUI and URP Runtime; Editor references URP Runtime; tests reference Runtime and NUnit. JSON syntax passes.
- **Serialization:** gameplay saves remain versioned JSON. Unity must rewrite Project Settings/assets on first open; review those diffs separately.
- **ScriptableObjects:** no gameplay state depends on a serialized ScriptableObject. URP pipeline/renderer assets are generated by the Editor setup and must be inspected after import.
- **Shaders/materials:** URP Lit is primary; Standard is only a defensive fallback. Verify no pink materials and that URP Lit survives Web shader stripping.
- **URP Global Settings:** Unity 6.3.27f1 creating one Global Settings asset during first import is expected. Keep that generated asset, its `.meta`, and the updated Graphics Settings reference together. Do not create a second asset; the project validator now requires exactly one. The generated files exist only in the owner's Windows checkout until they are reviewed and committed.
- **PWA:** template tokens, manifest, SVG prototype icon, network-first service worker and `_headers` remain text assets. Generate a real Web build to confirm Unity 6 template processing.
- **Scene references:** `EditorBuildSettings.asset` has exactly one enabled scene: `Assets/Scenes/Prototype_StartingBeach.unity`.
- **Conditional compilation:** Web/iOS/Android symbols only select profiles/storage; gameplay has no native platform dependency.

## Very simple next steps

1. Install **Unity Hub**.
2. In Unity Hub, install **Unity 6.3 LTS 6000.3.27f1**, matching `ProjectVersion.txt`.
3. Add **Web Build Support**. On macOS, also add iOS Build Support; keep Android modules for compatibility.
4. Make a fresh clone or a second clean checkout. Select branch `migration/unity-6.3-lts`.
5. Open that folder with Unity 6.3. Do not open the rollback checkout.
6. Let Unity import and resolve packages.
7. If Package Manager says one of the candidate package versions is unavailable, use the version recommended for that exact 6000.3 editor, keeping URP on the 17.3 line where available. Record the resolved versions and commit both `manifest.json` and the generated lockfile. Do not downgrade the project randomly.
8. If Safe Mode appears, remain in Safe Mode and fix every compile error. Pay special attention to `UrpProjectSetup.cs` and `PlatformBuildProfiles.cs`.
9. When compilation succeeds, close and reopen Unity once. Confirm `Assets/Settings/MobileRenderer.asset` and `MobileURP.asset` exist and are assigned.
10. Run **Island Survival → Validate Prototype 0.1**.
11. Open **Window → General → Test Runner → EditMode → Run All**.
12. Open `Prototype_StartingBeach`, enter Play Mode and complete the Prototype 0.1 regression loop.
13. Confirm the Console has no blocking errors or pink materials.
14. Build **Island Survival → Build → Web PWA Mobile Test**.
15. Follow `WEB_DEPLOYMENT.md` to host via HTTPS and test Safari on a physical iPhone.

## Validation checklist before merge

- [ ] Exact Unity 6.3 patch installed and recorded.
- [ ] Package Manager resolves URP, UGUI and Test Framework without errors.
- [ ] Project compiles with zero errors.
- [ ] First-import Project Settings/asset diffs reviewed and committed separately.
- [ ] URP renderer/pipeline created and assigned; no pink materials.
- [ ] Exactly one Web scene: Starting Beach.
- [ ] All Edit Mode tests pass with a non-zero test count.
- [ ] Prototype 0.1 Play Mode flow passes without gameplay regression.
- [ ] Web/PWA build command exits successfully and produces complete output.
- [ ] HTTPS deployment loads on desktop.
- [ ] Physical iPhone Safari touch, safe area, save/reload and A2HS tests pass.
- [ ] Android/iOS build profile code still compiles.

Only after the relevant boxes pass may the migration be called Editor-validated. Safari remains unverified until the physical-device checks pass.
