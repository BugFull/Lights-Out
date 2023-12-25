using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Raises an event when count down time ends
/// </summary>
public class CountDownTimer : MonoBehaviour
{
    #region Fields
    [SerializeField] private int countDownTime;
    [SerializeField] private TextMeshProUGUI displayText;

    private readonly WaitForSeconds oneSecond = new(1f);
    public event Action Done;
    #endregion

    #region Methods
    /// <summary>
    /// Starts the timer
    /// </summary>
    public void Begin()
    {        
        StartCoroutine(CountDown());
    }

    /// <summary>
    /// Count downs time 
    /// </summary>
    private IEnumerator CountDown()
    {
        int Count = countDownTime;
        while(Count > 0)
        {
            displayText.text = Count.ToString();
            yield return oneSecond;
            Count--;
        }
        Done?.Invoke();
    }
    #endregion
}
