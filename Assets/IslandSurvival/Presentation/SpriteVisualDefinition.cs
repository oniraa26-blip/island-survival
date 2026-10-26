using System;
using System.Collections.Generic;
using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    [Serializable]
    public sealed class DirectionalSprite
    {
        public SpriteDirection direction;
        public Sprite sprite;
    }

    [Serializable]
    public sealed class SpriteDirectionSet
    {
        [SerializeField] List<DirectionalSprite> sprites = new List<DirectionalSprite>();
        public bool TryGet(SpriteDirection direction, out Sprite sprite)
        {
            foreach (var entry in sprites)
                if (entry != null && entry.direction == direction && entry.sprite != null) { sprite = entry.sprite; return true; }
            sprite = null; return false;
        }
    }

    [Serializable]
    public sealed class SpriteAnimationSet
    {
        public string stateId;
        public SpriteDirectionSet directions = new SpriteDirectionSet();
    }

    [CreateAssetMenu(fileName = "SpriteVisualDefinition", menuName = "Island Survival/2.5D/Sprite Visual Definition")]
    public sealed class SpriteVisualDefinition : ScriptableObject
    {
        [SerializeField] VisualAssetKey assetKey;
        [SerializeField] SpriteDirectionSet idle = new SpriteDirectionSet();
        [SerializeField] List<SpriteAnimationSet> animations = new List<SpriteAnimationSet>();
        [SerializeField] Vector2 worldSize = new Vector2(1f, 2f);
        [SerializeField] bool castsBlobShadow = true;
        public VisualAssetKey AssetKey { get { return assetKey; } }
        public SpriteDirectionSet Idle { get { return idle; } }
        public Vector2 WorldSize { get { return worldSize; } }
        public bool CastsBlobShadow { get { return castsBlobShadow; } }
    }
}
