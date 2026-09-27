using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager: timer, scoring (with combo), fire tracking, and UI.
/// Singleton — access via GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Serialized Fields (names match existing scene references) ----
    [Header("UI Panels")]
    public GameObject gameover;
    public GameObject pauseMenuScreeen;

    [Header("UI Text")]
    public Text scoreText;
    public Text timerText;

    [Header("Game Settings")]
    public float gameDuration = 60f;

    [Header("Combo System")]
    [Tooltip("Seconds between extinguishes to maintain combo")]
    public float comboWindow = 5f;

    [Tooltip("Maximum combo multiplier")]
    public int comboMultiplierMax = 5;

    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- Runtime State ----
    private float timer;
    private int score;
    private int comboCount;
    private float lastExtinguishTime = -999f;
    private int totalFires;
    private int firesExtinguished;
    private bool isGameOver;

    // Registered fires
    private readonly List<FireScript> registeredFires = new List<FireScript>();

    // ================================================================
    //  LIFECYCLE
    // ================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        timer = gameDuration;
        score = 0;
        comboCount = 0;
        firesExtinguished = 0;
        isGameOver = false;
        Time.timeScale = 1f;

        UpdateUI();
    }

    void Update()
    {
        if (isGameOver) return;

        // Countdown timer
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                GameOver();
            }
            UpdateUI();
        }
    }

    // ================================================================
    //  FIRE TRACKING
    // ================================================================

    /// <summary>
    /// Called by each FireScript.Start() to register itself.
    /// Used for tracking total fires and win condition.
    /// </summary>
    public void RegisterFire(FireScript fire)
    {
        if (fire == null || registeredFires.Contains(fire)) return;
        registeredFires.Add(fire);
        totalFires++;
    }

    /// <summary>
    /// Called by FireScript when it is fully extinguished.
    /// Handles combo multiplier and win condition check.
    /// </summary>
    public void OnFireExtinguished(int baseScore)
    {
        if (isGameOver) return;

        firesExtinguished++;

        // ---- Combo Scoring ----
        if (Time.time - lastExtinguishTime < comboWindow)
        {
            comboCount = Mathf.Min(comboCount + 1, comboMultiplierMax);
        }
        else
        {
            comboCount = 1;
        }
        lastExtinguishTime = Time.time;

        int finalScore = baseScore * comboCount;
        score += finalScore;

        // Log combo info for debugging
        if (comboCount > 1)
            Debug.Log($"COMBO x{comboCount}! +{finalScore} points");

        UpdateUI();

        // ---- Win Condition: all fires out ----
        if (totalFires > 0 && firesExtinguished >= totalFires)
        {
            LevelComplete();
        }
    }

    /// <summary>
    /// Legacy AddScore method — kept for backward compatibility.
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points;
        UpdateUI();
    }

    // ================================================================
    //  UI
    // ================================================================

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(timer);
            timerText.text = $"Time: {seconds}s";

            // Urgency: turn red in last 10 seconds
            if (timer <= 10f)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }

    // ================================================================
    //  GAME FLOW
    // ================================================================

    void GameOver()
    {
        isGameOver = true;
        if (gameover != null)
            gameover.SetActive(true);
    }

    void LevelComplete()
    {
        isGameOver = true;

        // Time bonus: remaining seconds * 2
        int timeBonus = Mathf.FloorToInt(timer) * 2;
        score += timeBonus;
        Debug.Log($"LEVEL COMPLETE! Time bonus: +{timeBonus}");

        UpdateUI();

        if (gameover != null)
            gameover.SetActive(true);
    }

    // ================================================================
    //  BUTTON CALLBACKS (names match existing scene references)
    // ================================================================

    public void restartgame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseMenuScreeen != null)
            pauseMenuScreeen.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseMenuScreeen != null)
            pauseMenuScreeen.SetActive(false);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }
}