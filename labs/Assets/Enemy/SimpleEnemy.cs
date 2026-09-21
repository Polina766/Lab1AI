using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    enum State { Idle, Patrol, Chase, Attack, Search }
    State state = State.Idle;

    public Transform player;
    public Transform[] patrolPoints;
    int patrolIndex = 0;

    public float speed = 3f;
    public float viewRadius = 6f;
    public float viewAngle = 180f;
    public float attackRange = 1.2f;
    public int attackDamage = 10;
    public float searchTime = 3f;
    public float idleTime = 2f; // сколько секунд враг стоит при запуске, прежде чем начать патрулировать
    public LayerMask obstacleLayer; // слой стен — назначить в инспекторе

    Rigidbody2D rb;
    float attackCooldown = 0f;
    float searchClock = 0f;
    float idleClock = 0f;
    Vector2 lastSeenPos;
    Vector2 desiredMoveTarget; // куда двигаться — считаем в Update, а реально двигаем в FixedUpdate
    bool shouldMove = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        bool seePlayer = CanSeePlayer();
        shouldMove = false; // по умолчанию каждый кадр — стоим, если ни одна ветка не скажет иначе

        switch (state)
        {
            case State.Idle:
                shouldMove = false;
                idleClock += Time.deltaTime;
                if (seePlayer)
                {
                    state = State.Chase; // если игрока увидели даже во время Idle — сразу гонимся
                    break;
                }
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
                desiredMoveTarget = player.position;
                shouldMove = true;

                // attackRange + небольшой запас (0.2f) — чтобы враг не дёргался на границе радиуса атаки
                if (Vector2.Distance(transform.position, player.position) <= attackRange)
                    state = State.Attack;
                break;

            case State.Attack:
                shouldMove = false; // стоим на месте во время атаки
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
        }
    }

    void FixedUpdate()
    {
        // Вся физика двигается только здесь — убирает дрожание Rigidbody2D
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
        if (patrolPoints.Length == 0) return; // shouldMove остаётся false — стоим

        Transform target = patrolPoints[patrolIndex];
        desiredMoveTarget = target.position;
        shouldMove = true;

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    bool CanSeePlayer()
    {
        Vector2 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > viewRadius) return false;

        float angle = Vector2.Angle(transform.up, dir);
        if (angle > viewAngle / 2f) return false;

        // Raycast — проверяем, нет ли стены между врагом и игроком
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleLayer);
        if (hit.collider != null) return false; // луч упёрся в препятствие раньше игрока

        return true;
    }
}