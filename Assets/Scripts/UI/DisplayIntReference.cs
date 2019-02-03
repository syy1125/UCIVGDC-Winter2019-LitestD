using TMPro;

public class DisplayIntReference : GameEventListener
{
    public string label;
    public IntReference intRef;

    public void Display()
    {
        GetComponent<TextMeshProUGUI>().text = label + " " + intRef.value;
    }
}
