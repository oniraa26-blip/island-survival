using UnityEngine;
using UnityEngine.Rendering;

namespace IslandSurvival.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SpriteWorldVisual : MonoBehaviour
    {
        [SerializeField] SpriteSortCategory category = SpriteSortCategory.WorldObject;
        [SerializeField] bool dynamicVisual;
        SpriteRenderer spriteRenderer;
        SortingGroup sortingGroup;
        Transform cameraTransform;
        SpriteDirectionSet directionSet;
        SpriteDirection direction = SpriteDirection.S;

        public SpriteDirection Direction { get { return direction; } }

        public void Initialize(Sprite sprite, Vector2 worldSize, SpriteSortCategory sortCategory, bool isDynamic)
        {
            category = sortCategory;
            dynamicVisual = isDynamic;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            sortingGroup = GetComponent<SortingGroup>();
            if (sortingGroup == null) sortingGroup = gameObject.AddComponent<SortingGroup>();
            if (sprite != null)
            {
                var spriteSize = sprite.bounds.size;
                transform.localScale = new Vector3(worldSize.x / Mathf.Max(0.01f, spriteSize.x), worldSize.y / Mathf.Max(0.01f, spriteSize.y), 1f);
            }
            Refresh(true);
        }

        public void Initialize(SpriteVisualDefinition definition, Sprite sprite, SpriteSortCategory sortCategory, bool isDynamic)
        {
            directionSet = definition != null ? definition.Idle : null;
            Initialize(sprite, definition != null ? definition.WorldSize : Vector2.one, sortCategory, isDynamic);
        }

        public void SetDirectionFromWorldMovement(Vector3 movement)
        {
            var next = SpriteDirectionResolver.FromMovement(new Vector2(movement.x, movement.z));
            if (next == direction) return;
            direction = next;
            Sprite sprite;
            if (directionSet != null && directionSet.TryGet(direction, out sprite) && spriteRenderer != null) spriteRenderer.sprite = sprite;
        }

        void LateUpdate() { Refresh(false); }

        void Refresh(bool force)
        {
            if (cameraTransform == null && UnityEngine.Camera.main != null) cameraTransform = UnityEngine.Camera.main.transform;
            if (cameraTransform == null) return;
            transform.rotation = cameraTransform.rotation;
            if (sortingGroup == null) sortingGroup = GetComponent<SortingGroup>();
            if (sortingGroup != null) sortingGroup.sortingOrder = SpriteWorldSorting.Calculate(transform.position, cameraTransform.forward, category);
            if (!dynamicVisual && !force) enabled = false;
        }
    }
}
