using UnityEngine;
using System;

public class MovableItem : MonoBehaviour
{
    // public float speedScale = 1;
    private float moveSpeed = 2f;

    private float minChangeDirTime = 1f;
    private float maxChangeDirTime = 3f;
    private Vector2 moveDirection;


    private float settleTimer = 0;
    private float changeDirTimer;

    private float leftExtent;
    private float rightExtent;
    private float topExtent;
    private float bottomExtent;

    void Awake()
    {
        moveSpeed = GetComponent<DraggableItem>().ItemData.moveSpeed;

        // 计算中心点距离左右碰撞点
        var bounds = GetComponent<Collider2D>().bounds;
        Vector2 center = bounds.center;
        leftExtent = center.x - bounds.min.x;
        rightExtent = bounds.max.x - center.x;
        topExtent = center.y - bounds.min.y;
        bottomExtent = bounds.max.y - center.y;

    }

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        bool IsDragging = GetComponent<DraggableItem>().IsDragging;
        bool IsMoving = !(IsDragging || IsSettling()); 
        if (IsMoving)
        {
            UnityEngine.Vector2 newPos = moveDirection * moveSpeed * Time.deltaTime;
            transform.Translate(newPos);

            CheckScreenEdgeBounce();

            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0)
            {
                PickNewDirection();
            }
        }
        if (IsSettling()) 
        {
            settleTimer = Math.Min(settleTimer - Time.deltaTime, 0);
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
