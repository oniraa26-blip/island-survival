# Tropical Cozy 2.5D Visual Rework

This pass keeps gameplay roots, colliders, stable IDs, save/discovery state, movement, maps, and interaction unchanged. Sprite presentation remains a replaceable child.

## Scale

`PresentationScaleProfile` defines world size relative to a 2.5-unit player height. Ordinary palms are 1.9 player heights and ordinary tropical trees are 1.75. Grass, flowers, bushes, rocks, House L1, props, and the shipwreck use the same centralized lookup. A production ScriptableObject profile can replace the runtime default.

## Depth and anchors

All characters and world objects depth-sort in the same band from `SpriteGroundAnchor.WorldPosition`. Player uses feet; palms/trees use trunk base; buildings use foundation; rocks use bottom contact; shipwreck uses its authored ground/water contact. Gameplay colliders remain on their existing roots and do not determine sorting.

In Editor or a Development Build, press `F7` in Play Mode to toggle nearby magenta sorting anchors. `F8` remains the player directional debug overlay.

## Starting Beach

The composition is deterministic: deep/shallow water and foam lead into wet/dry sand, an open spawn beach, grass/dirt transition, clustered vegetation edges, and an open inland path. The shipwreck sits at the coastline with clustered crate, barrel, driftwood, and rocks. Normal vegetation frames rather than occupies the central corridor.

`PresentationLayerSlot` exposes replaceable terrain/water slots. `VisualAssetKey` exposes production environment slots (`ENV_*`, `PROP_*`, `GROUND_*`, `WATER_*`). Existing temporary atlases remain fallbacks only; no additional placeholder sprite set was generated.

House L1 Default/Tropical and future building levels/skins continue through the existing sprite/building visual catalog architecture.
