using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages Audio Volume
/// </summary>
public class VolumeManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private FloatVariable volumeVariable;
    #endregion

    #region Methods
    private void Start()
    {
        volumeVariable.OnValueChanged += OnVolumeChange;
    }
    private void OnDisable()
    {
        volumeVariable.OnValueChanged -= OnVolumeChange;
    }

    /// <summary>
    /// Changes Volume
    /// </summary>
    /// <param name="variable">Slider value</param>
    private void OnVolumeChange(float variable)
    {
        AudioListener.volume = variable;
    }

 
    #endregion
}
