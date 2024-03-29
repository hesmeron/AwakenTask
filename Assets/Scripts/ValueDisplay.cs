using TMPro;
using UnityEngine;

public class ValueDisplay : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text _text;
    [SerializeField] 
    private string _prefix = "Value:";

    public void SetValue(int value)
    {
        _text.text = _prefix + value.ToString();
    }    
    
    public void SetValue(string value)
    {
        _text.text = _prefix + value;
    }
}
