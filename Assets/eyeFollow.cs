using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Transform pupil; // Reference to the pupil (black part of the eye)
    public Transform whiteLayer; // Reference to the white part of the eye
    public float followSpeed = 5f; // Speed of the pupil following the player's direction
    public float maxPupilDistance = 0.1f; // Maximum distance the pupil can move
    
    // Define X-axis boundaries
    public float leftBoundary = -0.1f; // Maximum left position relative to white layer
    public float rightBoundary = 0.1f; // Maximum right position relative to white layer

    void Update()
    {
        if (player == null || pupil == null || whiteLayer == null)
            return;

        // Get only the X direction from eye to player
        float xDirection = player.position.x - whiteLayer.position.x;
        xDirection = Mathf.Sign(xDirection); // Just get the sign (-1 or 1)

        // Calculate the new X position with boundaries
        float targetX = whiteLayer.position.x + (xDirection * maxPupilDistance);
        
        // Clamp the position within boundaries relative to white layer
        float clampedX = Mathf.Clamp(
            targetX, 
            whiteLayer.position.x + leftBoundary, 
            whiteLayer.position.x + rightBoundary
        );

        // Create new position with only X modified
        Vector3 newPupilPosition = pupil.position;
        newPupilPosition.x = clampedX;

        // Move the pupil smoothly towards the new position
        pupil.position = Vector3.Lerp(pupil.position, newPupilPosition, followSpeed * Time.deltaTime);
    }
}