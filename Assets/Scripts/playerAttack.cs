using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackPrefab;
    [SerializeField] private Transform attackPointFront;
    [SerializeField] private Transform attackPointUp;
    [SerializeField] private Transform attackPointDown;
    
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>(); // Reference to player direction
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) // Attack when X is pressed
        {
            Attack();
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