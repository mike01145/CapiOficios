using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private Slider _music;
    [SerializeField] private Slider _sfx;

    private void Start()
    {
        if (_music != null)
        {
            _music.SetValueWithoutNotify(AudioManager.MusicVolume);
            _music.onValueChanged.AddListener(AudioManager.SetMusicVolume);
        }
        if (_sfx != null)
        {
            _sfx.SetValueWithoutNotify(AudioManager.SfxVolume);
            _sfx.onValueChanged.AddListener(AudioManager.SetSfxVolume);
        }
    }

}