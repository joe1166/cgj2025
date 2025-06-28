using UnityEngine;
using System;

public class MovableItem : MonoBehaviour
{

    private float moveSpeed = 2f;
    private Vector2 moveDirection;

    private float settleTimer = 0;
    private float changeDirTimer;
    private float minChangeDirTime = 1f;
    private float maxChangeDirTime = 3f;

    private float leftExtent;
    private float rightExtent;
    private float topExtent;
    private float bottomExtent;

    public void Init()
    {
        moveSpeed = GetComponent<DraggableItem>().ItemData.moveSpeed;

        // 计算碰撞检测变量
        var bounds = GetComponent<Collider2D>().bounds;
        Vector2 center = bounds.center;
        leftExtent = center.x - bounds.min.x;
        rightExtent = bounds.max.x - center.x;
        topExtent = center.y - bounds.min.y;
        bottomExtent = bounds.max.y - center.y;

        PickNewDirection();
    }

    void Update()
    {
        // bool IsDragging = GetComponent<DraggableItem>().IsDragging;
        bool canMove = GetComponent<DraggableItem>().CanMove();
        if (canMove)
        {
            // 移动
            UnityEngine.Vector2 newPos = moveDirection * moveSpeed * Time.deltaTime;
            transform.Translate(newPos);

            // 碰撞检测
            CheckScreenEdgeBounce();

            // 改变移动方向
            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0)
            {
                PickNewDirection();
            }
        }
        if (IsSettling())
        {
            settleTimer = Math.Max(settleTimer - Time.deltaTime, 0);

            // 当settle time归零时，释放位置并重置状态
            if (settleTimer <= 0)
            {
                ReleasePosition();
            }
        }
    }

    /// <summary>
    /// 释放位置并重置物品状态
    /// </summary>
    private void ReleasePosition()
    {
        DraggableItem draggableItem = GetComponent<DraggableItem>();
        PositionManager positionManager = FindObjectOfType<PositionManager>();

        if (positionManager != null && draggableItem != null)
        {
            // 释放位置
            positionManager.ReleasePosition(transform.position);

            // 重置IsSnapped状态
            draggableItem.ResetSnapState();

            Debug.Log($"物品 {draggableItem.ItemData?.itemName} 已释放位置，重新开始移动");
        }
    }

    bool IsSettling()
    {
        return (settleTimer > 0);
    }

    void PickNewDirection()
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        changeDirTimer = UnityEngine.Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    void CheckScreenEdgeBounce()
    {
        // 先获取边界位置（世界坐标）
        Vector3 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z));
        Vector3 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z));

        Vector3 pos = transform.position;
        bool bounced = false;

        // 水平方向碰撞
        if (pos.x - leftExtent <= screenMin.x || pos.x + rightExtent >= screenMax.x)
        {
            moveDirection.x = -moveDirection.x;
            bounced = true;
        }

        // 垂直方向碰撞
        if (pos.y - bottomExtent <= screenMin.y || pos.y + topExtent >= screenMax.y)
        {
            moveDirection.y = -moveDirection.y;
            bounced = true;
        }

        if (bounced)
        {
            moveDirection = moveDirection.normalized;
            changeDirTimer = UnityEngine.Random.Range(minChangeDirTime, maxChangeDirTime);
        }
    }

    public void Settle()
    {
        settleTimer = GetComponent<DraggableItem>().ItemData.settleTime;
    }

}
