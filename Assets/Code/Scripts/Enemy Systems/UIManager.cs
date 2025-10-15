using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("In-Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverReasonText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
    {
        gameOverPanel.SetActive(false); // Hide it at the start
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateTimer(float time)
    {
        time = Mathf.Max(0, time); // Don't show negative time
        var minutes = (int)time / 60;
        var seconds = (int)time % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    
    public void UpdateHighScore(int score)
    {
        highScoreText.text = "High Score: " + score;
    }

    public void ShowGameOverScreen(string reason, int finalScore, int highScore)
    {
        gameOverPanel.SetActive(true);
        gameOverReasonText.text = reason;
        finalScoreText.text = "Your Score: " + finalScore;
        highScoreText.text = "High Score: " + highScore;
    }
}