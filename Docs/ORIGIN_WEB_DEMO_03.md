# Origin Island Web Demo 0.3

## Scope

This milestone expands the existing Starting Beach prototype into a continuous southern/central Origin Island exploration playground. It is not the complete island and it is not final art. Existing movement, sprint, jump, camera, gathering, inventory, crafting, survival, building, save/load, localization, and mobile controls remain the gameplay foundation.

Expected first-time exploration is approximately 15–30 minutes when gathering, crafting, building a first camp, and visiting the landmarks. Actual duration must be measured in Play Mode and on the Web build.

## Zone layout and flow

| Zone ID | Localized location | Approximate center | Role |
| --- | --- | ---: | --- |
| `starting_beach` | Bãi Biển Khởi Đầu / Starting Beach | `(0, 25)` | Spawn, wreck, first resources and camp area |
| `freshwater_stream` | Suối Nước Ngọt / Freshwater Stream | `(0, 95)` | Curved natural guide inland; no drinking mechanic added |
| `giant_tree` | Đại Thụ / Giant Tree | `(0, 155)` | Persistent orientation landmark and central meadow hub |
| `blue_lagoon` | Hồ Xanh / Blue Lagoon | `(-105, 215)` | Lagoon, shoreline and future fishing hook |
| `twin_falls` | Thác Đôi / Twin Falls | `(95, 225)` | Two lightweight waterfall ribbons, pool and rock approach |
| `flower_valley` | Thung Lũng Hoa / Flower Valley | `(0, 292)` | Open bright meadow with solid-geometry flowers |
| `split_rock` | Đá Chẻ / Split Rock | `(105, 72)` | Large paired-rock coastal landmark |
| `sea_cave` | Hang Biển / Sea Cave | `(158, 58)` | Small explorable rock arch and mystery collectible |

Primary flow: Starting Beach → Freshwater Stream → Giant Tree, then branch toward Blue Lagoon, Twin Falls, or Flower Valley. The coastal branch runs Starting Beach → Split Rock → Sea Cave. Curved water, elevation, vegetation groupings, landmark silhouettes, and open meadow sightlines provide guidance without new invisible walls.

## Implemented gameplay

- First-visit `Origin Discovery` tracking for all eight zones.
- Localized discovery notification: `Đã khám phá: {location}` / `Discovered: {location}`.
- Discovery state saved in schema v2.
- Save loader remains compatible with Prototype 0.1 schema v1.
- Sea Cave contains one `blue_crystal_fragment` collectible with Vietnamese and English name/description.
- Collectible state is saved and does not grant XP, levels, or story progression.
- Existing resources are spread across beach, forest/stream, lagoon/falls, flower valley, rocky coast, and cave routes.
- Hunger/thirst drain is reduced to light exploration pressure (`1.2` and `1.8` per minute).
- Landmark protection volumes prevent camp placement directly inside the wreck, Giant Tree, Split Rock, and Sea Cave traversal spaces.

No fishing, drinking, quest, dialogue, XP, NPC, or new building mechanic was added. `Fishing Shoreline Foundation` is only a hierarchy marker for later systems.

## Visual systems

- Organic generated terrain mesh spanning the demo footprint, with low elevation and a sand-to-grass overlay.
- Curved tropical coastline, turquoise animated ocean grid and lightweight solid shoreline foam.
- Shared procedural meshes for rounded rocks, curved palm trunks, palm fronds, branches and hand-hewn planks.
- Starting wreck composition using broken hull planks, ribs, crates and mast debris.
- Palm LOD0/LOD1 groups, broadleaf trees, rounded bushes, grass clusters and driftwood.
- Persistent oversized Giant Tree proxy for navigation sightlines.
- Lagoon/pool discs, stream ribbon and twin waterfall ribbons animated at a throttled update rate.
- Solid-geometry flowers instead of large transparent particle fields.
- Warm daylight palette, mobile fog and turquoise environmental color impression.
- `PlayerVisualRoot` now separates the visible fallback from the CharacterController.

These are authored procedural prototype shapes, not final production assets. They are intended to establish scale, palette, composition, interaction clearance, and performance boundaries.

## Zone and memory architecture

`OriginDemoWorld` registers eight runtime zones with unique stable IDs. Starting Beach stays loaded. Other visual roots are generated when the player enters their activation radius and destroyed after leaving a larger hysteresis radius. The Web profile uses a smaller activation radius than native profiles.

The continuous terrain, lightweight discovery triggers, gameplay resource nodes, and Giant Tree proxy remain persistent. Meshes and materials are cached/shared, so recreating a visual root does not duplicate the underlying procedural mesh/material data. The existing additive `ZoneStreamingService` remains available for later authored scene chunks; this demo does not require eight new scenes or load screens.

