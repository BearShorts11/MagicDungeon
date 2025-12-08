using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    Player p;

    public static PlayerUI s;

    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Slider manaSlider;

    [SerializeField]
    private Text currentSpell;
    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text manaText;

    [Header("Notification Box")]
    [SerializeField]
    private GameObject notificationParent;
    [SerializeField]
    private Text notificationText; 
    [SerializeField]
    private float messageDuration = 5f; //Seconds a mesaage stays
    [SerializeField]
    private int maxMessages = 5; //Max messages in box

    //First in, first out (Perfect for a notification box!)
    private Queue<MessageEntry> messageQueue = new Queue<MessageEntry>();

    private void Awake()
    {
        s = this;
    }
    void Start()
    {
        p = GameObject.Find("Player").GetComponent<Player>();
        UpdateMessageParent();
    }

    void Update()
    {
        UIUpdate();
    }

    private void UIUpdate()
    {
        healthSlider.value = p.health;
        manaSlider.value = p.mana;

        currentSpell.text = $"Spell: {p.currentSpell.spellName}";
        healthText.text = $"{p.health.ToString()}/100";
        manaText.text = $"{p.mana.ToString()}/50";
        UpdateMessages();
    }
    
    // Update the UI text each frame
    private void UpdateMessages()
    {
        //remove messages older than duration (first in line will always be oldest, constant comparison best I think)
        while (messageQueue.Count > 0 && Time.time - messageQueue.Peek().timestamp > messageDuration)
        {
            messageQueue.Dequeue();
        }

        // Combine messages into one string, after each message it makes a new line (doesn't feel efficient but idk what else to do)
        string combined = "";
        foreach (var entry in messageQueue)
        {
            combined += entry.text + "\n";
        }

        notificationText.text = combined;

        UpdateMessageParent();
    }

    // call this method to display a new message
    public void AddMessage(string message)
    {
        MessageEntry newEntry = new MessageEntry
        {
            text = message,
            timestamp = Time.time
        };

        messageQueue.Enqueue(newEntry);
        TrimMessages();
    }

    //Immediately remove oldest messages if exceeding max
    private void TrimMessages()
    {
        while (messageQueue.Count > maxMessages)
        {
            messageQueue.Dequeue();
        }
    }

    /// <summary>
    /// Only displays notification panels/text if there is at least 1 message.
    /// </summary>
    private void UpdateMessageParent()
    {
        if (notificationParent != null)
        {
            notificationParent.SetActive(messageQueue.Count > 0);
        }
    }


    //STruct to track message and timestamp for fade-out
    private struct MessageEntry
    {
        public string text;
        public float timestamp;
    }
}
