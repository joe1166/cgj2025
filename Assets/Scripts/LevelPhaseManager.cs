using UnityEngine;
using System.Collections.Generic;

public enum PhaseType { Timer, ExternalSignal }

// [CreateAssetMenu(menuName = "Level/PhaseData")]
[System.Serializable]
public class PhaseData 
// : ScriptableObject
{
    public string phaseName;
    public PhaseType phaseType;
    public float case_timer_Duration;
}

public class LevelPhaseManager : MonoBehaviour
{

    private int currentIndex = 0;
    private float timer = 0f;
    // private float timer = 3f;
    private bool waitingForSignal = false;


    [Header("Phase GameObjects")]
    public List<PhaseData> phaseSequence = new List<PhaseData>
    {
        new PhaseData
        {
            phaseName = "Game",
            phaseType = PhaseType.Timer,
            case_timer_Duration = 3f
        },
        new PhaseData
        {
            phaseName = "Main",
            phaseType = PhaseType.ExternalSignal,
            // phaseType = PhaseType.Timer,
            case_timer_Duration = 100f
        },
        // new PhaseData
        // {
        //     phaseName = "Main",
        //     phaseType = PhaseType.Timer,
        //     // phaseType = PhaseType.Timer,
        //     case_timer_Duration = 3f
        // }        
    };
    // public List<PhaseManager> phaseManagerSequence;
    // public GameObject ItemManager ; 
    // public GameObject Phase_1_UI;
    public PositionManager Phase_1_PositionManager;
    // public PositionManager Phase_2_UI;
    public GameObject Phase_2_UI;
    public PositionManager Phase_2_PositionManager;
    public GameObject Phase_3_UI;
    public PositionManager Phase_3_PositionManager;

    // public System.Action<string> OnPhaseStart;

    private void Start()
    {
        StartPhase(phaseSequence[0]);
    }

    private void Update()
    {
        Debug.Log(currentIndex);
        Debug.Log(currentIndex);

        if (currentIndex >= phaseSequence.Count) 
            return;
        else 
        {
            var current = phaseSequence[currentIndex];

            if (current.phaseType == PhaseType.Timer)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    Advance();
                }
                // timer -= Time.deltaTime;
                // if (timer <= 0f)
                //     Advance();
            }
            // Level3TestUpdate();
        }
    }

    // public void L1P1_end()
    // {
    //         Phase_1_PositionManager.PauseAll();
    //         Phase_3_PositionManager.gameObject.SetActive(true);
    //         Advance();
    // }

    // public void L1P2_end()
    // {
    //         Phase_1_PositionManager.UnPauseAll();
    //         Phase_3_PositionManager.gameObject.SetActive(false);
    //         Advance();
    // }

    public void SignalPhaseDone()  // 外部通知调用这个
    {
        if (currentIndex >= phaseSequence.Count) return;

        var current = phaseSequence[currentIndex];
        if (current.phaseType == PhaseType.ExternalSignal)
        {
            Advance();
        }
    }

    private void StartPhase(PhaseData data)
    {
        Debug.Log($"进入阶段：{data.phaseName}");
        // OnPhaseStart?.Invoke(data.phaseName);

        if (data.phaseType == PhaseType.Timer)
        {
            timer = data.case_timer_Duration;
        }
        else
        {
            waitingForSignal = true;
        }
    }

    private void Advance()
    {
        // 一些具体的与逆行逻辑，切换前
        switch (currentIndex)
        {
            case 0:
                Phase_1_PositionManager.PauseAll();
                Phase_3_UI?.SetActive(true);
                Phase_3_PositionManager.gameObject.SetActive(true);
                break;
            case 1:
                Phase_1_PositionManager.UnPauseAll();
                Phase_3_UI?.SetActive(false);
                // Phase_3_PositionManager.
                Phase_3_PositionManager.HideAll();
                Phase_3_PositionManager.gameObject.SetActive(false);
                break;
        }

        // 泛运行逻辑
        currentIndex++;
        if (currentIndex < phaseSequence.Count)
            StartPhase(phaseSequence[currentIndex]);
        else
            Debug.Log("全部阶段完成");


        // 一些具体的与逆行逻辑，切换后

    }
}
