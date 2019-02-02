using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayIntReference : GameEventListener
{
    public string label;
    public IntReference intRef;

    private TMP_Text tmpText;

    private void Start()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    public void Display()
    {
        tmpText.text = label + " " + intRef.value.ToString();
    }
}
