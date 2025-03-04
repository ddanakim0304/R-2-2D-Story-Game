using System.Collections;
using UnityEngine;
using Cinemachine;

public class battleSceneManager : DialogueTrigger
{
    [Header("Battle Scene Settings")]
    public float targetCameraDistance = 27f;
    public float initialZoomedOutDistance = 27f; // Initial zoom out distance
    public float zoomedInDistance = 10f; // Zoomed in distance for dialogue
    public float transitionDuration = 3f; // Duration in seconds for camera transitions
    public float pauseDuration = 4f; // How long to pause when zoomed out
    
    // Store original camera values
    private float originalCameraDistance;
    private Vector3 originalTrackedOffset;
    private Vector3 targetTrackedOffset = new Vector3(0, 5.8f, 0);
    private CinemachineFramingTransposer framingTransposer;
    private bool hasTriggeredSequence = false;
    private PlayerController playerController;

    [Header("Camera Shake Settings")]
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.2f;

    protected override void Start()
    {
        // Call the base class Start to initialize dialogue system
        base.Start();
        
        // Find player controller
        if (dialogueTriggerTarget != null)
            playerController = dialogueTriggerTarget.GetComponent<PlayerController>();
        
        // Get camera components
        if (cinemachineCamera != null)
        {
            framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            // Store original camera values
            if (framingTransposer != null)
            {
                originalCameraDistance = framingTransposer.m_CameraDistance;
                originalTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            }
        }
    }

    // Override the OnTriggerEnter2D method
    new void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggeredSequence)
            return;

        // Check if this is our target (player)
        if ((dialogueTriggerTarget != null && other.gameObject == dialogueTriggerTarget) ||
            (dialogueTriggerTarget == null && other.CompareTag("Player")))
        {
            hasTriggeredSequence = true;
            
            // Disable player movement
            if (playerController != null)
                playerController.DisableControl();
            
            // Start the camera sequence instead of dialogue directly
            StartCoroutine(InitialCameraSequence());
        }
    }
    
    // Initial camera sequence: zoom out -> pause -> zoom in -> talk
    private IEnumerator InitialCameraSequence()
    {
        if (cinemachineCamera != null && framingTransposer != null)
        {
            float startCameraDistance = framingTransposer.m_CameraDistance;
            Vector3 startTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            // 1. ZOOM OUT
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                
                // Lerp from current distance to initialZoomedOutDistance
                framingTransposer.m_CameraDistance = Mathf.Lerp(startCameraDistance, initialZoomedOutDistance, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(startTrackedOffset, targetTrackedOffset, t);
                
                yield return null;
            }
            
            // 2. PAUSE for specified duration
            yield return new WaitForSeconds(pauseDuration);
            
            // 3. ZOOM IN
            elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                
                // Zoom in
                framingTransposer.m_CameraDistance = Mathf.Lerp(initialZoomedOutDistance, zoomedInDistance, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(targetTrackedOffset, originalTrackedOffset, t);
                
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            // 4. SHAKE CAMERA
            yield return StartCoroutine(ShakeCamera());

            // 5. START DIALOGUE
            base.OnTriggerEnter2D(dialogueTriggerTarget.GetComponent<Collider2D>());
        }
    }
    
    protected override void EndDialogue()
    {
        // Call the base class EndDialogue method
        base.EndDialogue();
        
        // 5. ZOOM OUT after dialogue
        StartCoroutine(AdjustCameraAfterDialogue());
    }

    private IEnumerator AdjustCameraAfterDialogue()
    {
        if (cinemachineCamera != null && framingTransposer != null)
        {
            float startCameraDistance = framingTransposer.m_CameraDistance;
            Vector3 startTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            float elapsedTime = 0f;
            
            // 1. ZOOM OUT to initialZoomedOutDistance
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                
                // Calculate lerp factor (0 to 1)
                float t = elapsedTime / transitionDuration;
                
                // Lerp to initialZoomedOutDistance
                framingTransposer.m_CameraDistance = Mathf.Lerp(startCameraDistance, initialZoomedOutDistance, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(startTrackedOffset, targetTrackedOffset, t);
                
                yield return null;
            }

        }
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalTrackedOffsetCopy = framingTransposer.m_TrackedObjectOffset;
        float elapsedTime = 0f;
        float shakeSpeed = 1.5f;
    
        while (elapsedTime < shakeDuration)
        {
            // Generate random offset for shake effect
            float xOffset = Random.Range(-1f, 1f) * shakeIntensity;
            float yOffset = Random.Range(-1f, 1f) * shakeIntensity;
    
            // Apply shake to tracked offset
            framingTransposer.m_TrackedObjectOffset = new Vector3(
                originalTrackedOffsetCopy.x + xOffset,
                originalTrackedOffsetCopy.y + yOffset,
                originalTrackedOffsetCopy.z
            );
    
            elapsedTime += Time.deltaTime * shakeSpeed; // Increase elapsedTime faster
            yield return null;
        }
    
        // Restore original offset
        framingTransposer.m_TrackedObjectOffset = originalTrackedOffsetCopy;
    }

}