using UnityEngine;

public class billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        // Find and store a reference to the main camera to be more efficient.
        mainCamera = Camera.main;
    }

    // LateUpdate runs after all other Update calls, which is perfect for camera work.
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // This makes the object's forward vector point towards the camera.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}