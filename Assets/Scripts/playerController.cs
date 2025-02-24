using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 7f;
    public float jumpForce = 7f;
    public int maxJumps = 2; // Allow double jump
    // public AudioClip jumpSound;
    // public AudioClip hurtSound;

    public float knockbackForce = 5f; 
    private bool isKnockback = false;
    
    public float flashDuration = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Added Animator
    // private AudioSource audioSource;
    private int remainingJumps;
    public bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        // audioSource = GetComponent<AudioSource>();
        remainingJumps = maxJumps;
    }

    void Update()
    {
        if (!isKnockback)  // Only allow movement if not in knockback
        {
            float moveInput = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            TurnCheck(moveInput);
        }
        
        animator.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));
        
        if (Input.GetButtonDown("Jump") && remainingJumps > 0 && !isKnockback)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            remainingJumps--;
        }
    }

    public void TakeDamage()
    {
        GetComponent<Health>().DecreaseHP(1);
        StartCoroutine(DamageFeedback());
    }
    private IEnumerator DamageFeedback()
    {
        isKnockback = true;
        
        Vector3 knockbackDirection = isFacingRight ? Vector3.left : Vector3.right;
        float elapsedTime = 0f;
        float knockbackDuration = 0.3f;
        
        while (elapsedTime < knockbackDuration)
        {
            transform.position += knockbackDirection * (knockbackForce * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    
        // Add pause after knockback
        yield return new WaitForSeconds(0.1f);
        
        isKnockback = false;
    }
    private void TurnCheck(float moveInput)
    {
        if (moveInput > 0 && !isFacingRight)
        {
            Turn();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        isFacingRight = !isFacingRight;
        transform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }

}
