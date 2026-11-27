using UnityEngine;
using UnityEngine.InputSystem; // Додали нову бібліотеку

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    public Camera cam;

    [Header("Zoom Settings")]
    public float minZoom = 2f;
    public float maxZoom = 50f;
    // Значення скролу у новій системі більше (120 замість 0.1), тому чутливість сильно зменшуємо
    public float zoomSensitivity = 0.05f; 

    [Header("Pan Settings")]
    public float keyboardPanSpeed = 15f;
    
    private Vector3 dragOrigin;

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandleMousePan();
    }

    private void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSensitivity;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    private void HandleMousePan()
    {
        if (Mouse.current == null) return;

        // Перевіряємо середню та праву кнопки миші
        bool isMiddleDown = Mouse.current.middleButton.wasPressedThisFrame;
        bool isRightDown = Mouse.current.rightButton.wasPressedThisFrame;
        bool isMiddlePressed = Mouse.current.middleButton.isPressed;
        bool isRightPressed = Mouse.current.rightButton.isPressed;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (isMiddleDown || isRightDown)
        {
            dragOrigin = cam.ScreenToWorldPoint(mousePos);
        }

        if (isMiddlePressed || isRightPressed)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(mousePos);
            Vector3 difference = dragOrigin - currentPos;
            cam.transform.position += difference;
        }
    }

    public void SetupCameraLimits(int mapSize)
    {
        maxZoom = mapSize * 0.6f; 
        if (cam.orthographicSize > maxZoom) 
        {
            cam.orthographicSize = maxZoom;
        }
    }
}