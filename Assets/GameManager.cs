using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEditor.Experimental.GraphView;
public class GameManager : MonoBehaviour
{
    public GameObject gameover;
    public GameObject pauseMenuScreeen;
    public GameManager(GameObject gameover)
    {
        this.gameover = gameover;
    }

    public static GameManager Instance;
    
    public Text scoreText;
    public Text timerText;
    public float gameDuration = 60f; // Adjust the game duration in seconds

    private float timer;
    private int score;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        timer = gameDuration;
        UpdateUI();
        InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
    }

    void UpdateTimer()
    {
        if (timer > 0)
        {
            timer -= 1f;
            UpdateUI();
        }
        else if (timer <= 0)
        {
            gameover.SetActive(true); // This will enable the GameOver GameObject
            CancelInvoke(nameof(UpdateTimer));
        }
        
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        timerText.text = $"Time: {Mathf.FloorToInt(timer)}s";
    }

    public void restartgame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenuScreeen.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenuScreeen.SetActive(false);
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene(1);

    }
}