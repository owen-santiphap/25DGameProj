using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Add this namespace

public class MainMenuInput : MonoBehaviour
{
    private bool _isLoading = false;

    void Update()
    {
        if (_isLoading) return;
        
        var anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        
        var anyMousePressed = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame);

        if (anyKeyPressed || anyMousePressed)
        {
            Debug.Log("Input detected. Starting scene load process.");
            _isLoading = true;
            
            // Call the static instance of SceneLoader.
            SceneLoader.Instance.LoadGameScenes();
        }
    }
}