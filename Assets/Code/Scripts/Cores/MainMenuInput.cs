using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Add this namespace

public class MainMenuInput : MonoBehaviour
{
    private bool isLoading = false;

    void Update()
    {
        // Exit early if already in the process of loading.
        if (isLoading) return;

        // --- 2. This is the new input check ---
        // It checks if any keyboard key was pressed this frame.
        var anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        // checks if any mouse button was pressed this frame.
        var anyMousePressed = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame);

        if (anyKeyPressed || anyMousePressed)
        {
            Debug.Log("Input detected. Starting scene load process.");
            isLoading = true;
            
            // Call the static instance of SceneLoader.
            SceneLoader.Instance.LoadGameScenes();
        }
    }
}