using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            Debug.Log("F5 pressed. Reloading to Main Menu.");
            LoadMainMenu();
        }
    }
    // Public method to be called by other scripts
    public void LoadGameScenes()
    {
        StartCoroutine(LoadSequence());
        // Start the coroutine that handles the multi-scene loading.
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator LoadSequence()
    {
        var asyncLoadBuffer = SceneManager.LoadSceneAsync("Buffer");
        
        while (!asyncLoadBuffer.isDone)
        {
            yield return null;
        }
        
        var asyncLoadGame = SceneManager.LoadSceneAsync("Gameplay");
        
        // Wait until the gameplay scene is fully loaded.
        while (!asyncLoadGame.isDone)
        {
            yield return null;
        }

        // --- Optional Cleanup ---
        // If you want to unload the buffer scene after the gameplay scene is loaded,
        // you would need to use SceneManager.LoadScene("GameplayScene", LoadSceneMode.Additive)
        // and then SceneManager.UnloadSceneAsync("BufferScene").
        // For this simple case, just replacing the scene is fine.
    }
}