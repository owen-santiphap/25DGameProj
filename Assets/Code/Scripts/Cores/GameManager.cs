using UnityEngine;
using UnityEngine.SceneManagement; // Make sure this is included

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameTimerDuration = 120f;

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
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
  
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeGame();
    }
    
    public void RegisterUIManager(UIManager manager)
    {
        uiManager = manager;
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
        uiManager.UpdateScore(Score);
        uiManager.UpdateTimer(CurrentTime);
        uiManager.UpdateHighScore(_highScore);
    }
    
    private void InitializeGame()
    {
        _isGameOver = false;
        Score = 0;
        CurrentTime = gameTimerDuration;
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<HealthSystem>().OnDeath.RemoveListener(OnPlayerDied);
            player.GetComponent<HealthSystem>().OnDeath.AddListener(OnPlayerDied);
        }
    }
    
    private void Update()
    {
        if (_isGameOver) return;

        CurrentTime -= Time.deltaTime;
        if (uiManager != null)
        {
            uiManager.UpdateTimer(CurrentTime);
        }

        if (CurrentTime <= 0)
        {
            GameOver("Time's Up!");
        }
    }

    public void AddScore(int amount)
    {
        if (_isGameOver) return;
        Score += amount;
        if (uiManager != null)
        {
            uiManager.UpdateScore(Score);
        }
    }

    private void OnPlayerDied()
    {
        GameOver("You Died!");
    }

    private void GameOver(string reason)
    {
        _isGameOver = true;
        Time.timeScale = 0f;

        if (Score > _highScore)
        {
            _highScore = Score;
            PlayerPrefs.SetInt("HighScore", _highScore);
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOverScreen(reason, Score, _highScore);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnMenu()
    {
        SceneManager.LoadScene(0);
    }
}