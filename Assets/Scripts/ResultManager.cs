using UnityEngine;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    private ValueDisplay _currentResultDisplay; 
    [SerializeField]
    private ValueDisplay _resultSumDisplay;
    private int _currenRollSum = 0;

    public void AddRollResult(int result)
    {
        _currenRollSum += result;
        _currentResultDisplay.SetValue(result);
        _resultSumDisplay.SetValue(_currenRollSum);
    }

    public void StartRoll()
    {
        _currentResultDisplay.SetValue("?");
    }    
    
    public void DiscardRoll()
    {
        _currentResultDisplay.SetValue("-");
    }
    
}
