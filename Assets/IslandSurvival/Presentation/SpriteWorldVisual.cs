using UnityEngine;
using UnityEngine.Rendering;
using IslandSurvival.Visual;

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
        VisualAssetKey assetKey;
        SpriteDirection direction = SpriteDirection.S;
        bool moving;
        bool sprinting;
        int animationFrame = -1;

        public SpriteDirection Direction { get { return direction; } }

        public void Initialize(Sprite sprite, Vector2 worldSize, SpriteSortCategory sortCategory, bool isDynamic, VisualAssetKey key = VisualAssetKey.None)
        {
            assetKey = key;
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
            Initialize(sprite, definition != null ? definition.WorldSize : Vector2.one, sortCategory, isDynamic, definition != null ? definition.AssetKey : VisualAssetKey.None);
        }

        public void SetDirectionFromWorldMovement(Vector3 movement)
        {
            var next = SpriteDirectionResolver.FromMovement(new Vector2(movement.x, movement.z));
            if (next == direction) return;
            direction = next;
            Sprite sprite;
            if (spriteRenderer == null) return;
            if (directionSet != null && directionSet.TryGet(direction, out sprite)) spriteRenderer.sprite = sprite;
            else if (PresentationRuntime.PlaceholderArtEnabled && TemporaryArtLibrary.TryGet(assetKey, direction, out sprite)) spriteRenderer.sprite = sprite;
        }

        public void SetMovement(bool isMoving, bool isSprinting)
        {
            if (moving == isMoving && sprinting == isSprinting) return;
            moving = isMoving; sprinting = isSprinting; animationFrame = -1;
            if (!moving) ApplyIdleSprite();
        }

        void LateUpdate()
        {
            Refresh(false);
            if (dynamicVisual && moving && assetKey == VisualAssetKey.PlayerCharacter)
            {
                var frame = Mathf.FloorToInt(Time.unscaledTime * (sprinting ? 8f : 5f)) & 1;
                if (frame != animationFrame)
                {
                    animationFrame = frame; Sprite sprite;
                    if (TemporaryArtLibrary.TryGetWalk(direction, frame, out sprite) && spriteRenderer != null) spriteRenderer.sprite = sprite;
                }
            }
        }

        void ApplyIdleSprite()
        {
            Sprite sprite;
            if (spriteRenderer != null && TemporaryArtLibrary.TryGet(assetKey, direction, out sprite)) spriteRenderer.sprite = sprite;
        }

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
