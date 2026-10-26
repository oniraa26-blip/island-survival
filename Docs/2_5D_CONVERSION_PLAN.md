# Island Survival 2.5D Conversion — Phase 01

## Scope and invariant baseline

Phase 01 changes presentation only. Gameplay remains in Unity world space, movement uses X/Z, Y remains height, and the existing `CharacterController`, interaction colliders, stable IDs, save v1/v2 semantics, discovery IDs, Origin Island zone IDs, localization, HUD, and zone streaming remain authoritative.

The current procedural 3D visual implementation, shaders, geometry and `ThirdPersonCamera` remain in the project. No final art or third-party art is included in this phase.

## Runtime architecture

- `PresentationMode.TwoPointFiveD` is the prototype default.
- `PresentationMode.ThreeD` is the rollback path.
- `PrototypeBootstrap` selects `IsometricCameraController` or `ThirdPersonCamera` without changing player/input initialization.
- `VisualAssetResolver` remains the only presentation construction boundary. It first resolves authored 2.5D definitions, then existing prefab/procedural fallback.
- `TwoPointFiveDVisualAdapter` converts a procedural fallback to a temporary sprite presentation while retaining its original objects for rollback.
- `SpriteWorldVisual` owns billboard orientation, direction state and depth order. Gameplay identity remains on `VisualPrefabInstance` and gameplay components remain above the visual root.
- `BuildingVisualDefinition` selects presentation by building ID, level, skin and direction. It does not own upgrade, storage, collider, save or building state.

## Camera

The 2.5D camera is orthographic, defaults to 40° pitch and 45° yaw, follows smoothly and has motion look-ahead. Player camera rotation input is intentionally ignored in Phase 01. Values live in `PrototypeTuning` for a later settings asset or cutscene/zoom driver.

## Sorting

Sorting is deterministic and allocation-free:

`category offset - dot(world position, horizontal camera forward) * 100`

This works with the fixed diagonal view instead of assuming that world Z alone is screen depth. Logical categories are Ground, GroundDecoration, Water, WorldObject, Character, Foreground and FX. The current runtime uses one shared Unity renderer sorting layer and category/order offsets; production Sorting Layers can be authored later without changing the calculation.

Static visuals calculate their order once after the camera exists and disable their updater. Dynamic character visuals continue updating. No central scan or hundreds of permanent `Update` calls are introduced.

## Rollback

Set `PrototypeTuning.presentationMode` to `PresentationMode.ThreeD`. Bootstrap then restores `ThirdPersonCamera`, and the resolver stops applying the 2.5D adapter. Existing 3D code/assets have not been deleted. This flag is deliberately not part of save data.

## Validation

Run:

1. `Island Survival > Validate Prototype 0.1`
2. `Island Survival > Visual > Validate 2.5D Foundation`
3. EditMode tests, especially `TwoPointFiveDTests`
4. Play `Assets/Scenes/Prototype_StartingBeach.unity`

In Play Mode verify movement, RUN, JUMP, interaction, HUD, save/load, discovery, ocean, zone streaming, front/behind ordering, and no pink/missing shaders. Cloud static checks cannot replace this Unity/target-device validation.

## Deferred to Phase 02+

Final sprites and animations, layered cosmetic character art, production atlases, four-direction building art, rebuilt zone compositions, buildings/farming/NPC/animals/boats, and final water artwork are intentionally excluded.
