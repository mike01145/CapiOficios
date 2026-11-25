using UnityEngine;

public class SoundCue : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;
    [SerializeField] private float _delay = 0f;

    public void Play()
    {
        if (_delay <= 0f) AudioManager.PlaySfx(_clip, _volume);
        else Invoke(nameof(PlayNow), _delay);
    }

    private void PlayNow() => AudioManager.PlaySfx(_clip, _volume);
}