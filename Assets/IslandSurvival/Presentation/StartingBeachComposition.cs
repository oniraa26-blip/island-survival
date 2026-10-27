using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class StartingBeachComposition
    {
        public const float SpawnClearRadius = 10f;
        public static readonly Vector3 PlayerSpawn = Vector3.zero;
        public static readonly Vector3 Shipwreck = new Vector3(16f, 0f, 4f);
        public static readonly Vector3[] PalmPositions =
        {
            new Vector3(-20f,0f,22f), new Vector3(-27f,0f,27f), new Vector3(22f,0f,25f), new Vector3(30f,0f,31f),
            new Vector3(-35f,0f,47f), new Vector3(-24f,0f,52f), new Vector3(29f,0f,51f), new Vector3(39f,0f,58f),
            new Vector3(-31f,0f,73f), new Vector3(27f,0f,78f), new Vector3(41f,0f,88f), new Vector3(-44f,0f,91f)
        };
        public static readonly Vector3[] RockClusterCenters =
        {
            new Vector3(-23f,0f,10f), new Vector3(-20f,0f,14f), new Vector3(24f,0f,9f), new Vector3(28f,0f,15f),
            new Vector3(-31f,0f,34f), new Vector3(34f,0f,38f), new Vector3(-18f,0f,39f), new Vector3(23f,0f,48f)
        };

        public static bool IsSpawnClear(Vector3 position, float footprintRadius)
        {
            var delta = new Vector2(position.x - PlayerSpawn.x, position.z - PlayerSpawn.z);
            return delta.magnitude - footprintRadius >= SpawnClearRadius;
        }
    }
}
