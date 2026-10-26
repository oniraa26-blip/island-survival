# Mobile Minimap Foundation

The Phase 02 minimap is a lightweight north-up UI map. It does not render the 3D world a second time and does not use a RenderTexture, lighting or post-processing.

## Runtime architecture

- `MinimapController`: safe-area HUD construction, fixed range, controlled 10 Hz projection refresh and player-arrow rotation.
- `MinimapMarkerRegistry`: unique marker IDs, exactly one local player and up to three teammate records.
- `MinimapMarker`: presentation-neutral position/category/name/color/active/discovery data.
- `MinimapIconDefinition` and `MinimapIconLibrary`: shared category colors and sizes.
- `MinimapProjection`: allocation-free X/Z-to-north-up UI projection, separately testable and reusable by a future full map.

The registry is exposed through `ServiceRegistry.MinimapMarkers` so future networking, NPC, quest, building and boat systems can register data without depending on HUD internals. No fake teammate is registered in normal gameplay.

## Discovery and clutter

Origin Island landmark records retain the existing stable discovery IDs. A landmark marker is hidden until `DiscoverySystem.IsDiscovered(id)` is true, and it is hidden outside local minimap range. Resources are not registered automatically. Teammates may clamp to the edge and shrink when outside range.

Supported categories are Player, Teammate, NPC, Quest, Building, Bed/Home, Market, Boat, Resource, Danger, Boss and Landmark. Phase 02 enables local player and discovered landmarks only.

## Mobile layout

The 286 px map occupies 14.9% of the 1920 px reference landscape width and is parented to the existing safe-area transform. Settings and Bag are below it; RUN, JUMP and interaction remain in the lower-right control area. Map graphics do not intercept touch raycasts.

## Validation

Run `Island Survival > UI > Validate Minimap`. In Play Mode it also checks the runtime root, tracked player and unique local marker. Use EditMode `MinimapTests` for registry capacity, north-up projection, discovery gating data and icon coverage.

Local Unity/device checks remain required for safe-area layout, readability, movement/direction updates and Web/iPhone performance.
