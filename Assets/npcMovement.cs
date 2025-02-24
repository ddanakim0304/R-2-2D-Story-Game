using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcMovement : MonoBehaviour
{
    public float speed = 5f;
    public float followDistance = 5f;  // Maximum distance to start following
    public float stopDistance = 1f;    // Minimum distance to maintain from player
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isFacingRight = true;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Only move if player is within follow distance but not too close
        if (distanceToPlayer < followDistance && distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            
            // Update animation
            animator.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));
            
            // Handle facing direction
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
            // Stop moving if too close or too far
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
