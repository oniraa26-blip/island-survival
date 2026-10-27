using IslandSurvival.Visual;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
#endif

namespace IslandSurvival.Presentation
{
    public enum GroundAnchorRole { PlayerFeet, TreeBase, PalmBase, BuildingBase, RockContact, ShipwreckContact, PropContact }

    public sealed class SpriteGroundAnchor : MonoBehaviour
    {
        [SerializeField] GroundAnchorRole role;
        public GroundAnchorRole Role { get { return role; } }
        public Vector3 WorldPosition { get { return transform.position; } }

        public void Initialize(VisualAssetKey key) { role = RoleFor(key); }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void OnEnable() { SortingAnchorDebugOverlay.Register(this); }
        void OnDisable() { SortingAnchorDebugOverlay.Unregister(this); }
#endif

        public static GroundAnchorRole RoleFor(VisualAssetKey key)
        {
            if (key == VisualAssetKey.PlayerCharacter || key == VisualAssetKey.PlayerStarter) return GroundAnchorRole.PlayerFeet;
            if (key == VisualAssetKey.PalmTree || key == VisualAssetKey.PalmA || key == VisualAssetKey.PalmB || key == VisualAssetKey.PalmC || key == VisualAssetKey.ENV_PALM_A || key == VisualAssetKey.ENV_PALM_B || key == VisualAssetKey.ENV_PALM_C) return GroundAnchorRole.PalmBase;
            if (key == VisualAssetKey.TropicalTree || key == VisualAssetKey.TropicalTreeA || key == VisualAssetKey.ENV_TREE_A || key == VisualAssetKey.ENV_TREE_B) return GroundAnchorRole.TreeBase;
            if (key == VisualAssetKey.HouseLevel1Default || key == VisualAssetKey.HouseLevel1Tropical) return GroundAnchorRole.BuildingBase;
            if (key == VisualAssetKey.Shipwreck || key == VisualAssetKey.ENV_SHIPWRECK) return GroundAnchorRole.ShipwreckContact;
            if (key == VisualAssetKey.Rock || key == VisualAssetKey.RockSmall || key == VisualAssetKey.RockMedium || key == VisualAssetKey.RockLarge || key == VisualAssetKey.RockCoastal || key == VisualAssetKey.ENV_ROCK_SMALL_A || key == VisualAssetKey.ENV_ROCK_SMALL_B || key == VisualAssetKey.ENV_ROCK_MEDIUM_A || key == VisualAssetKey.ENV_ROCK_LARGE_A) return GroundAnchorRole.RockContact;
            return GroundAnchorRole.PropContact;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static class SortingAnchorDebugOverlay
    {
        static readonly List<SpriteGroundAnchor> Anchors = new List<SpriteGroundAnchor>();
        public static bool Visible { get; set; }
        public static void Register(SpriteGroundAnchor anchor) { if (anchor != null && !Anchors.Contains(anchor)) Anchors.Add(anchor); }
        public static void Unregister(SpriteGroundAnchor anchor) { Anchors.Remove(anchor); }
        public static void Draw(Camera camera, Vector3 playerPosition)
        {
            if (!Visible || camera == null) return;
            for (var i = Anchors.Count - 1; i >= 0; i--)
            {
                var anchor = Anchors[i]; if (anchor == null) { Anchors.RemoveAt(i); continue; }
                if ((anchor.WorldPosition - playerPosition).sqrMagnitude > 900f) continue;
                var screen = camera.WorldToScreenPoint(anchor.WorldPosition); if (screen.z <= 0f) continue;
                var rect = new Rect(screen.x - 5f, Screen.height - screen.y - 5f, 10f, 10f);
                GUI.color = Color.magenta; GUI.Box(rect, GUIContent.none); GUI.color = Color.white;
                GUI.Label(new Rect(rect.x + 12f, rect.y - 5f, 150f, 22f), anchor.Role.ToString());
            }
        }
    }
#endif
}
