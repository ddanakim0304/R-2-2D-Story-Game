using UnityEngine;
using Ink.Runtime;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    public TextAsset inkJSON;
    // public GameObject dialogueUI_NPC;  // Comment out UI panels
    // public GameObject dialogueUI_Player;
    public TextMeshProUGUI dialogueText_NPC;
    public TextMeshProUGUI dialogueText_Player;
    public PlayerController player;

    private Story story;
    private bool isDialogueActive = false;
    private bool isR1Turn = true;

    void Start()
    {
        story = new Story(inkJSON.text);
        // Hide text instead of UI panels
        dialogueText_NPC.text = "";
        dialogueText_Player.text = "";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (isDialogueActive) return;

        isDialogueActive = true;
        player.DisableControl();
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (story.canContinue)
        {
            string line = story.Continue();
            DisplayLine(line);
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
            dialogueText_NPC.text = line.Substring(3);
            dialogueText_Player.text = "";  // Clear other text
            isR1Turn = false;
        }
        else if (line.StartsWith("R-2:"))
        {
            dialogueText_Player.text = line.Substring(3);
            dialogueText_NPC.text = "";  // Clear other text
            isR1Turn = true;
        }
    }

    void EndDialogue()
    {
        dialogueText_NPC.text = "";
        dialogueText_Player.text = "";
        isDialogueActive = false;
        player.EnableControl();
        Destroy(gameObject);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            ContinueDialogue();
        }
    }
}