# Visual Upgrade 0.4 — Phase 0–3 Foundation

## Status and scope

This document records the approved implementation of Phase 0 through Phase 3 only. Phase 4 model integration and every later art-production phase remain blocked on owner approval and external game-ready 3D assets.

Art Kit PNG files are reference images only. They are not imported into `Assets` and must never be used as world textures, billboards, or substitutes for 3D production assets.

## Phase 0 — baseline and regression contract

The baseline commit immediately before this work is `ac3124c125389d7d5a61688fc48ec87bd3743822` on `migration/unity-6.3-lts`.

### Visual baseline

- The player presentation is an orange capsule under `PlayerVisualRoot`.
- Terrain is a generated 32×32 grid using flat sand plus a raised grass overlay.
- Water is a generated grid/ribbon/disc animated by CPU vertex updates and normal recalculation.
- Palm, broadleaf tree, bush, grass, rocks and shipwreck are procedural fallback visuals.
- Materials are runtime-created colors using URP Lit where available.
- One directional sun, flat ambient light, linear fog and a solid camera background provide atmosphere.
- Starting Beach remains loaded; the seven other Origin Demo visual roots activate/unload by distance.

### Required regression behavior

- Movement, sprint, grounded jump, camera and all touch/desktop controls remain unchanged.
- Gathering, inventory, equipment, crafting, survival, building, sleep/respawn and contextual interaction remain unchanged.
- All eight `OriginDemoZones` IDs remain unchanged.
- Vietnamese remains the first-run language and English switching remains immediate.
- Save schema remains v2 and continues to load v1. Discovery and the Sea Cave collectible remain durable.
- Only `Prototype_StartingBeach` remains enabled for the Web prototype build.

### Performance baseline procedure

This cloud environment has no Unity Editor, WebGL player, Safari device lab or GPU profiler, so no FPS, draw-call, triangle or memory number is claimed here.

In Unity 6.3.27f1:

1. Open `Prototype_StartingBeach`, enter Play Mode and stand at spawn with the initial camera unchanged.
2. Run **Island Survival → Visual → Capture 10 Second Baseline**.
3. Do not move the camera during the sample.
4. Copy the `VISUAL_BASELINE_04` Console line into the test record.
5. Repeat at the wreck looking inland and at the first dense vegetation edge.
6. Use Unity Profiler for CPU/GPU/GC evidence; Editor values are diagnostic, not device acceptance.
7. Build Web/PWA, repeat the fixed route on iPhone Safari and record device/iOS, sustained FPS, peak memory symptoms, load time and any tab reload.

Target remains sustained 30 FPS on the chosen Web/iPhone test device. Budgets must be tuned from measured evidence rather than invented cloud figures.

## Phase 1 — visual abstraction

`VisualAssetCatalog` is a ScriptableObject with prefab and material entries. Runtime loads an optional catalog from `Resources/Visual/VisualAssetCatalog`. A missing catalog or missing key deliberately selects the existing procedural fallback.

`VisualAssetResolver` is the only path that selects an authored prefab. `VisualPrefabInstance` records the visual key, stable gameplay identity and whether the instance is a fallback. `VisualQualityPolicy` applies Web/mobile presentation rules without changing gameplay state.

The separation contract is:

- gameplay root owns stable IDs, interaction, save/discovery identity, collider and state;
- presentation is a replaceable child;
- an art prefab must not own authoritative gameplay state;
- model replacement must not change item IDs, zone IDs, save fields or interaction components.

`PlayerVisualRoot`, interactive palms, broadleaf trees, bushes, grass clusters, common environment rocks, resource pickups, the Starting Beach wreck and the Blue Crystal now resolve presentation through this contract. Their gameplay roots retain interaction/state/colliders. With no catalog/model, the existing labeled procedural fallback is retained. Split Rock, Sea Cave and Giant Tree remain their existing landmark fallback compositions until a later approved landmark phase. No fallback is presented as final art.

