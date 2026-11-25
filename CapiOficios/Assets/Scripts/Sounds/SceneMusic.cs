using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField] private bool _playOnStart = true;
    private void Start()
    {
        if (_playOnStart) AudioManager.PlayMusic(_clip);
    }

    public void PlayMusic() => AudioManager.PlayMusic(_clip);
}