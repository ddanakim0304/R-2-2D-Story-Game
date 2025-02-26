using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackPrefab;
    [SerializeField] private Transform attackPointFront;
    [SerializeField] private Transform attackPointUp;
    [SerializeField] private Transform attackPointDown;
    
    public float attackCooldown = 0.1f; // Cooldown time in seconds
    private float lastAttackTime = -1f; // Initialize to -1 to allow immediate first attack
    
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>(); // Reference to player direction
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && Time.time >= lastAttackTime + attackCooldown) // Check cooldown
        {
            Attack();
            lastAttackTime = Time.time; // Update the last attack time
        }
    }

    void Attack()
    {
        Transform attackPoint = attackPointFront; // Default attack direction
        Quaternion attackRotation = Quaternion.identity; // Default rotation (right)

        if (Input.GetAxisRaw("Vertical") > 0) // If pressing UP
        {
            attackPoint = attackPointUp;
            attackRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetAxisRaw("Vertical") < 0) // If pressing DOWN
        {
            attackPoint = attackPointDown;
            attackRotation = Quaternion.Euler(0, 0, -90);
        }

        // Instantiate the attack effect as a child of the player.
        GameObject attack = Instantiate(attackPrefab, transform);

        // Set its local position to the offset defined by the attack point,
        // so it appears in front of the player.
        attack.transform.localPosition = attackPoint.localPosition;
        attack.transform.localRotation = attackRotation;
    }
}