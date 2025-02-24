using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private Collider2D attackCollider;

    void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.enabled = false; // Disable at start
    }

    public void EnableHitbox()
    {
        attackCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        attackCollider.enabled = false;
    }

}
