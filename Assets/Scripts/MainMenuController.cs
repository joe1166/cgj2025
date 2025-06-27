using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button startGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Level Selection")]
    public GameObject levelSelectionPanel;
    public Button closeLevelSelectionButton;

    private void Start()
    {
        SetupUI();
        UpdateUI();
    }

    private void SetupUI()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => OpenLevelSelection());

        if (closeLevelSelectionButton != null)
            closeLevelSelectionButton.onClick.AddListener(() => CloseLevelSelection());

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => OpenSettings());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => QuitGame());
    }

    private void UpdateUI()
    {
        // 初始化关卡选择面板
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }
    }

    private void OpenLevelSelection()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
        }
    }

    private void CloseLevelSelection()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }
    }

    // 加载指定关卡
    public void LoadLevel(int levelIndex)
    {
        GameManager.Instance.LoadLevel(levelIndex);
        CloseLevelSelection();
    }

    private void OpenSettings()
    {
        Debug.Log("打开设置菜单");
        // 在这里实现设置菜单逻辑
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // 示例：检查关卡是否解锁（可以基于玩家进度或成就系统）
    private bool IsLevelUnlocked(int levelIndex)
    {
        // 这里可以实现关卡解锁逻辑
        // 例如：基于玩家最高通关关卡
        return levelIndex <= GetHighestUnlockedLevel();
    }

    private int GetHighestUnlockedLevel()
    {
        // 这里可以从PlayerPrefs或其他数据源获取最高解锁关卡
        // 示例：从PlayerPrefs读取
        return PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
    }

    // 示例：保存游戏进度
    public void SaveGameProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", GameManager.Instance.CurrentLevel);
        PlayerPrefs.SetInt("HighestUnlockedLevel", Mathf.Max(
            GetHighestUnlockedLevel(),
            GameManager.Instance.CurrentLevel
        ));
        PlayerPrefs.Save();
        Debug.Log("游戏进度已保存");
    }

    // 示例：加载游戏进度
    public void LoadGameProgress()
    {
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        // 这里可以添加逻辑来恢复游戏状态
        Debug.Log($"加载保存的关卡: {savedLevel}");
    }
}