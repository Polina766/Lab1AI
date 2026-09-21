using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayer; // слой врагов — назначить в инспекторе

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ
        {
            Attack();
        }
    }

    void Attack()
    {
        // Находим всех врагов в радиусе атаки вокруг игрока
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hits)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }

    // Визуализация радиуса атаки в редакторе (просто для удобства настройки)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}