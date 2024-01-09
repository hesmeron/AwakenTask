using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    private ValueDisplay _currentResultDisplay; 
    [SerializeField]
    private ValueDisplay _resultSumDisplay;
    private int _currenRollSum = 0;

    public void AddDiceResult(int result)
    {
        _currenRollSum += result;
        _currentResultDisplay.SetValue(result);
        _resultSumDisplay.SetValue(_currenRollSum);
    }
    
}
