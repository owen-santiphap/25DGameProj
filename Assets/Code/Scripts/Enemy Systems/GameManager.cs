using UnityEngine;
using UnityEngine.SceneManagement; // To restart the game

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameTimerDuration = 120f; // 2 minutes

    [Header("UI References")]
    [SerializeField] private UIManager uiManager;

    public float CurrentTime { get; private set; }
    public int Score { get; private set; }
    private int _highScore;

    private bool _isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want it to persist across scenes
        }
    }

    private void Start()
    {
        CurrentTime = gameTimerDuration;
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
        uiManager.UpdateHighScore(_highScore);

        // Find the player and subscribe to their death event
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<HealthSystem>().OnDeath.AddListener(OnPlayerDied);
        }
    }

    private void Update()
    {
        if (_isGameOver) return;

        CurrentTime -= Time.deltaTime;
        uiManager.UpdateTimer(CurrentTime);

        if (CurrentTime <= 0)
        {
            GameOver("Time's Up!");
        }
    }

    public void AddScore(int amount)
    {
        if (_isGameOver) return;
        Score += amount;
        uiManager.UpdateScore(Score);
    }

    private void OnPlayerDied()
    {
        GameOver("You Died!");
    }

    private void GameOver(string reason)
    {
        _isGameOver = true;
        Time.timeScale = 0f; // Pause the game

        if (Score > _highScore)
        {
            _highScore = Score;
            PlayerPrefs.SetInt("HighScore", _highScore);
        }

        uiManager.ShowGameOverScreen(reason, Score, _highScore);
    }
    
    // Call this from a UI button to restart
    public void RestartGame()
    {
        if (!_isGameOver) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}