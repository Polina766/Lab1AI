using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
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
        currentHealth = Mathf.Max(currentHealth, 0); // не уходим в минус

        Debug.Log("Здоровье игрока: " + currentHealth);

        if (sr != null)
        {
            // Если корутина уже играет (частые удары подряд) — останавливаем и запускаем заново
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Игрок погиб!");
            // Тут можно добавить перезапуск уровня, экран смерти и т.д.
        }
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(1f);
        sr.color = originalColor;
    }
}