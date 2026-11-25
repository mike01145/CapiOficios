using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoStreamingPlayer : MonoBehaviour
{
    [SerializeField] private string _fileName = "intro.mp4";
    [SerializeField] private bool _playOnStart = true;

    private VideoPlayer _vp;

    private void Awake()
    {
        _vp = GetComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.source = VideoSource.Url;
        _vp.url = System.IO.Path.Combine(Application.streamingAssetsPath, _fileName);
        _vp.errorReceived += (v, msg) => Debug.LogError("Video: " + msg);
    }

    private void Start()
    {
        if (_playOnStart) _vp.Play();
    }

    // Por si querés arrancarlo desde un botón
    public void Play() => _vp.Play();
}