## Performance decisions

- One low-density continuous terrain mesh; no Unity Terrain texture layers.
- GPU instancing enabled on shared URP Lit materials.
- Shared cached geometry for repeated vegetation and rocks.
- Palm LOD groups and lower Web vegetation density.
- Zone activation/unload with hysteresis.
- Opaque solid vegetation; no alpha-card forest.
- Visual Upgrade 0.4 Phase 3 moves ocean, lagoon, stream, foam and waterfall motion to a lightweight GPU shader; active water no longer recalculates mesh normals on the CPU.
- No realtime reflections, post-processing stack, additional realtime lights, or desktop-only shader features.
- One directional light; platform shadow distance/profile remains authoritative.
- Small mesh counts and no new textures, avoiding Web texture memory growth.
- Ambience points are hierarchy-only placement markers and have no bundled clips or dependency on Unity's optional Audio module.

Frame rate and WebGL memory have not been measured in this cloud environment. Profile the built player before raising vegetation or shadow budgets.

## Placeholder and missing assets

Still placeholder/procedural:

- player capsule under `PlayerVisualRoot`;
- wreck and damaged crates;
- Giant Tree and all vegetation;
- rocks, Sea Cave and crystal;
- water, foam and waterfalls;
- campfire/leaf-bed art;
- flowers and meadow dressing.

External/custom art still needed:

- quality 4–5-head stylized character model;
- Idle/Walk/Run/Jump/Fall/Land animation set and Animator Controller;
- authored palms, broadleaf trees, bushes, grass and flower variants;
- production wreck, Giant Tree, Split Rock, cave and waterfall meshes;
- mobile-friendly handcrafted materials/textures;
- licensed ocean, wind, bird, forest, waterfall and cave ambience clips;
- optional lightweight VFX authored and profiled for Web/iPhone.

No unlicensed external art or audio was added.

## Unity 6.3.27f1 test procedure

1. Open `Assets/Scenes/Prototype_StartingBeach.unity`.
2. Wait for compilation/import and confirm Console has no errors.
3. Run **Island Survival → Validate Prototype 0.1**. This now includes zone IDs, spawn height, location localization, save versions, font/catalog, scene and platform checks.
4. Run all Edit Mode tests.
5. Enter Play Mode and verify movement, sprint, jump, camera and mobile UI first.
6. Follow the stream from the beach to the Giant Tree.
7. Visit Lagoon, Twin Falls, Flower Valley, Split Rock and Sea Cave.
8. Confirm each discovery appears only once and switches correctly between Vietnamese/English.
9. Collect the Blue Crystal Fragment, save, reload, and confirm it does not reappear.
10. Place campfire/leaf bed in normal open areas; confirm protected landmark centers reject placement.
11. Save/load an old v1 save if available, then save again and inspect normal gameplay state.
12. Watch Hierarchy while travelling: distant non-persistent zone visual roots should unload and recreate when revisited.

Visual checks requiring human judgment:

- coastline and terrain do not expose holes or severe clipping;
- traversal slopes remain comfortable for CharacterController;
- Giant Tree is readable from useful approaches;
- wreck is recognizable without blocking spawn view;
- cave arch is enterable and collectible reachable;
- water/foam/waterfalls face the camera correctly;
- LOD changes are acceptable;
- UI does not cover the center view;
- Vietnamese glyphs remain correct.

## Web/PWA build

Use **Island Survival → Build → Web PWA Mobile Test**. Output remains `Builds/WebPWA` and is ignored by Git. Follow [WEB_DEPLOYMENT.md](WEB_DEPLOYMENT.md) to host it over HTTPS. Do not open the generated build directly through `file://`.

## iPhone Safari checklist

- Launch from the HTTPS URL in Safari landscape.
- Confirm initial load and Web memory use do not trigger a tab reload.
- Test left joystick, camera swipe, RUN, JUMP, interaction, inventory and settings.
- Walk the complete route and watch for frame-time spikes when zones activate/unload.
- Check 30 FPS behavior at dense palms, Giant Tree, Lagoon and Twin Falls.
- Check touch targets and safe area around notch/Dynamic Island in both landscape directions.
- Confirm Vietnamese/English switching and persistence after page reload.
- Confirm discovery and collectible save after reload/Add to Home Screen.
- Inspect water motion and waterfall orientation on WebGL.
- Record peak memory and Console/browser errors where possible.

Origin Island Web Demo 0.3 cannot be considered visually or performance validated until it is played in Unity and tested on a real iPhone Safari/PWA build.
