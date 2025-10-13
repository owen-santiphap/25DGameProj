using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // A static reference to this script so it can be called from anywhere.
    public static SceneLoader Instance;

    private void Awake()
    {
        // --- Singleton Pattern ---
        // If an instance already exists and it's not this one, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Otherwise, set the instance to this and make it persist across scenes.
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Public method to be called by other scripts (like our Main Menu).
    public void LoadGameScenes()
    {
        // Start the coroutine that handles the multi-scene loading.
        StartCoroutine(LoadSequence());
    }

    private IEnumerator LoadSequence()
    {
        // 1. Load the buffer scene first.
        // LoadSceneAsync allows the game to not freeze while loading.
        AsyncOperation asyncLoadBuffer = SceneManager.LoadSceneAsync("Buffer");

        // Wait until the buffer scene is fully loaded.
        while (!asyncLoadBuffer.isDone)
        {
            // You could update a loading bar here if you had one.
            yield return null;
        }

        // 2. Load the main gameplay scene.
        // The buffer scene is now active, providing a clean slate.
        AsyncOperation asyncLoadGame = SceneManager.LoadSceneAsync("Gameplay");
        
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