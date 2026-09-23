using UnityEngine;

public class TeleportEnemy : MonoBehaviour
{
    enum State { Idle, Patrol, Chase, Attack, Search, Teleport }
    State state = State.Idle;

    [Header("Ссылки")]
    public Transform player;
    public Transform[] patrolPoints;
    int patrolIndex = 0;

    [Header("Движение")]
    public float speed = 3f;

    [Header("Атака")]
    public float attackRange = 1.2f;
    public int attackDamage = 10;

    [Header("Тайминги состояний")]
    public float idleTime = 2f;
    public float searchTime = 3f;

    [Header("Препятствия")]
    public LayerMask obstacleLayer;

    [Header("Сканирование 360°")]
    public float scanRadius = 3f;      // по методичке — 1 метр, увеличено для удобного тестирования
    public float scanInterval = 1f;    // периодичность сканирования (задаётся самостоятельно)
    float scanTimer = 0f;
    bool lastScanResult = false;

    [Header("Телепортация")]
    public float teleportCooldown = 5f;
    public float teleportMaxDistance = 4f;
    float teleportTimer;

    Rigidbody2D rb;
    float attackCooldown = 0f;
    float searchClock = 0f;
    float idleClock = 0f;
    Vector2 lastSeenPos;
    Vector2 desiredMoveTarget;
    bool shouldMove = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        teleportTimer = teleportCooldown; // чтобы не телепортировался мгновенно при первой встрече
    }

    void Update()
    {
        bool seePlayer = ScanForPlayer();
        shouldMove = false;
        teleportTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                idleClock += Time.deltaTime;
                if (seePlayer) { state = State.Chase; break; }
                if (idleClock >= idleTime) state = State.Patrol;
                break;

            case State.Patrol:
                Patrol();
                if (seePlayer) state = State.Chase;
                break;

            case State.Chase:
                if (!seePlayer)
                {
                    lastSeenPos = player.position;
                    searchClock = 0f;
                    state = State.Search;
                    break;
                }

                if (teleportTimer <= 0f)
                {
                    state = State.Teleport;
                    break;
                }

                desiredMoveTarget = player.position;
                shouldMove = true;

                if (Vector2.Distance(transform.position, player.position) <= attackRange)
                    state = State.Attack;
                break;

            case State.Attack:
                shouldMove = false;
                if (Vector2.Distance(transform.position, player.position) > attackRange + 0.2f)
                {
                    state = State.Chase;
                    break;
                }
                attackCooldown -= Time.deltaTime;
                if (attackCooldown <= 0f)
                {
                    player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
                    attackCooldown = 1f;
                }
                break;

            case State.Search:
                if (seePlayer) { state = State.Chase; break; }
                if (Vector2.Distance(transform.position, lastSeenPos) > 0.2f)
                {
                    desiredMoveTarget = lastSeenPos;
                    shouldMove = true;
                }
                searchClock += Time.deltaTime;
                if (searchClock >= searchTime) state = State.Patrol;
                break;

            case State.Teleport:
                shouldMove = false;
                DoTeleport();
                state = State.Chase;
                break;
        }
    }

    void FixedUpdate()
    {
        if (shouldMove)
        {
            Vector2 dir = (desiredMoveTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[patrolIndex];
        desiredMoveTarget = target.position;
        shouldMove = true;

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    bool ScanForPlayer()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;

            float dist = Vector2.Distance(transform.position, player.position);
            lastScanResult = dist <= scanRadius;

            if (lastScanResult)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);
                if (hit.collider != null) lastScanResult = false;
            }
        }
        return lastScanResult;
    }

    void DoTeleport()
    {
        Vector2 randomOffset = Random.insideUnitCircle * teleportMaxDistance;
        Vector2 targetPos = (Vector2)transform.position + randomOffset;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.5f, obstacleLayer);
        if (hit == null)
        {
            transform.position = targetPos;
        }

        teleportTimer = teleportCooldown;
    }
}