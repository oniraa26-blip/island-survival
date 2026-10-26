# 2.5D Starting Beach — Phase 02

Phase 02 adds a visual-only Starting Beach slice on top of the validated Phase 01 architecture. It does not alter gameplay roots, colliders, save/discovery IDs, movement, input, survival values, localization or Origin zone IDs.

## Temporary original art

Two original transparent atlases live under `Resources/Art2D/Temporary`:

- `StartingBeachAtlas`: three distinct palms, broadleaf tree, flowering bush, grass, three rock scales, driftwood/rope, crate, barrel, explorer, shipwreck, hut and crop plot.
- `DirectionalAtlas`: eight explorer directions and four directions for Default/Tropical House L1 skins.
- `ExplorerWalkAtlas`: two lightweight walk frames for every explorer direction; sprint reuses these frames at a faster presentation rate.

These files are explicitly temporary. `TemporaryArtLibrary` is an adapter beneath the existing catalog/resolver boundary. Approved production `SpriteVisualDefinition` assets remain higher priority and can replace temporary art without changing gameplay code.

## Composition

Existing freeform terrain, curved shoreline, GPU water and foam remain authoritative. The terrain vertex mask now includes a soft winding inland dirt path while preserving topology and collision. Starting Beach resolves varied 2D palms, tropical vegetation, rounded rock sizes, shipwreck, player and resource presentations. A presentation-only proof group adds restrained crate/barrel/driftwood/flower accents, a four-second Default/Tropical house skin preview, and dry/wet soil plus crop-stage visual samples inland.

The house and farming proofs contain no gameplay/save components. They demonstrate presentation substitution only.

## Performance

- Two 1254×1254 RGBA atlases; no 4K placeholder textures.
- Sprites share Unity's sprite material.
- Static world sorters disable themselves after resolving camera/order.
- One low-frequency presentation-only house preview updater.
- No `renderer.material`, CPU water deformation, realtime 2D global lights or changes to zone streaming.
- Atlas textures have mipmaps/read-write disabled and are ready for platform compression validation.

## Developer switches

- `PrototypeTuning.presentationMode`: `TwoPointFiveD` or rollback `ThreeD`.
- `PresentationRuntime.PlaceholderArtEnabled`: temporary art on/off. This is not exposed in player HUD or save data.

## Validation status

Cloud checks can validate source/assets/diff only. Unity compilation, menu validators, EditMode tests, Play Mode composition, shader appearance, depth ordering, mobile landscape framing and Web/iPhone performance require Unity 6000.3.27f1 locally.
