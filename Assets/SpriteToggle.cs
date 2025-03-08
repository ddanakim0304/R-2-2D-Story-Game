using UnityEngine;
using System.Collections;
using Ink.Runtime;
using TMPro;

public class SpriteToggle : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Assign in Inspector
    public float duration = 7f; // Time to keep the sprite enabled
    public float fadeDuration = 0.5f; // Fade in/out duration
    private bool isActive = false;
    
    [Header("Dialogue Settings")]
    public TextAsset inkJSON; // Assign your Ink JSON file
    public TextMeshProUGUI dialogueText; // Assign dialogue text UI element
    public PlayerController playerController; // Reference to player controller
    private Story story;
    private bool dialogueStarted = false;
    
    // Track key presses
    private int keyPressCount = 0;
    private const int maxPressesBeforeDialogue = 5;
    
    // Typing effect variables
    private float typingSpeed = 0.05f;
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;
    private bool isDialogueComplete = false;

    void Start()
    {
        // Initialize story but don't start yet
        if (inkJSON != null)
        {
            story = new Story(inkJSON.text);
        }
        
        // Hide dialogue text initially
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        // Find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    void Update()
    {
        // If dialogue is complete, don't allow more interactions
        if (isDialogueComplete)
            return;
            
        if (Input.GetKeyDown(KeyCode.A) && !isActive)
        {
            keyPressCount++;
            
            if (keyPressCount >= maxPressesBeforeDialogue && !dialogueStarted)
            {
                // Start dialogue instead of normal sprite toggle
                StartDialogue();
            }
            else if (!dialogueStarted)
            {
                // Normal sprite toggle behavior until we reach the count
                StartCoroutine(EnableSpriteTemporarily());
            }
        }
        
        // Handle dialogue continuation
        if (dialogueStarted && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                ContinueDialogue();
            }
        }
    }

    IEnumerator EnableSpriteTemporarily()
    {
        isActive = true;
        
        // Make sure sprite renderer is enabled before fading
        spriteRenderer.enabled = true;
        
        // Fade in
        yield return StartCoroutine(FadeSprite(0f, 1f, fadeDuration));
        
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return StartCoroutine(FadeSprite(1f, 0f, fadeDuration));
        
        // Disable sprite after fully faded out
        spriteRenderer.enabled = false;
        isActive = false;
    }
    
    IEnumerator FadeSprite(float startAlpha, float targetAlpha, float fadeDuration)
    {
        Color currentColor = spriteRenderer.color;
        float elapsedTime = 0f;
        
        // Perform the fade over time
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            
            currentColor.a = newAlpha;
            spriteRenderer.color = currentColor;
            
            yield return null;
        }
        
        // Ensure we reach the exact target alpha
        currentColor.a = targetAlpha;
        spriteRenderer.color = currentColor;
    }
    
    void StartDialogue()
    {
        if (inkJSON == null || dialogueText == null)
        {
            Debug.LogError("Missing inkJSON or dialogueText reference!");
            return;
        }
        
        dialogueStarted = true;
        isActive = true;
        
        // Disable player movement during dialogue
        if (playerController != null)
        {
            playerController.DisableControl();
        }
        
        // Make sprite fully visible immediately
        spriteRenderer.enabled = true;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
        
        // Begin dialogue
        ContinueDialogue();
    }
    
    void ContinueDialogue()
    {
        if (story.canContinue)
        {
            string line = story.Continue();
            DisplayText(line);
        }
        else
        {
            EndDialogue();
        }
    }
    
    void DisplayText(string text)
    {
        if (dialogueText != null)
        {
            fullText = text.Trim();
            
            // Stop any existing coroutine
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            // Start new typing coroutine
            typingCoroutine = StartCoroutine(TypeText(fullText, dialogueText));
        }
    }
    
    IEnumerator TypeText(string text, TextMeshProUGUI textComponent)
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
    
    void CompleteTyping()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            isTyping = false;
            dialogueText.text = fullText;
        }
    }
    
    void EndDialogue()
    {
        // Re-enable player control before destroying
        if (playerController != null)
        {
            playerController.EnableControl();
        }
        
        dialogueText.text = "";
        isDialogueComplete = true;
        
        // Destroy the game object after dialogue is complete
        Destroy(gameObject);
    }
}