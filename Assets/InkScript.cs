using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;
using UnityEngine.SceneManagement;

public class InkScript : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON; // Drag your JSON file here in the inspector

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // Time between characters

    // Ink Story object
    private Story story;
    private bool canContinue = false;
    
    // Typing effect variables
    private bool isTyping = false;
    private bool isTypingComplete = false;
    private string fullText = "";
    private Coroutine typingCoroutine;
    
    // Track special line for scene transition
    private bool hasSpecialLine = false;

    private void Start()
    {
        LoadStory();
    }

    private void Update()
    {
        // Check for space bar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // If currently typing, complete the typing effect
            if (isTyping)
            {
                CompleteTyping();
            }
            // Check if we're on the special line and typing is done
            else if (hasSpecialLine && isTypingComplete)
            {
                // Transition to next scene
                EndStory();
            }
            // Otherwise continue to next line if possible
            else if (canContinue)
            {
                ContinueStory();
            }
        }
    }

    // Initialize the story
    void LoadStory()
    {
        if (inkJSON != null)
        {
            // Create a new Story object using the compiled JSON
            story = new Story(inkJSON.text);
            
            
            // Start the story
            ContinueStory();
        }
        else
        {
            Debug.LogError("Ink JSON file not assigned in the inspector!");
        }
    }

    // Continue to the next line of the story
    void ContinueStory()
    {
        // If still typing and not complete, don't continue
        if (isTyping && !isTypingComplete)
            return;
            
        if (story.canContinue)
        {
            // Get the next line of dialogue
            string text = story.Continue();
            
            // Remove any tags or comments
            text = text.Trim();
            
            // Check if this is our special line
            hasSpecialLine = (text == "But memories can ache.");
            
            // Display the text with typing effect
            DisplayText(text);
            
            // Check if we can continue after this
            canContinue = story.canContinue;
        }
        else if (story.currentChoices.Count > 0)
        {
            // Handle choices (not implemented in this basic version)
            Debug.Log("Story has choices. Not implemented in this basic version.");
        }
        else
        {
            // End of story
            EndStory();
        }
    }

    // Display text in the UI with typing effect
    void DisplayText(string text)
    {
        if (dialogueText != null)
        {
            fullText = text;
            
            // Stop any existing coroutine
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            // Start new typing coroutine
            typingCoroutine = StartCoroutine(TypeText(text, dialogueText));
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
            dialogueText.text = fullText;
        }
    }

    // End the story
    void EndStory()
    {
        Debug.Log("End of story");
        canContinue = false;
        try {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            Debug.Log($"Loading scene index: {nextSceneIndex}");
            SceneManager.LoadSceneAsync(nextSceneIndex);
        } catch (System.Exception e) {
            Debug.LogError($"Failed to load next scene: {e.Message}");
        }
    }
}