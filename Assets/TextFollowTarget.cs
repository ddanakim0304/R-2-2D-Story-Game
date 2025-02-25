using UnityEngine;

public class TextFollowTarget : MonoBehaviour 
{
    public Transform target;
    public float yOffset = 0f;

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
