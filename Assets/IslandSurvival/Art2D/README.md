# Island Survival Art2D

Production art should be organized under this root by responsibility:

- `Characters/`
- `Environment/Vegetation/`
- `Environment/Rocks/`
- `Environment/Terrain/`
- `Environment/Water/`
- `Buildings/`
- `Props/`
- `Story/`
- `Farming/`
- `FX/`
- `UI/`
- `Atlases/`

Approved art is referenced by `SpriteVisualDefinition`, `BuildingVisualDefinition` and `VisualAssetCatalog`; it does not need to live in `Resources`.

Phase 02 temporary atlases are intentionally isolated at `Assets/IslandSurvival/Resources/Art2D/Temporary` because the runtime placeholder adapter must load without authored catalog assets. Delete or disable that temporary source only after equivalent catalog entries have been validated.
