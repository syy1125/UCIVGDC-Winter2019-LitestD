using TMPro;

public class DisplayIntReference : GameEventListener
{
    private string template;
    public IntReference[] refs;

    private void Start()
    {
        template = GetComponent<TextMeshProUGUI>().text;
        Display();
    }

    public void Display()
    {
        GetComponent<TextMeshProUGUI>().text = string.Format(template, refs);
    }
}
