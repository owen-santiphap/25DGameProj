using UnityEngine;
using UnityEngine.InputSystem;

public class InputRelay : MonoBehaviour
{
    public void OnRestart(InputAction.CallbackContext context)
    {
        if (context.performed && GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed && GameManager.Instance != null)
        {
            GameManager.Instance.ReturnMenu();
        }
    }
}