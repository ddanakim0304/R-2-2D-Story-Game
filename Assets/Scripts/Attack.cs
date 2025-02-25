using UnityEngine;
using System;

public class Attack : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    // Use System.Action instead of a custom delegate.
    public event Action OnEnemyHit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to an enemy
        if (other.CompareTag("Enemy"))
        {
            enemyScript enemy = other.GetComponent<enemyScript>();
            if (enemy != null)
            {
                enemy.ReceiveDamage(damage);
                OnEnemyHit?.Invoke();
            }
        }
    }
}