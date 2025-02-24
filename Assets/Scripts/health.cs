using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void DecreaseHP(int amount)
    {
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
    }
}