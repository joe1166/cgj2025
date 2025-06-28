using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 条件拖拽物品 - 需要满足前置条件才能拖拽
/// </summary>
public class ShakableDraggableItem : DraggableItem
{
    // private bool isShaking = false;
    private bool isPassedOut = false;
    private float passedOutTimer = 0f;

    private Queue<float> recentShakeTimes = new Queue<float>();
    private Vector2 lastMousePosition;
    private int lastShakeDirection = 0; // -1 for left, 1 for right

    // 最后几次的转向
    // 转向之后的

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 调用父类的拖拽结束方法
        base.OnEndDrag(eventData);

        // // 如果没放对地方，就直接清醒过来？
        // EndPassedOut();

    }

    public override bool CanMove()
    {
        bool baseCanMove = base.CanMove();
        bool canMove = (!isPassedOut) && baseCanMove;
        return canMove;
    }


    public override void Init()
    {
        base.Init();

        lastMousePosition = Input.mousePosition;
    }

    public override void UpdateAfterHook()
    {
        if (isPassedOut)
        {
            passedOutTimer -= Time.deltaTime;
            if (passedOutTimer <= 0f)
            {
                EndPassedOut();
            }
            return;
        }

        if (IsDragging)
        {
            DetectShake();
        }
        else
        {
            // 非拖拽时清除计数
            recentShakeTimes.Clear();
        }
    }

    private void DetectShake()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        float deltaX = currentMousePosition.x - lastMousePosition.x;

        int currentDirection = deltaX > 1f ? 1 : (deltaX < -1f ? -1 : 0);

        if (currentDirection != 0 && currentDirection != lastShakeDirection && lastShakeDirection != 0)
        {
            // 检测到一次翻转
            recentShakeTimes.Enqueue(Time.time);
            Debug.Log("检测到摇晃 !");

            // 只保留最近 1 秒内的摇晃记录
            while (recentShakeTimes.Count > 0 && Time.time - recentShakeTimes.Peek() > 1f)
            {
                recentShakeTimes.Dequeue();
            }

            if (recentShakeTimes.Count >= 3)
            {
                // 维持摇晃状态 1 秒
                Debug.Log("持续！！!");
                float duration = recentShakeTimes.Last() - recentShakeTimes.Peek();
                Debug.Log(duration);

                if (duration <= 1f)
                {
                    StartPassedOut();
                }
            }
        }

        lastMousePosition = currentMousePosition;
        if (currentDirection != 0)
        {
            lastShakeDirection = currentDirection;
        }
    }

    private void StartPassedOut()
    {
        isPassedOut = true;
        passedOutTimer = 5f;
        recentShakeTimes.Clear();
        Debug.Log("晕倒！");
    }

    public void EndPassedOut()
    {
        if (isPassedOut)
        {
            isPassedOut = false;
            passedOutTimer = 0f;
            Debug.Log("恢复正常");
        }
    }

    public override bool settleConditionHook()
    {
        Debug.Log("晕倒情况：" + isPassedOut);
        return isPassedOut;
    }

}