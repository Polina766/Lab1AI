using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
  
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D или стрелки влево-вправо
        float vertical = Input.GetAxisRaw("Vertical");     // W/S или стрелки вверх-вниз

        moveDirection = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }
}