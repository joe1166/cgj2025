using UnityEngine;

public class ItemManagerWithSignal : PositionManager
{
    private LevelPhaseManager _myLevelPhaseManager;

    public void Init(LevelPhaseManager levelPhaseManager)
    {
        _myLevelPhaseManager = levelPhaseManager;

    }

    public void OnFinished()
    {
        _myLevelPhaseManager.SignalPhaseDone();
    }

}