using UnityEngine;

public class TextFollowTarget : MonoBehaviour 
{
    public Transform target; // Assign your character here
    public float yOffset = 1.5f; // Height offset above the character

    void LateUpdate()
    {
        if (target != null)
        {
            // Follow the character's position
            transform.position = target.position + Vector3.up * yOffset;
            transform.forward = Camera.main.transform.forward; // Always face the camera
        }
    }
}
