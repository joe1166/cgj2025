using UnityEngine;

public class L3P2PositionManager : ItemManagerWithSignal 
{
    private float t = 3f;

    public void Update()
    {
        t -= Time.deltaTime;
        if (t < 0)
        {
            CheckLevelCompletion();       
        }
    }
    public virtual void CheckLevelCompletion()
    {


        // 检查关卡中所有可能的正确位置是否都被占用
        if (base.IsLevelComplete())
        {
            Debug.Log("UI内关卡完成！");
            this.OnFinished();

            // // 通知关卡控制器
            // LevelController levelController = FindObjectOfType<LevelController>();
            // if (levelController != null)
            // {
            //     levelController.CompleteLevel();
            // }
        }
    }
}