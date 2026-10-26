using UnityEngine;

namespace IslandSurvival.Presentation
{
    public enum SpriteSortCategory
    {
        Ground = -30000,
        GroundDecoration = -20000,
        Water = -10000,
        WorldObject = 0,
        Character = 1000,
        Foreground = 10000,
        FX = 20000
    }

    public static class SpriteWorldSorting
    {
        public const int Precision = 100;

        public static int Calculate(Vector3 worldPosition, Vector3 cameraForward, SpriteSortCategory category)
        {
            var groundForward = new Vector3(cameraForward.x, 0f, cameraForward.z);
            if (groundForward.sqrMagnitude < 0.0001f) groundForward = new Vector3(1f, 0f, 1f);
            groundForward.Normalize();
            return (int)category + Mathf.RoundToInt(-Vector3.Dot(worldPosition, groundForward) * Precision);
        }
    }
}
