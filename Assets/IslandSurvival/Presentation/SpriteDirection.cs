using UnityEngine;

namespace IslandSurvival.Presentation
{
    public enum SpriteDirection { N, NE, E, SE, S, SW, W, NW }

    public static class SpriteDirectionResolver
    {
        public static SpriteDirection FromWorldMovement(Vector3 movement, Transform viewTransform)
        {
            if (viewTransform == null) return FromMovement(new Vector2(movement.x, movement.z));

            var screenRight = viewTransform.right;
            screenRight.y = 0f;
            screenRight.Normalize();
            var screenUp = viewTransform.forward;
            screenUp.y = 0f;
            screenUp.Normalize();

            return FromMovement(new Vector2(
                Vector3.Dot(movement, screenRight),
                Vector3.Dot(movement, screenUp)));
        }

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
