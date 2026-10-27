using UnityEngine;

namespace IslandSurvival.Presentation
{
    public enum ScreenFacing { Back, BackRight, Right, FrontRight, Front, FrontLeft, Left, BackLeft }

    // Legacy atlas slot identifiers. Do not infer screen-facing semantics from these names.
    public enum SpriteDirection { N, NE, E, SE, S, SW, W, NW }

    public static class SpriteDirectionResolver
    {
        public static ScreenFacing FromWorldMovement(Vector3 movement, Transform viewTransform, ScreenFacing previous, out Vector2 screenMovement)
        {
            if (viewTransform == null)
                screenMovement = new Vector2(movement.x, movement.z);
            else
            {
                var screenRight = viewTransform.right;
                screenRight.y = 0f;
                screenRight.Normalize();
                var screenUp = viewTransform.forward;
                screenUp.y = 0f;
                screenUp.Normalize();
                screenMovement = new Vector2(
                    Vector3.Dot(movement, screenRight),
                    Vector3.Dot(movement, screenUp));
            }

            return FromScreenMovement(screenMovement, previous);
        }

        public static ScreenFacing FromScreenMovement(Vector2 movement, ScreenFacing previous = ScreenFacing.Front)
        {
            if (movement.sqrMagnitude < 0.0001f) return previous;
            var angle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt(angle / 45f);
            if (index < 0) index += 8;
            return (ScreenFacing)(index % 8);
        }

        public static SpriteDirection AtlasSlot(ScreenFacing facing)
        {
            // Verified visually against DirectionalAtlas.png and ExplorerWalkAtlas.png.
            // Their image order is Front, FrontRight, Right, BackRight,
            // Back, BackLeft, Left, FrontLeft.
            switch (facing)
            {
                case ScreenFacing.Back: return SpriteDirection.S;
                case ScreenFacing.BackRight: return SpriteDirection.SE;
                case ScreenFacing.Right: return SpriteDirection.E;
                case ScreenFacing.FrontRight: return SpriteDirection.NE;
                case ScreenFacing.Front: return SpriteDirection.N;
                case ScreenFacing.FrontLeft: return SpriteDirection.NW;
                case ScreenFacing.Left: return SpriteDirection.W;
                default: return SpriteDirection.SW;
            }
        }

        public static SpriteDirection FromWorldMovement(Vector3 movement, Transform viewTransform)
        {
            Vector2 ignored;
            return AtlasSlot(FromWorldMovement(movement, viewTransform, ScreenFacing.Front, out ignored));
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
