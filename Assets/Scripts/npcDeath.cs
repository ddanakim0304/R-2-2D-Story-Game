using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement; // Added for scene management

public class npcDeath : DialogueTrigger
{
    [Header("NPC Death Settings")]
    public float targetXPosition = 186.3f;
    public float walkSpeed = 4f;
    
    [Header("Camera Settings")]
    public float zoomedInSize = 2.35f; // Using orthographicSize instead of distance
    public float zoomedOutSize = 8f;   // Final zoom out size
    public float cameraTransitionTime = 2.5f;
    public float NPCcameraTransitionTime = 0.2f;
    
    [Header("Monster Settings")]
    public GameObject monster;
    
    [Header("Scene Transition")]
    public string nextSceneName; // Assign the next scene name in Inspector
    public float delayBeforeSceneChange = 3f;
    
    private Rigidbody2D npcRb;
    private bool isFacingRight = true;
    private float originalCameraSize;
    private Vector3 originalTrackedOffset;
    private Transform originalFollowTarget;
    private Camera virtualCamera;
    
    protected override void Start()
    {
        base.Start();
        if (npcTransform != null)
        {
            npcRb = npcTransform.GetComponent<Rigidbody2D>();
        }
        
        // Get the virtual camera component like in battleSceneManager
        if (cinemachineCamera != null)
        {
            virtualCamera = cinemachineCamera.gameObject.GetComponent<Camera>();
            if (virtualCamera == null)
            {
                virtualCamera = cinemachineCamera.gameObject.GetComponentInChildren<Camera>();
            }
            
            if (virtualCamera != null)
            {
                originalCameraSize = virtualCamera.orthographicSize;
            }
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
        
        if (framingTransposer != null && virtualCamera != null)
        {
            // Store original values
            float startCameraSize = virtualCamera.orthographicSize;
            originalTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            // Add a 2-second delay before changing the camera target
            yield return new WaitForSeconds(2f);
            
            // Change camera to follow NPC
            cinemachineCamera.Follow = npcTransform;
            
            // Zoom in gradually
            float elapsedTime = 0f;
            Vector3 initialTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            while (elapsedTime < NPCcameraTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / NPCcameraTransitionTime;
                
                // Use orthographicSize instead of m_CameraDistance
                virtualCamera.orthographicSize = Mathf.Lerp(startCameraSize, zoomedInSize, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialTrackedOffset, Vector3.zero, t);
                
                yield return null;
            }
            
            // Ensure we reach the exact target values
            virtualCamera.orthographicSize = zoomedInSize;
            framingTransposer.m_TrackedObjectOffset = Vector3.zero;
        }
        else
        {
            Debug.LogError("Required camera components not found!");
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
        yield return new WaitForSeconds(2f);
        
        // Handle monster death animation
        if (monster != null)
        {
            // Remove all children of the monster
            foreach (Transform child in monster.transform)
            {
                Destroy(child.gameObject);
            }

            Animator monsterAnimator = monster.GetComponent<Animator>();
            if (monsterAnimator != null)
            {
                monsterAnimator.enabled = true;
            }
            npcTransform.GetComponent<SpriteRenderer>().enabled = false;

            yield return StartCoroutine(FinalZoomOut());
        }
        else
        {
            // If no monster exists, wait 3 seconds and load next scene
            yield return new WaitForSeconds(delayBeforeSceneChange);
            LoadNextScene();
        }
    }
    
    private IEnumerator FinalZoomOut()
    {
        if (cinemachineCamera != null && virtualCamera != null)
        {
            // Get the framing transposer component
            CinemachineFramingTransposer framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            if (framingTransposer != null)
            {
                float startCameraSize = virtualCamera.orthographicSize;
                Vector3 startTrackedOffset = framingTransposer.m_TrackedObjectOffset;
                
                // ZOOM OUT
                float elapsedTime = 0f;
                while (elapsedTime < cameraTransitionTime)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / cameraTransitionTime;
                    
                    // Use orthographicSize similar to battleSceneManager
                    virtualCamera.orthographicSize = Mathf.Lerp(startCameraSize, zoomedOutSize, t);
                    framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(startTrackedOffset, new Vector3(8.03f, 7.7f, 0), t);
                    
                    yield return null;
                }
                
                // Wait for 3 seconds after zoom out completes
                yield return new WaitForSeconds(delayBeforeSceneChange);
                
                // Load the next scene
                LoadNextScene();
            }
        }
    }
    
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No next scene name specified!");
            // Destroy the object if no scene is specified
            Destroy(gameObject);
        }
    }
    
    private void Turn()
    {
        isFacingRight = !isFacingRight;
        npcTransform.rotation = Quaternion.Euler(0f, isFacingRight ? 0f : 180f, 0f);
    }
}