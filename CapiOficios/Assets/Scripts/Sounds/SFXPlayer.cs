using System;
using System.Collections.Generic;
using UnityEngine;


public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private SfxRule[] _rules;

    private readonly Dictionary<SfxRule, AudioSource> _loops = new();

    private void OnEnable() => GameEvents.OnFeedbackRaised += OnFeedback;

    private void OnDisable()
    {
        GameEvents.OnFeedbackRaised -= OnFeedback;
        foreach (var s in _loops.Values) if (s != null) s.Stop();
    }

    private void OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        foreach (var r in _rules)
        {
            if (r.loop && r.stopOn == e.Feedback && _loops.TryGetValue(r, out var src) && src != null)
                src.Stop();

            if (r.feedback != e.Feedback || r.clips == null || r.clips.Length == 0) continue;
            var clip = r.clips[UnityEngine.Random.Range(0, r.clips.Length)];

            if (r.loop) PlayLoop(r, clip);
            else AudioManager.PlaySfx(clip, r.volume);
        }
    }

    private void PlayLoop(SfxRule r, AudioClip clip)
    {
        if (!_loops.TryGetValue(r, out var src) || src == null)
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.outputAudioMixerGroup = AudioManager.SfxGroup;   // pasa por el mixer
            _loops[r] = src;
        }
        src.clip = clip;
        src.volume = r.volume;
        src.Play();
    }
}
[Serializable]
public class SfxRule
{
    public Feedback feedback;
    public AudioClip[] clips;                 // si hay varios, elige uno al azar
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop;
    [Tooltip("Solo con Loop: el Feedback que corta el sonido.")]
    public Feedback stopOn = Feedback.None;
}