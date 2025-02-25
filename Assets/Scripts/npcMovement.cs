using UnityEngine;

public class npcMovement : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public float followDistance = 2f;  // Maximum distance to start following
    public float stopDistance = 1f;    // Minimum distance to maintain from player
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isFacingRight = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer < followDistance && distanceToPlayer > stopDistance)
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
    }
    
    private void Turn()
    {
        isFacingRight = !isFacingRight;
        transform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }
}