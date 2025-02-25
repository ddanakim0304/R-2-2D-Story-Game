using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 7f;
    public float jumpForce = 7f;
    private bool canJump = true;
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

    private bool canControl = true;  // Add this at the top with other private variables

    // Add these two public methods
    public void EnableControl()
    {
        canControl = true;
        isKnockback = false;  // Reset knockback state when enabling control
    }

    public void DisableControl()
    {
        canControl = false;
        rb.velocity = Vector2.zero;  // Stop all movement
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        // audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isKnockback && canControl)  // Only allow movement if not in knockback and has control
        {
            float moveInput = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            TurnCheck(moveInput);
            
            // Modified jump check
            if (Input.GetButtonDown("Jump") && canJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                canJump = false;
            }
        }
        
        animator.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));
    }

    // Add this method to reset jump when touching ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
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
