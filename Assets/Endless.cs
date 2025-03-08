using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endless : MonoBehaviour
{
    public Transform playerTransform;  // Reference to the player
    public float groundLength = 20f;   // Length of one ground tile segment
    public float distanceThreshold = 10f; // Distance before repositioning tiles
    
    private Transform[] groundTiles;   // Array of ground tile transforms
    private Vector2 groundMovementDirection = Vector2.right; // Direction to move tiles
    private float lastRepositionX;     // Track when we last repositioned
    
    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        // Get all the child ground tiles
        groundTiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            groundTiles[i] = transform.GetChild(i);
        }
        
        // Sort tiles by x position if needed
        SortTilesByPosition();
        
        // Initial position tracking
        lastRepositionX = playerTransform.position.x;
    }

    void Update()
    {
        // Check if player has moved far enough to trigger repositioning
        if (Mathf.Abs(playerTransform.position.x - lastRepositionX) > distanceThreshold)
        {
            // Determine direction of player movement
            float moveDirection = playerTransform.position.x > lastRepositionX ? 1 : -1;
            RepositionGroundTiles(moveDirection);
            lastRepositionX = playerTransform.position.x;
        }
    }
    
    void SortTilesByPosition()
    {
        // Simple insertion sort by X position
        for (int i = 1; i < groundTiles.Length; i++)
        {
            Transform key = groundTiles[i];
            int j = i - 1;
            
            while (j >= 0 && groundTiles[j].position.x > key.position.x)
            {
                groundTiles[j + 1] = groundTiles[j];
                j--;
            }
            
            groundTiles[j + 1] = key;
        }
    }
    
    void RepositionGroundTiles(float direction)
    {
        if (direction > 0) // Moving right
        {
            // Get leftmost tile
            Transform leftmost = groundTiles[0];
            
            // Find the rightmost position
            Transform rightmost = groundTiles[groundTiles.Length - 1];
            Vector3 newPosition = rightmost.position + new Vector3(groundLength, 0, 0);
            
            // Move the leftmost to become the new rightmost
            leftmost.position = newPosition;
            
            // Rearrange the array (shift all elements left and put first element at the end)
            for (int i = 0; i < groundTiles.Length - 1; i++)
            {
                groundTiles[i] = groundTiles[i + 1];
            }
            groundTiles[groundTiles.Length - 1] = leftmost;
        }
        else // Moving left
        {
            // Get rightmost tile
            Transform rightmost = groundTiles[groundTiles.Length - 1];
            
            // Find the leftmost position
            Transform leftmost = groundTiles[0];
            Vector3 newPosition = leftmost.position - new Vector3(groundLength, 0, 0);
            
            // Move the rightmost to become the new leftmost
            rightmost.position = newPosition;
            
            // Rearrange the array (shift all elements right and put last element at the beginning)
            for (int i = groundTiles.Length - 1; i > 0; i--)
            {
                groundTiles[i] = groundTiles[i - 1];
            }
            groundTiles[0] = rightmost;
        }
    }
}