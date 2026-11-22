using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimKind { Pop, Shake, Tilt, Bounce }

[Serializable]
public class AnimRule
{
    public Feedback feedback;
    public AnimKind kind = AnimKind.Pop;
    public float duration = 0.25f;
    [Tooltip("Pop: escala extra (0.15). Shake: metros (0.01). Tilt: grados (30). Bounce: metros (0.05).")]
    public float strength = 0.15f;
    [Tooltip("Si no es None, se repite hasta que llegue este Feedback (ej. GrindStart -> GrindEnd).")]
    public Feedback stopOn = Feedback.None;
}

public class FeedbackAnimator : MonoBehaviour
{
    [SerializeField] private AnimRule[] _rules;

    private class Running
    {
        public Coroutine co;
        public AnimKind kind;
        public Feedback stopOn;
        public Vector3 pos, scale;
        public Quaternion rot;
    }

    private readonly Dictionary<Transform, Running> _running = new();

    private void OnEnable() => GameEvents.OnFeedbackRaised += OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackRaised -= OnFeedback;

    private void OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        if (e.Source == null) return;

        if (_running.TryGetValue(e.Source, out var run) && run.stopOn == e.Feedback)
            Stop(e.Source);

        foreach (var r in _rules)
            if (r.feedback == e.Feedback) Play(e.Source, r);
    }

    private void Play(Transform t, AnimRule r)
    {
        Stop(t);
        var run = new Running
        {
            kind = r.kind,
            stopOn = r.stopOn,
            pos = t.localPosition,
            scale = t.localScale,
            rot = t.localRotation
        };
        _running[t] = run;
        run.co = StartCoroutine(Routine(t, r, run));
    }

    private void Stop(Transform t)
    {
        if (!_running.TryGetValue(t, out var run)) return;
        if (run.co != null) StopCoroutine(run.co);
        Restore(t, run);
        _running.Remove(t);
    }

    // Solo se restaura lo que esa animación modifica (así no pisa la posición de algo que lleva la mano)
    private static void Restore(Transform t, Running run)
    {
        if (t == null) return;
        switch (run.kind)
        {
            case AnimKind.Pop: t.localScale = run.scale; break;
            case AnimKind.Tilt: t.localRotation = run.rot; break;
            default: t.localPosition = run.pos; break;
        }
    }

    private IEnumerator Routine(Transform t, AnimRule r, Running run)
    {
        bool loop = r.stopOn != Feedback.None;
        float time = 0f;

        while (loop || time < r.duration)
        {
            if (t == null) yield break;
            float p = loop ? Mathf.PingPong(time / r.duration, 1f) : time / r.duration;

            switch (r.kind)
            {
                case AnimKind.Pop:
                    t.localScale = run.scale * (1f + r.strength * Mathf.Sin(p * Mathf.PI));
                    break;
                case AnimKind.Tilt:
                    t.localRotation = run.rot * Quaternion.Euler(0f, 0f, Mathf.Sin(p * Mathf.PI) * r.strength);
                    break;
                case AnimKind.Shake:
                    float amp = loop ? 1f : 1f - p;
                    t.localPosition = run.pos + new Vector3(Mathf.Sin(time * 60f), 0f, Mathf.Cos(time * 47f)) * r.strength * amp;
                    break;
                case AnimKind.Bounce:
                    t.localPosition = run.pos + Vector3.up * (Mathf.Abs(Mathf.Sin(p * Mathf.PI * 2f)) * r.strength * (1f - p));
                    break;
            }

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        Restore(t, run);
        _running.Remove(t);
    }
}