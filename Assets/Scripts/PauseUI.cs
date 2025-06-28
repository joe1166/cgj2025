using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 暂停界面控制器
/// </summary>
public class PauseUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI timeText; // 显示当前用时的文本

    private void Update()
    {
        // 实时更新时间显示
        if (timeText != null && GameManager.Instance != null)
        {
            timeText.text = $"当前用时: {GameManager.Instance.GetFormattedTime()}";
        }
    }

}