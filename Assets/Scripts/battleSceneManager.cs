using System.Collections;
using UnityEngine;
using Cinemachine;

public class battleSceneManager : DialogueTrigger
{
    [Header("Battle Scene Settings")]
    public float targetCameraSize = 8f;
    public float initialZoomedOutSize = 8f; // Initial zoom out size
    public float zoomedInSize = 2.35f; // Zoomed in size for dialogue
    public float transitionDuration = 3f; // Duration in seconds for camera transitions
    public float pauseDuration = 4f; // How long to pause when zoomed out
    
    // Store original camera values
    private float originalCameraSize;
    private Vector3 originalTrackedOffset;
    private Vector3 targetTrackedOffset = new Vector3(0, 5.8f, 0);
    private CinemachineFramingTransposer framingTransposer;
    private Camera virtualCamera; // Reference to the actual camera component on followCam
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
            // Get the virtual camera's actual camera component
            virtualCamera = cinemachineCamera.gameObject.GetComponent<Camera>();
            if (virtualCamera == null)
            {
                // If no camera component on cinemachine object, try to find it in children
                virtualCamera = cinemachineCamera.gameObject.GetComponentInChildren<Camera>();
            }
            
            framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            // Store original camera values
            if (framingTransposer != null)
            {
                originalTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            }
            
            // Store original camera size
            if (virtualCamera != null)
            {
                originalCameraSize = virtualCamera.orthographicSize;
            }
            else
            {
                Debug.LogError("Could not find camera component on followCam GameObject!");
            }
        }
    }

    // Override the OnTriggerEnter2D method
    protected override void OnTriggerEnter2D(Collider2D other)
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
        if (cinemachineCamera != null && framingTransposer != null && virtualCamera != null)
        {
            float startCameraSize = virtualCamera.orthographicSize;
            Vector3 startTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            // 1. ZOOM OUT
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                
                // Lerp from current size to initialZoomedOutSize
                virtualCamera.orthographicSize = Mathf.Lerp(startCameraSize, initialZoomedOutSize, t);
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
                virtualCamera.orthographicSize = Mathf.Lerp(initialZoomedOutSize, zoomedInSize, t);
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
        playerController.speed = 1.8f;

        if (npcTransform != null)
        {
            // Disable the npcMovement component
            npcMovement npcMover = npcTransform.GetComponent<npcMovement>();
            if (npcMover != null)
            {
                npcMover.enabled = false;
            }
        }
        
        // Start the final zoom out effect BEFORE destroying the gameObject
        StartCoroutine(FinalZoomOut());
    }
    
    private IEnumerator FinalZoomOut()
    {
        if (cinemachineCamera != null && framingTransposer != null && virtualCamera != null)
        {
            float startCameraSize = virtualCamera.orthographicSize;
            Vector3 startTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            
            // ZOOM OUT
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                
                // Lerp from current size to initialZoomedOutSize
                virtualCamera.orthographicSize = Mathf.Lerp(startCameraSize, initialZoomedOutSize, t);
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(startTrackedOffset, targetTrackedOffset, t);
                
                yield return null;
            }
            
            // Re-enable player control after zoom out is complete
            if (playerController != null)
                playerController.EnableControl();
                
            // Now that the zoom is complete, call the base EndDialogue to clean up
            base.EndDialogue();
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