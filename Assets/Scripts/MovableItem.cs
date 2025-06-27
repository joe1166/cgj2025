using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    // public float speedScale = 1;
    public float moveSpeed = 2f;
    public float minChangeDirTime = 1f;
    public float maxChangeDirTime = 3f;
    public float settleTimer = 0;

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
            UnityEngine.Vector3 newPos = moveDirection * moveSpeed * Time.deltaTime;
            transform.Translate(newPos);
            Debug.Log(newPos);

            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0)
            {
                PickNewDirection();
            }
        }

    }

    void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        Debug.Log("New Direction");
        Debug.Log(moveDirection);
        changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 获取法线方向
        Vector2 normal = collision.contacts[0].normal;

        // 法线方向 ±60° 随机一个方向
        float baseAngle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 180f; // 反向朝法线走
        float offset = Random.Range(-60f, 60f);
        float newAngle = baseAngle + offset;

        moveDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

        changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime); // 碰撞也重置时间
    }

    public void Settle()
    {
        settleTimer = GetComponent<DraggableItem>().ItemData.settleTime;
    }
}
