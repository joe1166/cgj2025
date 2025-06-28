using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 关卡完成界面控制器
/// </summary>
public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI levelText; // 显示关卡号
    public TextMeshProUGUI timeText; // 显示完成用时


    private void OnEnable()
    {
        // 界面显示时更新信息
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // 更新关卡号
        if (levelText != null)
        {
            levelText.text = $"第 {GameManager.Instance.CurrentLevel} 关";
        }

        // 更新完成用时
        if (timeText != null)
        {
            timeText.text = $"完成用时: {GameManager.Instance.GetFormattedTime()}";
        }

    }


}