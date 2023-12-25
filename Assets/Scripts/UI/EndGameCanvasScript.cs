using TMPro;
using UnityEngine;

public class EndGameCanvasScript : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameEvent endGameEvent;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI loseText;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        endGameEvent.ActionEvent += DidWin;
    }
    private void OnDestroy()
    {
        endGameEvent.ActionEvent -= DidWin;
    }
    #endregion

    #region Event Handler
    public void DidWin(Component sender, object data)
    {
        if(sender is PlayerLOS && data is bool value)
        {
            winText.enabled = value;
            loseText.enabled = !value;
        }
    }
    #endregion
}
