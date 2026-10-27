using System;
using System.Collections.Generic;
using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    [Serializable]
    public sealed class PresentationScaleEntry
    {
        public VisualAssetKey key;
        public float heightInPlayerHeights = 0.5f;
        public float widthToHeight = 1f;
        public float minPlayerHeights;
        public float maxPlayerHeights;
    }

    [CreateAssetMenu(fileName = "PresentationScaleProfile", menuName = "Island Survival/2.5D/Presentation Scale Profile")]
    public sealed class PresentationScaleProfile : ScriptableObject
    {
        public const float PlayerWorldHeight = 2.5f;
        public const float PlayerReadabilityScale = 1.2f;
        [SerializeField] List<PresentationScaleEntry> entries = new List<PresentationScaleEntry>();
        static PresentationScaleProfile runtimeDefault;

        public IReadOnlyList<PresentationScaleEntry> Entries { get { return entries; } }
        public static PresentationScaleProfile RuntimeDefault
        {
            get
            {
                if (runtimeDefault == null) runtimeDefault = Resources.Load<PresentationScaleProfile>("Visual/PresentationScaleProfile");
                if (runtimeDefault == null) runtimeDefault = CreateDefault();
                return runtimeDefault;
            }
        }

        public Vector2 WorldSize(VisualAssetKey key)
        {
            var entry = Find(key);
            var height = (entry != null ? entry.heightInPlayerHeights : 0.5f) * PlayerWorldHeight;
            return new Vector2(height * (entry != null ? entry.widthToHeight : 1f), height);
        }

        public static Vector2 ApplyReadabilityScale(VisualAssetKey key, Vector2 size)
        {
            return key == VisualAssetKey.PlayerCharacter || key == VisualAssetKey.PlayerStarter
                ? size * PlayerReadabilityScale
                : size;
        }

        public PresentationScaleEntry Find(VisualAssetKey key)
        { for (var i = 0; i < entries.Count; i++) if (entries[i].key == key) return entries[i]; return null; }

        public static PresentationScaleProfile CreateDefault()
        {
            var profile = CreateInstance<PresentationScaleProfile>(); profile.name = "Runtime Default Presentation Scale Profile";
            profile.Add(VisualAssetKey.PlayerCharacter, 1f, 0.5f, 1f, 1f);
            profile.Add(VisualAssetKey.PlayerStarter, 1f, 0.5f, 1f, 1f);
            profile.AddGroup(new[] { VisualAssetKey.GrassCluster, VisualAssetKey.GrassA, VisualAssetKey.ENV_GRASS_A, VisualAssetKey.ENV_GRASS_B, VisualAssetKey.ENV_GRASS_C }, 0.28f, 1.35f, 0.15f, 0.35f);
            profile.AddGroup(new[] { VisualAssetKey.FlowerA, VisualAssetKey.ENV_FLOWER_A, VisualAssetKey.ENV_FLOWER_B }, 0.3f, 1.1f, 0.15f, 0.4f);
            profile.AddGroup(new[] { VisualAssetKey.Bush, VisualAssetKey.BushA, VisualAssetKey.ENV_BUSH_A, VisualAssetKey.ENV_BUSH_B }, 0.58f, 1.35f, 0.35f, 0.65f);
            profile.AddGroup(new[] { VisualAssetKey.RockSmall, VisualAssetKey.ENV_ROCK_SMALL_A, VisualAssetKey.ENV_ROCK_SMALL_B }, 0.38f, 1.35f, 0.25f, 0.45f);
            profile.AddGroup(new[] { VisualAssetKey.Rock, VisualAssetKey.RockMedium, VisualAssetKey.ENV_ROCK_MEDIUM_A }, 0.68f, 1.4f, 0.5f, 0.8f);
            profile.AddGroup(new[] { VisualAssetKey.RockLarge, VisualAssetKey.RockCoastal, VisualAssetKey.ENV_ROCK_LARGE_A }, 1.1f, 1.35f, 0.85f, 1.3f);
            profile.AddGroup(new[] { VisualAssetKey.PalmTree, VisualAssetKey.PalmA, VisualAssetKey.PalmB, VisualAssetKey.PalmC, VisualAssetKey.ENV_PALM_A, VisualAssetKey.ENV_PALM_B, VisualAssetKey.ENV_PALM_C }, 2.15f, 0.62f, 2f, 2.8f);
            profile.AddGroup(new[] { VisualAssetKey.TropicalTree, VisualAssetKey.TropicalTreeA, VisualAssetKey.ENV_TREE_A, VisualAssetKey.ENV_TREE_B }, 2f, 0.78f, 2f, 3f);
            profile.AddGroup(new[] { VisualAssetKey.HouseLevel1Default, VisualAssetKey.HouseLevel1Tropical }, 2.4f, 1.25f, 2.2f, 3.2f);
            profile.AddGroup(new[] { VisualAssetKey.Shipwreck, VisualAssetKey.ENV_SHIPWRECK }, 2.8f, 1.6f, 2.5f, 4f);
            profile.AddGroup(new[] { VisualAssetKey.PropCrate, VisualAssetKey.PROP_CRATE, VisualAssetKey.Barrel, VisualAssetKey.PROP_BARREL }, 0.55f, 1f, 0.4f, 0.75f);
            profile.AddGroup(new[] { VisualAssetKey.PropDriftwood, VisualAssetKey.PROP_DRIFTWOOD, VisualAssetKey.PropRope }, 0.4f, 2f, 0.25f, 0.6f);
            return profile;
        }

        void AddGroup(VisualAssetKey[] keys, float height, float aspect, float min, float max)
        { for (var i = 0; i < keys.Length; i++) Add(keys[i], height, aspect, min, max); }
        void Add(VisualAssetKey key, float height, float aspect, float min, float max)
        { entries.Add(new PresentationScaleEntry { key = key, heightInPlayerHeights = height, widthToHeight = aspect, minPlayerHeights = min, maxPlayerHeights = max }); }
    }
}
