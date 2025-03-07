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
            originalOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
            originalTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            // Set higher damping for smooth transition
            float originalXDamping = framingTransposer.m_XDamping;
            float originalYDamping = framingTransposer.m_YDamping;
            float originalZDamping = framingTransposer.m_ZDamping;
            
            framingTransposer.m_XDamping = 5.0f;
            framingTransposer.m_YDamping = 5.0f;
            framingTransposer.m_ZDamping = 5.0f;
            
            // Change camera to follow NPC
            cinemachineCamera.Follow = npcTransform;
            framingTransposer.m_TrackedObjectOffset = Vector3.zero;
            
            // Zoom in gradually
            float startTime = Time.time;
            float initialOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
            
            while (Time.time < startTime + cameraTransitionTime)
            {
                float t = (Time.time - startTime) / cameraTransitionTime;
                cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(initialOrthoSize, zoomInDistance, t);
                yield return null;
            }
            
            // Ensure we reach the exact target values
            cinemachineCamera.m_Lens.OrthographicSize = zoomInDistance;
            
            // Restore original damping values
            framingTransposer.m_XDamping = originalXDamping;
            framingTransposer.m_YDamping = originalYDamping; 
            framingTransposer.m_ZDamping = originalZDamping;
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
        
        // Reset camera to player
        StartCoroutine(ResetCamera());
        
        // Start walking behavior
        StartCoroutine(WalkToTarget());
    }
    
    private IEnumerator ResetCamera()
    {
        yield return new WaitForSeconds(1f); // Small delay before switching back
        
        if (cinemachineCamera != null)
        {
            CinemachineFramingTransposer framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            if (framingTransposer != null)
            {
                // Set higher damping for smooth transition back
                float originalXDamping = framingTransposer.m_XDamping;
                float originalYDamping = framingTransposer.m_YDamping;
                float originalZDamping = framingTransposer.m_ZDamping;
                
                framingTransposer.m_XDamping = 5.0f;
                framingTransposer.m_YDamping = 5.0f;
                framingTransposer.m_ZDamping = 5.0f;
                
                // Reset follow target
                cinemachineCamera.Follow = originalFollowTarget;
                
                // Zoom out gradually
                float startTime = Time.time;
                float initialOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
                Vector3 initialOffset = framingTransposer.m_TrackedObjectOffset;
                
                while (Time.time < startTime + cameraTransitionTime)
                {
                    float t = (Time.time - startTime) / cameraTransitionTime;
                    cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(initialOrthoSize, originalOrthoSize, t);
                    framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialOffset, originalTrackedOffset, t);
                    yield return null;
                }
                
                // Ensure we reach the exact target values
                cinemachineCamera.m_Lens.OrthographicSize = originalOrthoSize;
                framingTransposer.m_TrackedObjectOffset = originalTrackedOffset;
                
                // Restore original damping values
                framingTransposer.m_XDamping = originalXDamping;
                framingTransposer.m_YDamping = originalYDamping;
                framingTransposer.m_ZDamping = originalZDamping;
            }
        }
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

        if (player != null)
        {
            player.EnableControl();
        }
        
        Destroy(gameObject);
    }
    
    private void Turn()
    {
        isFacingRight = !isFacingRight;
        npcTransform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }
}