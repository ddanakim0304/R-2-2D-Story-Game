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
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Store the original Z position
            float originalZ = player.transform.position.z;
            
            // Calculate knockback direction in 2D (only X and Y)
            Vector2 knockbackDirection = ((Vector2)playerPosition - (Vector2)transform.position).normalized;
            float elapsedTime = 0f;
            float knockbackDuration = 0.1f;
            
            while (elapsedTime < knockbackDuration)
            {
                // Apply knockback only to X and Y, preserve Z
                Vector3 newPosition = player.transform.position;
                newPosition.x += knockbackDirection.x * (knockbackForce * Time.deltaTime);
                newPosition.y += knockbackDirection.y * (knockbackForce * Time.deltaTime);
                newPosition.z = originalZ;  // Maintain original Z position
                
                player.transform.position = newPosition;
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