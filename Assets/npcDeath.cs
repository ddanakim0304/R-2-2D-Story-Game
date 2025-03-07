using System.Collections;
using UnityEngine;
using Cinemachine;

public class npcDeath : DialogueTrigger
{
    [Header("NPC Death Settings")]
    public float targetXPosition = 186.3f;
    public float walkSpeed = 4f;
    
    [Header("Camera Settings")]
    public float zoomInDistance = 10f;
    public float cameraTransitionTime = 2.5f;
    public float NPCcameraTransitionTime = 0.2f;
    
    [Header("Monster Settings")]
    public GameObject monster;
    
    private Rigidbody2D npcRb;
    private bool isFacingRight = true;
    private float originalOrthoSize;
    private Vector3 originalTrackedOffset;
    private Transform originalFollowTarget;
    
    protected override void Start()
    {
        base.Start();
        if (npcTransform != null)
        {
            npcRb = npcTransform.GetComponent<Rigidbody2D>();
        }
    }
    
    protected override void StartDialogue()
    {
        if (isDialogueActive) return;
    
        isDialogueActive = true;
        player.DisableControl();
        
        // Start camera transition
        if (cinemachineCamera != null)
        {
            StartCoroutine(AdjustCameraForNPC());
        }
        
        // Choose the starting point if specified
        if (!string.IsNullOrEmpty(storySection))
        {
            story.ChoosePathString(storySection);
            currentKnot = storySection;
        }
        
        ContinueDialogue();
    }
    
    private IEnumerator AdjustCameraForNPC()
    {
        if (cinemachineCamera == null || npcTransform == null)
        {
            Debug.LogError("Camera or NPC transform is missing!");
            yield break;
        }
        
        // Store original camera settings
        originalFollowTarget = cinemachineCamera.Follow;
        CinemachineFramingTransposer framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        
        if (framingTransposer != null)
        {
            // Store original values
            float originalCameraDistance = framingTransposer.m_CameraDistance;
            originalTrackedOffset = framingTransposer.m_TrackedObjectOffset;

            
            // Add a 2-second delay before changing the camera target
            yield return new WaitForSeconds(2f);
            
            // Change camera to follow NPC
            cinemachineCamera.Follow = npcTransform;
            
            // Zoom in gradually
            float elapsedTime = 0f;
            float initialCameraDistance = framingTransposer.m_CameraDistance;
            Vector3 initialTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            while (elapsedTime < NPCcameraTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / NPCcameraTransitionTime;
                
                // Interpolate both the camera distance and tracked offset
                framingTransposer.m_CameraDistance = Mathf.Lerp(initialCameraDistance, zoomInDistance, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialTrackedOffset, Vector3.zero, t);
                
                yield return null;
            }
            
            // Ensure we reach the exact target values
            framingTransposer.m_CameraDistance = zoomInDistance;
            framingTransposer.m_TrackedObjectOffset = Vector3.zero;
            
        }
        else
        {
            Debug.LogError("CinemachineFramingTransposer component not found on camera!");
        }
    }
    
    protected override void EndDialogue()
    {
        dialogueText_NPC.text = "";
        dialogueText_Player.text = "";
        isDialogueActive = false;
        
        // Start walking behavior
        StartCoroutine(WalkToTarget());
    }
    private IEnumerator WalkToTarget()
    {
        if (npcRb == null)
        {
            Debug.LogError("No Rigidbody2D found on NPC!");
            yield break;
        }
        
        // Set direction to face target
        if ((targetXPosition > npcTransform.position.x && !isFacingRight) ||
            (targetXPosition < npcTransform.position.x && isFacingRight))
        {
            Turn();
        }
        
        while (Mathf.Abs(npcTransform.position.x - targetXPosition) > 0.1f)
        {
            float direction = targetXPosition > npcTransform.position.x ? 1f : -1f;
            npcRb.velocity = new Vector2(direction * walkSpeed, npcRb.velocity.y);
            
            yield return null;
        }
        
        npcRb.velocity = Vector2.zero;
        
        // Handle monster death animation
        if (monster != null)
        {
            // Remove all children of the monster
            foreach (Transform child in monster.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Get the Animator component and trigger the death animation
            Animator monsterAnimator = monster.GetComponent<Animator>();
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger("death");
                
                // Optional: wait for animation to complete (adjust time as needed)
                yield return new WaitForSeconds(1.5f);
            }
        }
        
        Destroy(gameObject);
    }
    private void Turn()
    {
        isFacingRight = !isFacingRight;
        npcTransform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }
}