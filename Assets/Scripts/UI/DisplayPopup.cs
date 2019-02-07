using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayPopup : MonoBehaviour
{
    public TextMeshProUGUI textField;
    public Button continueButton;

    private Queue<string> messages = new Queue<string>();
    private Animator anim;
    private string visible = "Visible"; // So I don't mess up spelling in multiple places

    private void Start()
    {
        anim = GetComponent<Animator>();
    
        continueButton.onClick.AddListener(ShowNextMessage); // Would it be better to do this in the Unity Editor?
    }

    // Generate a random string with a given size
    // I just copied this from internet for testing purposes.
    // Will be removed.
    public string RandomString(int size, bool lowerCase)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        System.Random random = new System.Random();
        char ch;
        for (int i = 0; i < size; i++)
        {
            ch = System.Convert.ToChar(System.Convert.ToInt32(System.Math.Floor(26 * random.NextDouble() + 65)));
            builder.Append(ch);
        }
        if (lowerCase)
            return builder.ToString().ToLower();
        return builder.ToString();
    }

    public void EnqueueMessage(string message)
    {
        message = RandomString(20, true); // Just for testing
        messages.Enqueue(message);

        if (messages.Count == 1 && !anim.GetBool(visible))
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (messages.Count == 0)
        {
            anim.SetBool(visible, false);
        }
        else
        {
            textField.text = messages.Dequeue();
            anim.SetBool(visible, true); // Maybe I should check if it equals false first
        }
    }
}
