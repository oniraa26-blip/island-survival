using UnityEngine;

namespace IslandSurvival.Presentation
{
    public enum SpriteDirection { N, NE, E, SE, S, SW, W, NW }

    public static class SpriteDirectionResolver
    {
        public static SpriteDirection FromMovement(Vector2 movement)
        {
            if (movement.sqrMagnitude < 0.0001f) return SpriteDirection.S;
            var angle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt(angle / 45f);
            if (index < 0) index += 8;
            return (SpriteDirection)(index % 8);
        }
    }
}
