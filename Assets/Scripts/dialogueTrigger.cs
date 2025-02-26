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
    private Transform playerTransform;

    public GameObject dialogueTriggerTarget;
    
    // Typing effect variables
    private float typingSpeed = 0.12f; // Time between characters (lower = faster)
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;

    private Story story;
    private bool isDialogueActive = false;
    private bool isR1Turn = true;
    private string currentKnot = "";
    
    // Choice handling variables
    private int currentChoiceIndex = 0;
    private bool isShowingChoices = false;

    void Start()
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

    void StartDialogue()
    {
        if (isDialogueActive) return;

        isDialogueActive = true;
        player.DisableControl();
        
        // Make camera follow the NPC with smooth transition
        if (cinemachineCamera != null && npcTransform != null)
        {
            // Set higher damping values before changing the follow target
            CinemachineFramingTransposer framingTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                // Higher values = slower transitions (more damping)
                framingTransposer.m_XDamping = 2.5f;
                framingTransposer.m_YDamping = 2.5f;
                framingTransposer.m_ZDamping = 2.5f;
            }
            
            // Now set the follow target
            cinemachineCamera.Follow = npcTransform;
        }
        
        // Choose the starting point if specified
        if (!string.IsNullOrEmpty(storySection))
        {
            story.ChoosePathString(storySection);
            currentKnot = storySection;
        }
        
        ContinueDialogue();
    }
    void OnTriggerEnter2D(Collider2D other)
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
    public void ContinueDialogue()
    {
        // If showing choices, don't continue
        if (isShowingChoices)
            return;
            
        if (story.canContinue)
        {
            string line = story.Continue();
            DisplayLine(line);
            
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
            dialogueText_Player.text = ""; // Clear other text
            isR1Turn = false;
            
            // Start typing effect for NPC text
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(fullText, dialogueText_NPC));
        }
        else
        {
            fullText = line.Substring(0);
            dialogueText_NPC.text = ""; // Clear other text
            isR1Turn = true;
            
            // Start typing effect for Player text
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(fullText, dialogueText_Player));
        }
    }
    
    // Coroutine for typewriter effect
    private IEnumerator TypeText(string text, TextMeshProUGUI textComponent)
    {
        isTyping = true;
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
    }
        
    // Complete the current typing animation
    private void CompleteTyping()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            isTyping = false;
            
            if (isR1Turn)
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

    void EndDialogue()
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
            
        // Handle choices with arrow keys
        if (isShowingChoices)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Move to previous choice (wrap around)
                currentChoiceIndex--;
                if (currentChoiceIndex < 0)
                    currentChoiceIndex = story.currentChoices.Count - 1;
                    
                DisplayCurrentChoice();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Move to next choice (wrap around)
                currentChoiceIndex++;
                if (currentChoiceIndex >= story.currentChoices.Count)
                    currentChoiceIndex = 0;
                    
                DisplayCurrentChoice();
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // Make the selected choice
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