using UnityEngine;
using Ink.Runtime;
using TMPro;
using Cinemachine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    public TextAsset inkJSON;
    public TextMeshProUGUI dialogueText_NPC;
    public TextMeshProUGUI dialogueText_Player;
    public PlayerController player;
    public string storySection = "";
    public CinemachineVirtualCamera cinemachineCamera;
    public Transform npcTransform;
    public Transform playerTransform;

    public GameObject dialogueTriggerTarget;
    
    // Typing effect variables
    private float typingSpeed = 0.05f; // Time between characters (lower = faster)
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;

    public Story story;
    public bool isDialogueActive = false;
    private bool isPlayerTurn = true;
    public string currentKnot = "";
    
    // Choice handling variables
    private int currentChoiceIndex = 0;
    private bool isShowingChoices = false;
    private bool isTypingComplete = false;

    // Debug option
    [Header("Debug Options")]
    public bool enableDebugSkip = true;
    public KeyCode skipDialogueKey = KeyCode.Backspace;
    

    protected virtual void Start()
    {
        story = new Story(inkJSON.text);
        // Hide text initially
        dialogueText_NPC.text = "";
        dialogueText_Player.text = "";
        
        // If NPC transform isn't set, use this object
        if (npcTransform == null)
            npcTransform = transform;
            
        // Store player transform reference (assuming player reference exists)
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void StartDialogue()
    {
        if (isDialogueActive) return;
    
        isDialogueActive = true;
        player.DisableControl();
        
        // Start camera transition with lerp
        if (cinemachineCamera != null && npcTransform != null)
        {
            StartCoroutine(SmoothCameraTransition(playerTransform, npcTransform, 5.0f)); // 2.0f is duration in seconds
        }
        
        // Choose the starting point if specified
        if (!string.IsNullOrEmpty(storySection))
        {
            story.ChoosePathString(storySection);
            currentKnot = storySection;
        }
        
        ContinueDialogue();
    }
    
    // Add this new coroutine
    private IEnumerator SmoothCameraTransition(Transform fromTarget, Transform toTarget, float duration)
    {
        // Store original camera settings
        CinemachineFramingTransposer framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        float originalXDamping = framingTransposer.m_XDamping;
        float originalYDamping = framingTransposer.m_YDamping;
        float originalZDamping = framingTransposer.m_ZDamping;
        
        // Set higher damping for very smooth movement
        framingTransposer.m_XDamping = 5.0f;
        framingTransposer.m_YDamping = 5.0f;
        framingTransposer.m_ZDamping = 5.0f;
        
        // Set the target
        cinemachineCamera.Follow = toTarget;
        
        // Wait for the transition to complete
        yield return new WaitForSeconds(duration);
        
        // Optionally restore original damping values after transition
        framingTransposer.m_XDamping = originalXDamping;
        framingTransposer.m_YDamping = originalYDamping;
        framingTransposer.m_ZDamping = originalZDamping;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we have a target set and the collider matches that target
        if (dialogueTriggerTarget != null && other.gameObject == dialogueTriggerTarget)
        {
            StartDialogue();
        }
        // Keep the tag check as a fallback if no specific target is set
        else if (dialogueTriggerTarget == null && other.CompareTag("Player"))
        {
            StartDialogue();
        }
    }
    protected virtual void ContinueDialogue()
    {
        // If showing choices or still typing, don't continue
        if (isShowingChoices || (isTyping && !isTypingComplete))
            return;
                
        if (story.canContinue)
        {
            string line = story.Continue();
            DisplayLine(line);
            
            // Reset typing complete flag for new text
            isTypingComplete = false;
            
            // Check if we've moved to a different knot
            string path = story.state.currentPathString;
            if (!string.IsNullOrEmpty(currentKnot) && !string.IsNullOrEmpty(path) && 
                !path.StartsWith(currentKnot) && !path.Equals(currentKnot)) 
            {
                // We've moved beyond our section, so end dialogue
                EndDialogue();
                return;
            }
            
            // Check for choices after continuing
            if (story.currentChoices.Count > 0)
            {
                isShowingChoices = true;
                currentChoiceIndex = 0;
                DisplayCurrentChoice();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void DisplayLine(string line)
    {
        if (line.StartsWith("R-1:"))
        {
            fullText = line.Substring(5);
            dialogueText_Player.text = ""; // Clear previous player text

            isPlayerTurn = false;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(fullText, dialogueText_NPC));
        }
        else
        {
            fullText = line.Substring(5);
            dialogueText_NPC.text = ""; // Clear previous NPC text

            isPlayerTurn = true;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(fullText, dialogueText_Player));
        }
    }

    
    // Coroutine for typewriter effect
    private IEnumerator TypeText(string text, TextMeshProUGUI textComponent)
    {
        isTyping = true;
        isTypingComplete = false;
        textComponent.text = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            textComponent.text += c;
            
            // Skip waiting for spaces
            if (c != ' ')
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        isTyping = false;
        isTypingComplete = true;
    }
        
    // Complete the current typing animation
    private void CompleteTyping()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            isTyping = false;
            isTypingComplete = true;
            
            if (isPlayerTurn)
                dialogueText_Player.text = fullText;
            else
                dialogueText_NPC.text = fullText;
        }
    }
    
    private void DisplayCurrentChoice()
    {
        if (story.currentChoices.Count == 0)
            return;
            
        Choice choice = story.currentChoices[currentChoiceIndex];
        
        // Display the choice with arrow indicators
        string choiceText = $"<- {choice.text} ->";
        
        // Show choice in the player's text area
        dialogueText_Player.text = choiceText;
        dialogueText_NPC.text = "";
    }
    
    private void MakeChoice()
    {
        if (!isShowingChoices || story.currentChoices.Count == 0)
            return;
            
        // Select the current choice
        story.ChooseChoiceIndex(currentChoiceIndex);
        
        // Reset choice state
        isShowingChoices = false;
        
        // Continue to the next dialogue line
        ContinueDialogue();
    }

    protected virtual void EndDialogue()
    {
        dialogueText_NPC.text = "";
        dialogueText_Player.text = "";
        isDialogueActive = false;
        
        // Return camera to follow the player
        if (cinemachineCamera != null && playerTransform != null)
        {
            cinemachineCamera.Follow = playerTransform;
        }
        
        player.EnableControl();
        Destroy(gameObject);
    }

    void Update()
    {
        if (!isDialogueActive)
            return;
            
        // Debug skip dialogue functionality
        if (enableDebugSkip && Input.GetKeyDown(skipDialogueKey))
        {
            EndDialogue();
            return;
        }

        // Handle choices with arrow keys
        if (isShowingChoices)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentChoiceIndex = (currentChoiceIndex - 1 + story.currentChoices.Count) % story.currentChoices.Count;
                DisplayCurrentChoice();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentChoiceIndex = (currentChoiceIndex + 1) % story.currentChoices.Count;
                DisplayCurrentChoice();
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                MakeChoice();
            }
        }

        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // If currently typing, show full text immediately
            if (isTyping)
            {
                CompleteTyping();
            }
            // Otherwise continue to next line
            else
            {
                ContinueDialogue();
            }
        }
    }
}