using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum UiSound { Click, Correct, Wrong }

public class AudioManager : MonoBehaviour
{
    private static AudioManager _i;

    public static float MusicVolume { get; private set; } = 1f;
    public static float SfxVolume { get; private set; } = 1f;
    public static AudioMixerGroup SfxGroup => _i != null ? _i._sfxGroup : null;

    private const string MusicKey = "vol_music";
    private const string SfxKey = "vol_sfx";
    private const string MusicParam = "MusicVol";
    private const string SfxParam = "SfxVol";

    [Header("Mixer")]
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private AudioMixerGroup _musicGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;

    [Header("Fuentes")]
    [SerializeField] private AudioSource _music;
    [SerializeField] private AudioSource _sfx;
    [SerializeField] private float _fadeSeconds = 0.6f;

    [Header("Sonidos de UI")]
    [SerializeField] private AudioClip _click;
    [SerializeField] private AudioClip _correct;
    [SerializeField] private AudioClip _wrong;

    private Coroutine _fade;
    private bool _duplicate;

    private void Awake()
    {
        if (_i != null && _i != this) { _duplicate = true; Destroy(gameObject); return; }
        _i = this;
        DontDestroyOnLoad(gameObject);

        _music.outputAudioMixerGroup = _musicGroup;
        _sfx.outputAudioMixerGroup = _sfxGroup;
        _music.loop = true;
        _music.playOnAwake = false;
        _music.volume = 0f;   // el fade lo sube; el volumen real lo maneja el mixer

        MusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
    }

    // El mixer se setea en Start: en Awake a veces el valor se ignora
    private void Start()
    {
        if (_duplicate) return;
        Apply();
    }

    private void Apply()
    {
        _mixer.SetFloat(MusicParam, ToDb(MusicVolume));
        _mixer.SetFloat(SfxParam, ToDb(SfxVolume));
    }

    // 0..1 lineal -> decibeles (0 = silencio)
    private static float ToDb(float v) => Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;

    public static void SetMusicVolume(float v)
    {
        MusicVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        if (_i != null) _i.Apply();
    }

    public static void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        if (_i != null) _i.Apply();
    }

    public static void PlayMusic(AudioClip clip)
    {
        if (_i == null || clip == null) return;
        if (_i._music.clip == clip && _i._music.isPlaying) return;
        if (_i._fade != null) _i.StopCoroutine(_i._fade);
        _i._fade = _i.StartCoroutine(_i.SwapMusic(clip));
    }

    public static void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (_i == null || clip == null) return;
        _i._sfx.PlayOneShot(clip, volume);
    }

    public static void PlayUi(UiSound sound)
    {
        if (_i == null) return;
        var clip = sound == UiSound.Click ? _i._click : sound == UiSound.Correct ? _i._correct : _i._wrong;
        PlaySfx(clip);
    }

    private IEnumerator SwapMusic(AudioClip clip)
    {
        float start = _music.volume;
        for (float t = 0f; t < _fadeSeconds && _music.isPlaying; t += Time.unscaledDeltaTime)
        {
            _music.volume = start * (1f - t / _fadeSeconds);
            yield return null;
        }
        _music.clip = clip;
        _music.Play();
        for (float t = 0f; t < _fadeSeconds; t += Time.unscaledDeltaTime)
        {
            _music.volume = t / _fadeSeconds;
            yield return null;
        }
        _music.volume = 1f;
        _fade = null;
    }

    private void OnDisable()
    {
        if (!_duplicate) PlayerPrefs.Save();   // importante en WebGL
    }
}