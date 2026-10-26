# Island Survival 2.5D Art Pipeline

## Coordinate and camera convention

- X/Z: ground movement and placement.
- Y: physical elevation; never encode gameplay movement in Canvas/UI coordinates.
- Default camera: orthographic, 40° pitch, 45° yaw, landscape.
- Sprites are world-space `SpriteRenderer` presentations under gameplay roots.

## Pixels per unit and source size

Author at a consistent **100 pixels per Unity unit**. Export larger source art when needed, but keep each asset's declared world size consistent across directions and animation frames. Do not solve scale differences with arbitrary prefab transforms.

Recommended first-pass budgets:

- character: about 1.1 × 2.15 world units;
- bush/rock: 1–3 world units;
- palm/tree: 4–8 world units;
- large building: split only when required for occlusion or atlas limits.

## Pivot standard

Every standing world sprite uses **bottom center at its ground contact point**:

- character: between the feet;
- tree: trunk base;
- building: center of ground contact footprint;
- rock/chest/prop: bottom center.

Keep transparent padding consistent between directions. Incorrect pivots cause placement and depth-sorting errors.

## Direction and naming

Characters, NPCs and animals support `N, NE, E, SE, S, SW, W, NW`. Buildings use `NE, NW, SE, SW`. Direction is derived from X/Z movement; gameplay rotation/state does not live in the sprite.

Naming:

- static: `<asset-id>_<direction>`
- animated: `<asset-id>_<state>_<direction>_<frame>`
- example: `player_starter_walk_NE_03`
- building: `<building-id>_L<level>_<skin>_<direction>`
- example: `house_L1_tropical_SW`

Internal IDs remain stable English identifiers. Display text remains localized through localization keys.

## Character layers

Future character prefabs may contain Body, Hair, Top, Bottom, Shoes, Hat, Backpack and Tool renderers. All layers must share pivot, PPU, frame canvas and sorting group. Phase 01 only establishes the root and does not implement cosmetics.

## Building visuals

`BuildingVisualDefinition` maps:

`building ID -> level -> skin ID -> direction -> sprite`

Changing a skin must never replace the gameplay root, collider, persistent ID, storage, functionality or upgrade state.

## Import settings

- Texture Type: Sprite (2D and UI).
- Sprite Mode: Single or Multiple as authored.
- Mesh Type: Tight for irregular isolated art; Full Rect for aligned animation/layer frames.
- Pivot: custom bottom center `(0.5, 0)` unless an animation sheet requires a shared explicit pixel pivot.
- Filter: Bilinear for painted art; do not use Point unless the approved art direction changes.
- Mip Maps: off for most character/UI-like sprites; consider on for large world sprites after device profiling.
- Alpha Is Transparency: on.
- Read/Write: off after import processing.
- Web/iPhone max size and compression must be tested visually; avoid forcing RGBA compression on SingleChannel textures.

## SpriteAtlas plan

Keep atlas groups modular: character base, character equipment, Starting Beach environment, buildings/props, and FX. Enable rotation only if it does not break tooling; allow tight packing for static art, and keep layered/animated frames aligned. Use platform overrides and split atlases before exceeding mobile/Web texture or memory budgets.

## Shadows and materials

Use the shared sprite material. Grounding shadows are low-cost translucent blob sprites with no realtime light dependency. Do not instantiate one material per renderer. Water remains the existing GPU-animated URP path; no CPU deformation or screen-space refraction is introduced.

## Checklist before catalog entry

1. Original art; no copied third-party game assets.
2. Correct bottom-center pivot and world scale.
3. Required directions present and named consistently.
4. No gameplay components or colliders in visual prefab.
5. Shared material and atlas-ready texture.
6. Test front/behind sorting from Starting Beach camera.
7. Test WebGL/iPhone memory and clarity before increasing resolution.

## Phase 02 source layout

Use `Assets/IslandSurvival/Art2D` for approved production sources, grouped into Characters, Environment, Buildings, Props, Story, Farming, FX, UI and Atlases. Runtime-loadable temporary proof art is isolated under `Resources/Art2D/Temporary` and must not become an implicit production dependency.
