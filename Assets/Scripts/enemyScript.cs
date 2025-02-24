using UnityEngine;
using System.Collections;

public class enemyScript : MonoBehaviour
{
    private Health enemyHealth;
    private Animator animator;

    [SerializeField] private float deathDelay = 1.0f;

    private Vector3 playerPosition;
    public float knockbackForce = 2f;
    private bool isKnockback = false;

    void Awake()
    {
        enemyHealth = GetComponent<Health>();
        animator = GetComponent<Animator>();
    }
    // Use collision for player interactions
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("Player got attacked!");
            collision.gameObject.GetComponent<PlayerController>().TakeDamage();
        }
    }

    public void ReceiveDamage(int damage)
    {
        enemyHealth.DecreaseHP(damage);
        Debug.Log("Enemy got attacked!");

        // Get player position from the active player in scene
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        StartCoroutine(KnockbackEffect());

        if (enemyHealth.currentHP <= 0)
        {
            animator.SetBool("Die", true);
            StartCoroutine(DelayedDestroy());
        }
    }

    private IEnumerator KnockbackEffect()
    {
        isKnockback = true;
        
        // Calculate knockback direction TOWARDS enemy (since we're moving the player)
        Vector3 knockbackDirection = (playerPosition - transform.position).normalized;
        float elapsedTime = 0f;
        float knockbackDuration = 0.1f;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            while (elapsedTime < knockbackDuration)
            {
                player.transform.position += knockbackDirection * (knockbackForce * Time.deltaTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        isKnockback = false;
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}