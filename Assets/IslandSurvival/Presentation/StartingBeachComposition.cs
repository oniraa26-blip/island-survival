using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class StartingBeachComposition
    {
        public const float SpawnClearRadius = 10f;
        public static readonly Vector3 PlayerSpawn = Vector3.zero;
        public static readonly Vector3 Shipwreck = new Vector3(16f, 0f, 4f);
        public static readonly Vector3 House = new Vector3(-23f, 0f, 38f);
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
        public static readonly Vector3[] TropicalTreePositions =
        {
            new Vector3(-24f,0f,45f), new Vector3(24f,0f,58f), new Vector3(-31f,0f,71f),
            new Vector3(31f,0f,45f), new Vector3(-38f,0f,58f), new Vector3(38f,0f,71f), new Vector3(-45f,0f,45f)
        };
        public static readonly Vector3[] BushPositions =
        {
            new Vector3(-15f,0f,20f), new Vector3(-19f,0f,23f), new Vector3(-22f,0f,20f),
            new Vector3(13f,0f,26f), new Vector3(17f,0f,29f), new Vector3(21f,0f,27f),
            new Vector3(-18f,0f,47f), new Vector3(-23f,0f,50f), new Vector3(17f,0f,49f), new Vector3(22f,0f,52f)
        };
        public static readonly Vector3[] GrassPositions =
        {
            new Vector3(-12f,0f,18f), new Vector3(-17f,0f,21f), new Vector3(-22f,0f,24f),
            new Vector3(11f,0f,23f), new Vector3(16f,0f,26f), new Vector3(22f,0f,29f),
            new Vector3(-16f,0f,42f), new Vector3(-21f,0f,45f), new Vector3(-26f,0f,48f),
            new Vector3(14f,0f,43f), new Vector3(19f,0f,47f), new Vector3(25f,0f,51f)
        };
        public static readonly Vector3[] FlowerPositions =
        {
            new Vector3(-17f,0f,22f), new Vector3(-21f,0f,25f), new Vector3(15f,0f,28f), new Vector3(20f,0f,30f)
        };

        public static bool IsSpawnClear(Vector3 position, float footprintRadius)
        {
            var delta = new Vector2(position.x - PlayerSpawn.x, position.z - PlayerSpawn.z);
            return delta.magnitude - footprintRadius >= SpawnClearRadius;
        }
    }
}
