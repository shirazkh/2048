using System;
using UnityEngine;

[DefaultExecutionOrder(-1)] // ensure manager is initialized before other scene scripts
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private TileBoard board; // gameplay entry point

    public int Score { get; private set; }        // current run score
    public int HighScore { get; private set; }    // persisted best
    public bool GameStarted { get; private set; } // gates input/UI
    public bool GameOverFlag { get; private set; }

    // UI listens to these; GM never touches UI components directly
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;
    public event Action OnGameStarted;
    public event Action OnGameOver;

    private void Start()
    {
        // Load persisted high score and notify UI once
        HighScore = PlayerPrefs.GetInt(Constants.HighScoreKey, 0);
        OnHighScoreChanged?.Invoke(HighScore);

        NewGame(); // auto-start
    }

    public void NewGame()
    {
        GameStarted = true;
        GameOverFlag = false;

        SetScore(0);           // reset and notify UI
        OnGameStarted?.Invoke();

        board.ClearBoard();    // wipe tiles 
        board.CreateTile();    // first two tiles
        board.CreateTile();
        board.enabled = true;  // allow input
    }

    public void IncreaseScore(int points) // called from merges
    {
        SetScore(Score + points);
    }

    public void GameOver() // called when no moves left
    {
        board.enabled = false; // freeze board updates
        GameOverFlag = true;
        OnGameOver?.Invoke();  // UI overlay / retry button reacts
    }

    // Central score setter: fires events + persists high score
    private void SetScore(int value)
    {
        Score = value;
        OnScoreChanged?.Invoke(Score);

        if (Score > HighScore)
        {
            HighScore = Score;
            OnHighScoreChanged?.Invoke(HighScore);
            PlayerPrefs.SetInt(Constants.HighScoreKey, HighScore); // persist
            PlayerPrefs.Save();
        }
    }
}
