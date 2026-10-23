using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandSurvival.InputSystem
{
    public sealed class MobileInput : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool InteractPressed { get; set; }
        public Vector2 JoystickOrigin { get; private set; }
        public Vector2 JoystickPosition { get; private set; }
        public bool JoystickVisible { get { return moveFinger >= 0; } }
        int moveFinger = -1;
        int lookFinger = -1;
        Vector3 lastMousePosition;
        bool sprintButtonHeld;
        const float JoystickRadius = 90f;

        void Update()
        {
            LookDelta = Vector2.zero;
            ReadTouch();
            if (moveFinger < 0) Move = Vector2.zero;
            SprintHeld = sprintButtonHeld;
#if UNITY_EDITOR || UNITY_STANDALONE
            var keyboard = new Vector2(
                (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f),
                (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow) ? 1f : 0f));
            if (keyboard.sqrMagnitude > 0.01f) Move = Vector2.ClampMagnitude(keyboard, 1f);
            if (UnityEngine.Input.GetMouseButton(1))
            {
                var delta = UnityEngine.Input.mousePosition - lastMousePosition;
                LookDelta = new Vector2(delta.x, -delta.y);
            }
            lastMousePosition = UnityEngine.Input.mousePosition;
            SprintHeld = UnityEngine.Input.GetKey(KeyCode.LeftShift) || sprintButtonHeld;
            if (UnityEngine.Input.GetKeyDown(KeyCode.E)) InteractPressed = true;
#endif
        }

        void LateUpdate()
        {
            InteractPressed = false;
        }

        public void SetSprintButton(bool held) { sprintButtonHeld = held; }

        void ReadTouch()
        {
            foreach (var touch in UnityEngine.Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;
                    if (touch.position.x < Screen.width * 0.45f && touch.position.y < Screen.height * 0.72f && moveFinger < 0)
                    { moveFinger = touch.fingerId; JoystickOrigin = touch.position; JoystickPosition = touch.position; }
                    else if (touch.position.x >= Screen.width * 0.45f && lookFinger < 0) lookFinger = touch.fingerId;
                }
                if (touch.fingerId == moveFinger)
                {
                    JoystickPosition = touch.position;
                    Move = Vector2.ClampMagnitude((touch.position - JoystickOrigin) / JoystickRadius, 1f);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) { moveFinger = -1; Move = Vector2.zero; }
                }
                else if (touch.fingerId == lookFinger)
                {
                    if (touch.phase == TouchPhase.Moved) LookDelta += new Vector2(touch.deltaPosition.x, -touch.deltaPosition.y);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) lookFinger = -1;
                }
            }
        }
    }
}
