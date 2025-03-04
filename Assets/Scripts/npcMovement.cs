using UnityEngine;

public class npcMovement : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public float followDistance = 2f;  // Maximum distance to start following
    public float stopDistance = 1f;    // Minimum distance to maintain from player
    public float jumpForce = 7f;       // Jump force value (similar to player's)
    public float jumpDetectionThreshold = 2f; // Threshold to detect player jump
    public float teleportDistance = 10f; // Distance to trigger teleportation
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isFacingRight = true;
    private bool canJump = true;       // Jump control variable
    private float lastPlayerY;         // Track player's last Y position
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (player != null)
            lastPlayerY = player.position.y;
    }

    void Update()
    {
        if (player == null) return;
        
        // Detect player jumps by checking upward velocity
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.velocity.y > jumpDetectionThreshold && canJump)
        {
            // Player is jumping, so NPC should jump too
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canJump = false;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Teleport NPC if it's too far from the player
        if (distanceToPlayer > teleportDistance)
        {
            // Calculate position at stopDistance from player
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 newPosition = (Vector2)player.position - (direction * stopDistance);
            
            // Directly set position (ignoring physics)
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            
            // Update facing direction
            if ((player.position.x > transform.position.x && !isFacingRight) || 
                (player.position.x < transform.position.x && isFacingRight))
            {
                Turn();
            }
            
            animator.SetFloat("velocityX", 0);
        }
        else if (distanceToPlayer < followDistance && distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            
            animator.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));
            
            if (direction.x > 0 && !isFacingRight)
            {
                Turn();
            }
            else if (direction.x < 0 && isFacingRight)
            {
                Turn();
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetFloat("velocityX", 0);
        }
        
        lastPlayerY = player.position.y;
    }
    
    private void Turn()
    {
        isFacingRight = !isFacingRight;
        transform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }
    
    // Reset jump ability when NPC touches the ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }
}