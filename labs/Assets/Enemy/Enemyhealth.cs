using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;

    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashRoutine;

    void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(gameObject.name + " здоровье: " + currentHealth);

        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " уничтожен!");
            Destroy(gameObject);
        }
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(1f);
        sr.color = originalColor;
    }
}
