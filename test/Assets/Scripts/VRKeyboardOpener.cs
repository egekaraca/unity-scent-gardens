using UnityEngine;
using TMPro;

public class VrKeyboardOpener : MonoBehaviour
{
    public TMP_InputField input;

    private void Awake()
    {
        input.onSelect.AddListener(_ => Open());
    }

    public void Open()
    {
        // Default klavyeyi a�ar; overlay olarak g�r�n�r
        var kb = TouchScreenKeyboard.Open(
            input.text,
            TouchScreenKeyboardType.Default,
            autocorrection: false,
            multiline: false,
            secure: false);
    }
}
