using System.Collections;           
using TMPro;                        
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;     // current score label
    [SerializeField] private TextMeshProUGUI hiScoreText;   // best score label
    [SerializeField] private CanvasGroup gameOverOverlay;   // fade-in on game over

    private void OnEnable()
    {
        // Subscribe once the manager exists (UI reacts to game events)
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
        GameManager.Instance.OnGameStarted += HandleGameStarted;
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        // Safe unsubscribe (Instance may be null on teardown)
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        GameManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        // Paint initial UI from current manager state
        HandleScoreChanged(GameManager.Instance.Score);
        HandleHighScoreChanged(GameManager.Instance.HighScore);

        // Ensure overlay is hidden at boot
        if (gameOverOverlay != null)
        {
            gameOverOverlay.alpha = 0f;
            gameOverOverlay.interactable = false;
            gameOverOverlay.blocksRaycasts = false;
        }
    }


    // === Event handlers ===

    private void HandleScoreChanged(int s)
    {
        if (scoreText != null) scoreText.text = s.ToString(); 
    }

    private void HandleHighScoreChanged(int hs)
    {
        if (hiScoreText != null) hiScoreText.text = hs.ToString();
    }

    private void HandleGameStarted()
    {
        // Hide overlay instantly, disable interactions
        if (gameOverOverlay != null)
        {
            gameOverOverlay.interactable = false;
            gameOverOverlay.blocksRaycasts = false;
            StartCoroutine(Fade(gameOverOverlay, 0f, 0f)); 
        }
 }

    private void HandleGameOver()
    {
        // Show overlay with a short fade and enable clicks
        if (gameOverOverlay != null)
        {
            gameOverOverlay.interactable = true;
            gameOverOverlay.blocksRaycasts = true;
            StartCoroutine(Fade(gameOverOverlay, 1f, 0.5f)); 
        }
    }

    //overlay fade 
    private static IEnumerator Fade(CanvasGroup g, float to, float duration)
    {
        var from = g.alpha; var t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / duration); 
            yield return null;
        }
        g.alpha = to; 
    }
}