## Phase 2 — material and lighting foundation

The catalog supports shared authored materials for vegetation, bark/wood, rock, dry sand, wet sand, dirt, grass and props. The new visual path loads bundled shaders as typed resources instead of discovering them by name. The old `WorldFactory.Material(Color)` path remains only for procedural fallback content.

The bundled foundation shaders are:

- `StylizedSurface`: matte mobile-readable lit surface for future authored materials;
- `StylizedTerrain`: four-way vertex-color blend for dry sand, wet sand, grass and dirt;
- `TropicalWater`: GPU vertex waves, shallow/deep color separation, lagoon and foam modes without refraction.

All runtime-generated foundation materials are cached/shared and enable instancing. Color feedback now uses `MaterialPropertyBlock`; it does not access `renderer.material` or create a material instance each frame.

`TropicalDaylightProfile` provides warm sunlight, tropical ambient color, blue atmospheric background and light linear fog. An optional catalog profile can override defaults. Existing time-of-day behavior remains authoritative after initialization.

Web quality keeps hard shadows, shorter shadow/fog distance, lower render scale, lower vegetation density, no LOD crossfade for newly resolved prefabs, and no bush/grass shadow casting.

## Phase 3 — terrain and water

Terrain topology, generated height function and gameplay collider are unchanged. Each terrain vertex now stores normalized RGBA blend weights:

- R: dry sand;
- G: wet sand;
- B: grass;
- A: dirt.

Weights use distance from the curved shoreline plus low-frequency patches for organic transitions. The redundant raised grass overlay is no longer rendered by the active world path.

Ocean, lagoon, stream, foam and waterfall objects retain their existing meshes and placement. They now animate in `TropicalWater` on the GPU. The active world no longer attaches `OceanSurface`, so it no longer modifies vertices or recalculates normals on the CPU. `OceanSurface.cs` remains temporarily as rollback code but is unused.

There is no realtime refraction, depth texture dependency, screen-space effect or gameplay water change.

## Catalog and future model integration

Recommended asset layout:

```text
Assets/IslandSurvival/Art/
  Characters/Player/{Models,Animations,Materials,Textures,Prefabs}
  Environment/Vegetation/{Palms,TropicalTrees,Bushes,Grass}
  Environment/{Rocks,Terrain,Water}
  Props/BeachStory/
  Shared/{Materials,Textures,Atlases}
Assets/IslandSurvival/Visual/
  VisualAssetKey.cs
  VisualAssetCatalog.cs
  VisualAssetResolver.cs
  VisualPrefabInstance.cs
  VisualQualityPolicy.cs
```

When real models arrive, create `Resources/Visual/VisualAssetCatalog.asset`, assign approved prefabs/materials, and replace one category at a time. Never move gameplay components into those prefabs.

## Validation

Cloud/static checks can validate source structure, JSON/YAML syntax, key stability, shader presence and forbidden reference scans. Unity must validate shader compilation, runtime rendering, tests, Play Mode regressions and Web build output.

Required owner checks after pulling:

1. Confirm Console compiles with zero errors and no pink shaders.
2. Run **Island Survival → Validate Prototype 0.1**.
3. Run all Edit Mode tests, including `VisualFoundationTests`.
4. Complete the current Prototype 0.1 gameplay regression route.
5. Save/load v1 and v2 data and verify discoveries/collectible state.
6. Confirm the player capsule is labeled as fallback and gameplay root/collider behavior is unchanged.
7. Watch terrain transitions and GPU-animated ocean/lagoon/foam/waterfalls.
8. Build **Island Survival → Build → Web PWA Mobile Test**.
9. Deploy through HTTPS and validate touch, safe area, 30 FPS target and memory behavior on iPhone Safari.

## Phase 4 gate

Do not integrate replacement palm, tree, bush, grass, rock, shipwreck or character models until their real production assets exist and Phase 4 is explicitly approved.
