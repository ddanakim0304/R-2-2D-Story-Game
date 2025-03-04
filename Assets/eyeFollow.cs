using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Transform pupil; // Reference to the pupil (black part of the eye)
    public Transform whiteLayer; // Reference to the white part of the eye
    public float followSpeed = 5f; // Speed of the pupil following the player's direction
    public float maxPupilDistance = 0.1f; // Maximum distance the pupil can move

    void Update()
    {
        // Get the direction from the eye to the player
        Vector3 direction = (player.position - whiteLayer.position).normalized;

        // Calculate the new position for the pupil
        Vector3 newPupilPosition = whiteLayer.position + direction * maxPupilDistance;

        // Move the pupil smoothly towards the new position
        pupil.position = Vector3.Lerp(pupil.position, newPupilPosition, followSpeed * Time.deltaTime);
    }
}
