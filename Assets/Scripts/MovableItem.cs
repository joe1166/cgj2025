using UnityEngine;

public class MovableItem : MonoBehaviour
{
    // public float speedScale = 1;
    private float moveSpeed = 2f;
    public float minChangeDirTime = 1f;
    public float maxChangeDirTime = 3f;
    private float settleTimer = 0;


    private Vector2 moveDirection;
    private float changeDirTimer;

    void Awake()
    {
        moveSpeed = GetComponent<DraggableItem>().ItemData.moveSpeed;

    }

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        bool IsDragging = GetComponent<DraggableItem>().IsDragging;
        bool isMoving = IsDragging && (settleTimer == 0);
        if (true)
        {
            UnityEngine.Vector2 newPos = moveDirection * moveSpeed * Time.deltaTime;
            transform.Translate(newPos);

            CheckScreenEdgeBounce();

            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0)
            {
                // PickNewDirection();
            }
        }

    }

    void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    void CheckScreenEdgeBounce()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        bool bounced = false;

        // 左右碰撞
        if (viewPos.x <= 0f || viewPos.x >= 1f)
        {
            moveDirection.x = -moveDirection.x;
            bounced = true;
        }

        // 上下碰撞
        if (viewPos.y <= 0f || viewPos.y >= 1f)
        {
            moveDirection.y = -moveDirection.y;
            bounced = true;
        }

        if (bounced)
        {
            moveDirection = moveDirection.normalized;
            changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime);
        }
    }

    public void Settle()
    {
        settleTimer = GetComponent<DraggableItem>().ItemData.settleTime;
    }
}
