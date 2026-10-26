using System.Collections.Generic;
using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class TemporaryArtLibrary
    {
        const string StartingAtlasPath = "Art2D/Temporary/StartingBeachAtlas";
        const string DirectionalAtlasPath = "Art2D/Temporary/DirectionalAtlas";
        const string WalkAtlasPath = "Art2D/Temporary/ExplorerWalkAtlas";
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static bool TryGet(VisualAssetKey key, SpriteDirection direction, out Sprite sprite)
        {
            string atlas; int column; int row;
            if (!TryMap(key, direction, out atlas, out column, out row)) { sprite = null; return false; }
            var cacheKey = atlas + ":" + column + ":" + row;
            if (Cache.TryGetValue(cacheKey, out sprite) && sprite != null) return true;
            var texture = Resources.Load<Texture2D>(atlas);
            if (texture == null) { sprite = null; return false; }
            sprite = Slice(texture, column, row, "TEMP_" + key + "_" + direction);
            Cache[cacheKey] = sprite; return true;
        }

        public static bool TryGetWalk(SpriteDirection direction, int frame, out Sprite sprite)
        {
            var directionIndex = (int)direction;
            var row = directionIndex / 2;
            var column = (directionIndex % 2) * 2 + Mathf.Clamp(frame, 0, 1);
            var cacheKey = WalkAtlasPath + ":" + column + ":" + row;
            if (Cache.TryGetValue(cacheKey, out sprite) && sprite != null) return true;
            var texture = Resources.Load<Texture2D>(WalkAtlasPath);
            if (texture == null) { sprite = null; return false; }
            sprite = Slice(texture, column, row, "TEMP_PlayerWalk_" + direction + "_" + frame);
            Cache[cacheKey] = sprite; return true;
        }

        static Sprite Slice(Texture2D texture, int column, int row, string name)
        {
            var x0 = Mathf.RoundToInt(texture.width * column / 4f);
            var x1 = Mathf.RoundToInt(texture.width * (column + 1) / 4f);
            var yTop = Mathf.RoundToInt(texture.height * (4 - row) / 4f);
            var yBottom = Mathf.RoundToInt(texture.height * (3 - row) / 4f);
            var sprite = Sprite.Create(texture, new Rect(x0, yBottom, x1 - x0, yTop - yBottom), new Vector2(0.5f, 0.04f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = name; return sprite;
        }

        static bool TryMap(VisualAssetKey key, SpriteDirection direction, out string atlas, out int column, out int row)
        {
            atlas = StartingAtlasPath; column = 0; row = 0;
            switch (key)
            {
                case VisualAssetKey.PlayerCharacter:
                case VisualAssetKey.PlayerStarter:
                    atlas = DirectionalAtlasPath;
                    var index = (int)direction;
                    column = index % 4; row = index / 4; return true;
                case VisualAssetKey.PalmA: column = 0; row = 0; return true;
                case VisualAssetKey.PalmB: column = 1; row = 0; return true;
                case VisualAssetKey.PalmC: column = 2; row = 0; return true;
                case VisualAssetKey.TropicalTreeA: column = 3; row = 0; return true;
                case VisualAssetKey.BushA:
                case VisualAssetKey.FlowerA: column = 0; row = 1; return true;
                case VisualAssetKey.GrassA: column = 1; row = 1; return true;
                case VisualAssetKey.RockSmall:
                case VisualAssetKey.ResourceStone: column = 2; row = 1; return true;
                case VisualAssetKey.RockMedium: column = 3; row = 1; return true;
                case VisualAssetKey.RockLarge:
                case VisualAssetKey.RockCoastal: column = 0; row = 2; return true;
                case VisualAssetKey.PropDriftwood:
                case VisualAssetKey.PropRope:
                case VisualAssetKey.ResourceWood:
                    column = 1; row = 2; return true;
                case VisualAssetKey.PropCrate:
                case VisualAssetKey.Chest: column = 2; row = 2; return true;
                case VisualAssetKey.Barrel: column = 3; row = 2; return true;
                case VisualAssetKey.Shipwreck: column = 1; row = 3; return true;
                case VisualAssetKey.HouseLevel1Default: atlas = DirectionalAtlasPath; column = BuildingColumn(direction); row = 2; return true;
                case VisualAssetKey.HouseLevel1Tropical: atlas = DirectionalAtlasPath; column = BuildingColumn(direction); row = 3; return true;
                case VisualAssetKey.FarmSoilDry:
                case VisualAssetKey.FarmSoilWet:
                case VisualAssetKey.CropTestStage1:
                case VisualAssetKey.CropTestStage2:
                case VisualAssetKey.CropTestStage3: column = 3; row = 3; return true;
                default: return false;
            }
        }

        static int BuildingColumn(SpriteDirection direction)
        {
            switch (direction)
            {
                case SpriteDirection.NE: return 0;
                case SpriteDirection.NW: return 1;
                case SpriteDirection.SE: return 2;
                default: return 3;
            }
        }
    }
}
