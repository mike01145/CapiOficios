using UnityEngine;
using UnityEngine.Video;

public class VideoContinueGate : MonoBehaviour
{
    [SerializeField] private VideoPlayer _video;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private float _showAfterPercent = 1f;   // 1 = al terminar; 0.9 = al 90%
    [SerializeField] private bool _showOnError = true;       // si el video falla, no dejar al jugador trabado

    private bool _shown;

    private void Awake()
    {
        if (_video == null) _video = GetComponent<VideoPlayer>();
        _video.isLooping = false;                 // con loop, loopPointReached se repite
        _video.loopPointReached += OnEnd;
        _video.errorReceived += OnError;
        _continueButton.SetActive(false);
    }

    private void OnEnable()
    {
        _shown = false;
        _continueButton.SetActive(false);
    }

    private void Update()
    {
        // Mostrarlo antes del final, si lo configuraste (por ejemplo al 90 %)
        if (_shown || _showAfterPercent >= 1f) return;
        if (_video.isPlaying && _video.length > 0 && _video.time / _video.length >= _showAfterPercent)
            Show();
    }

    private void OnEnd(VideoPlayer vp) => Show();

    private void OnError(VideoPlayer vp, string msg)
    {
        Debug.LogError("Video: " + msg);
        if (_showOnError) Show();
    }

    private void Show()
    {
        if (_shown) return;
        _shown = true;
        _continueButton.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_video == null) return;
        _video.loopPointReached -= OnEnd;
        _video.errorReceived -= OnError;
    }
}