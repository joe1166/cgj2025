using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public Button restartButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button mainMenuButton;

    [Header("Level Settings")]
    public GameObject levelCompletePanel;
    public GameObject pausePanel;

    private void Start()
    {
        // 订阅事件
        GameManager.Instance.OnLevelChanged += OnLevelChanged;
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        // 设置UI按钮事件
        SetupUI();

        // 更新UI显示
        UpdateUI();
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelChanged -= OnLevelChanged;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    private void SetupUI()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance.RestartLevel());

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => GameManager.Instance.PauseGame());

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance.ResumeGame());

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
    }

    private void OnLevelChanged(int newLevel)
    {
        Debug.Log($"切换到关卡 {newLevel}");
        UpdateUI();
    }

    private void OnGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"游戏状态改变为: {newState}");
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 更新关卡文本
        if (levelText != null)
        {
            levelText.text = $"关卡 {GameManager.Instance.CurrentLevel} / {GameManager.Instance.TotalLevels}";
        }

        // 根据游戏状态显示/隐藏UI元素
        switch (GameManager.Instance.CurrentGameState)
        {
            case GameManager.GameState.Playing:
                ShowPlayingUI();
                break;
            case GameManager.GameState.Paused:
                ShowPausedUI();
                break;
            case GameManager.GameState.LevelComplete:
                ShowLevelCompleteUI();
                break;
        }
    }

    private void ShowPlayingUI()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (resumeButton != null) resumeButton.gameObject.SetActive(false);
    }

    private void ShowPausedUI()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
        if (resumeButton != null) resumeButton.gameObject.SetActive(true);
    }

    private void ShowLevelCompleteUI()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    // 示例：当玩家完成关卡目标时调用
    public void CompleteLevel()
    {
        GameManager.Instance.LevelComplete();
    }

    // 示例：检查关卡进度
    public void CheckLevelProgress()
    {
        float progress = GameManager.Instance.GetLevelProgress();
        Debug.Log($"当前进度: {progress * 100:F1}%");
    }
}