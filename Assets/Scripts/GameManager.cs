using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance { get; private set; }

    // 当前关卡索引
    public int CurrentLevel { get; private set; } = 1;

    // 总关卡数
    public int TotalLevels { get; private set; } = 10; // 可以根据实际关卡数调整

    // 游戏状态
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelComplete
    }

    public GameState CurrentGameState { get; private set; } = GameState.MainMenu;

    // 计时器相关
    private float levelStartTime = 0f;
    private float levelElapsedTime = 0f;
    private bool isTimerRunning = false;

    // 事件
    public System.Action<int> OnLevelChanged;
    public System.Action<GameState> OnGameStateChanged;

    [Header("Loading Screen")]
    public CanvasGroup loadingScreen;
    public float fadeDuration = 0.5f;
    public float minLoadingTime = 1.0f; // 最小加载时间，确保黑屏至少显示这么久

    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        // 初始化游戏设置
        CurrentLevel = 1;
        CurrentGameState = GameState.MainMenu;

        // 初始化加载界面
        if (loadingScreen != null)
        {
            loadingScreen.alpha = 0f;
            loadingScreen.gameObject.SetActive(false);
        }
    }

    // 开始新游戏
    public void StartNewGame()
    {
        CurrentLevel = 1;
        LoadLevel(CurrentLevel);
        SetGameState(GameState.Playing);
    }

    // 加载指定关卡
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 1 && levelIndex <= TotalLevels)
        {
            CurrentLevel = levelIndex;
            OnLevelChanged?.Invoke(CurrentLevel);
            SetGameState(GameState.Playing);
            ResetTimer(); // 重置计时器
            StartCoroutine(LoadLevelCoroutine(levelIndex));
        }
        else
        {
            Debug.LogWarning($"关卡 {levelIndex} 不存在！");
        }
    }

    // 关卡加载协程
    private IEnumerator LoadLevelCoroutine(int levelIndex)
    {
        string sceneName = $"Level{levelIndex}";
        yield return StartCoroutine(LoadSceneCoroutine(sceneName));
        Debug.Log($"加载关卡 {levelIndex} 完成");

        // 关卡加载完成后开始计时
        StartTimer();
    }

    // 通用场景加载协程
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 显示加载界面（黑屏）
        yield return StartCoroutine(FadeToBlack());

        // 记录开始时间
        float startTime = Time.unscaledTime;

        // 检查是否是重新加载当前场景
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool isReloadingCurrentScene = (currentSceneName == sceneName);

        if (isReloadingCurrentScene)
        {
            // 重新加载当前场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            // 先加载新场景（Additive模式）
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 等待一帧确保场景完全加载
            yield return null;

            // 卸载旧场景
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentSceneName);

            // 等待场景卸载完成
            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            // 强制垃圾回收，清理场景资源
            System.GC.Collect();
            yield return null;

        }

        // 确保至少显示最小加载时间
        float elapsedTime = Time.unscaledTime - startTime;
        if (elapsedTime < minLoadingTime)
        {
            yield return new WaitForSecondsRealtime(minLoadingTime - elapsedTime);
        }

        // 隐藏加载界面（亮屏）
        Time.timeScale = 1f;
        yield return StartCoroutine(FadeToClear());
    }

    // 淡入黑屏
    private IEnumerator FadeToBlack()
    {
        if (loadingScreen == null) yield break;

        loadingScreen.gameObject.SetActive(true);
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            loadingScreen.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        loadingScreen.alpha = 1f;
    }

    // 淡出亮屏
    private IEnumerator FadeToClear()
    {
        if (loadingScreen == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            loadingScreen.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        loadingScreen.alpha = 0f;
        loadingScreen.gameObject.SetActive(false);
    }

    // 下一关
    public void NextLevel()
    {
        if (CurrentLevel < TotalLevels)
        {
            LoadLevel(CurrentLevel + 1);
        }
        else
        {
            // 游戏通关
            GameComplete();
        }
    }

    // 重新开始当前关卡
    public void RestartLevel()
    {
        LoadLevel(CurrentLevel);
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        CurrentGameState = GameState.MainMenu;
        OnGameStateChanged?.Invoke(CurrentGameState);
        StopTimer(); // 停止计时
        StartCoroutine(LoadMainMenuCoroutine());
    }

    // 加载主菜单协程
    private IEnumerator LoadMainMenuCoroutine()
    {
        yield return StartCoroutine(LoadSceneCoroutine("MainMenu"));
    }

    // 暂停游戏
    public void PauseGame()
    {
        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
        PauseTimer(); // 暂停计时
    }

    // 恢复游戏
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
        ResumeTimer(); // 恢复计时
    }

    // 关卡完成
    public void LevelComplete()
    {
        SetGameState(GameState.LevelComplete);
        Time.timeScale = 0f;
        StopTimer(); // 停止计时
        // 可以在这里添加关卡完成的效果、音效等
    }

    // 游戏通关
    public void GameComplete()
    {
        Debug.Log("恭喜通关！");
        StopTimer(); // 停止计时
        // 可以在这里添加通关奖励、返回主菜单等逻辑
        ReturnToMainMenu();
    }

    // 设置游戏状态
    private void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(CurrentGameState);
    }

    // 获取关卡进度百分比
    public float GetLevelProgress()
    {
        return (float)CurrentLevel / TotalLevels;
    }

    // 检查是否是最后一关
    public bool IsLastLevel()
    {
        return CurrentLevel >= TotalLevels;
    }

    // 计时器相关方法
    /// <summary>
    /// 获取当前关卡用时（秒）
    /// </summary>
    public float GetCurrentLevelTime()
    {
        if (isTimerRunning)
        {
            return Time.time - levelStartTime;
        }
        return levelElapsedTime;
    }

    /// <summary>
    /// 获取格式化的时间字符串 (MM:SS)
    /// </summary>
    public string GetFormattedTime()
    {
        float time = GetCurrentLevelTime();
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// 开始计时
    /// </summary>
    private void StartTimer()
    {
        levelStartTime = Time.time;
        isTimerRunning = true;
        Debug.Log("关卡计时开始");
    }

    /// <summary>
    /// 暂停计时
    /// </summary>
    private void PauseTimer()
    {
        if (isTimerRunning)
        {
            levelElapsedTime = Time.time - levelStartTime;
            isTimerRunning = false;
            Debug.Log($"关卡计时暂停，当前用时: {GetFormattedTime()}");
        }
    }

    /// <summary>
    /// 恢复计时
    /// </summary>
    private void ResumeTimer()
    {
        if (!isTimerRunning && CurrentGameState == GameState.Playing)
        {
            levelStartTime = Time.time - levelElapsedTime;
            isTimerRunning = true;
            Debug.Log("关卡计时恢复");
        }
    }

    /// <summary>
    /// 停止计时
    /// </summary>
    private void StopTimer()
    {
        if (isTimerRunning)
        {
            levelElapsedTime = Time.time - levelStartTime;
            isTimerRunning = false;
            Debug.Log($"关卡计时结束，总用时: {GetFormattedTime()}");
        }
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    private void ResetTimer()
    {
        levelElapsedTime = 0f;
        isTimerRunning = false;
        Debug.Log("关卡计时器重置");
    }
}
