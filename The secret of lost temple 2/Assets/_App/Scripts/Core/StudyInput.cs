using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

// Shared pointer access for the AR scripts when Android uses only the new Input System.
public static class StudyInput
{
#if ENABLE_INPUT_SYSTEM
    static TouchControl TouchAt(int index)
    {
        if (Touchscreen.current != null)
            foreach (var touch in Touchscreen.current.touches)
                if (touch.press.isPressed || touch.press.wasReleasedThisFrame)
                    if (index-- == 0) return touch;
        return null;
    }
    public static int touchCount
    {
        get { int count = 0; if (Touchscreen.current != null) foreach (var t in Touchscreen.current.touches)
            if (t.press.isPressed || t.press.wasReleasedThisFrame) count++; return count; }
    }
    public static Touch GetTouch(int index)
    {
        var t = TouchAt(index);
        if (t == null) throw new System.ArgumentOutOfRangeException(nameof(index));
        var phase = t.phase.ReadValue();
        return new Touch { fingerId = t.touchId.ReadValue(), position = t.position.ReadValue(), deltaPosition = t.delta.ReadValue(),
            phase = phase == UnityEngine.InputSystem.TouchPhase.Began ? UnityEngine.TouchPhase.Began :
                phase == UnityEngine.InputSystem.TouchPhase.Moved ? UnityEngine.TouchPhase.Moved :
                phase == UnityEngine.InputSystem.TouchPhase.Ended ? UnityEngine.TouchPhase.Ended :
                phase == UnityEngine.InputSystem.TouchPhase.Canceled ? UnityEngine.TouchPhase.Canceled : UnityEngine.TouchPhase.Stationary };
    }
    public static Vector3 mousePosition => TouchAt(0) != null ? (Vector3)TouchAt(0).position.ReadValue() :
        Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
    public static bool GetMouseButton(int button) => button == 0 &&
        ((Touchscreen.current?.primaryTouch.press.isPressed ?? false) || (Mouse.current?.leftButton.isPressed ?? false));
    public static bool GetMouseButtonDown(int button) => button == 0 &&
        ((Touchscreen.current?.primaryTouch.press.wasPressedThisFrame ?? false) || (Mouse.current?.leftButton.wasPressedThisFrame ?? false));
    public static bool anyKeyDown => (Keyboard.current?.anyKey.wasPressedThisFrame ?? false) || GetMouseButtonDown(0);
    public static float GetAxis(string axis) => Mouse.current == null ? 0 : axis == "Mouse ScrollWheel" ? Mouse.current.scroll.ReadValue().y / 1200f :
        axis == "Mouse X" ? Mouse.current.delta.ReadValue().x * .1f : 0;
    public static bool GetKeyDown(KeyCode key) => Keyboard.current != null &&
        (key == KeyCode.Space ? Keyboard.current.spaceKey.wasPressedThisFrame : key == KeyCode.Escape && Keyboard.current.escapeKey.wasPressedThisFrame);
#else
    public static int touchCount => Input.touchCount;
    public static Touch GetTouch(int index) => Input.GetTouch(index);
    public static Vector3 mousePosition => Input.mousePosition;
    public static bool GetMouseButton(int button) => Input.GetMouseButton(button);
    public static bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);
    public static bool anyKeyDown => Input.anyKeyDown;
    public static float GetAxis(string axis) => Input.GetAxis(axis);
    public static bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
#endif
}
