using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickPopAnimator : MonoBehaviour
{
    [SerializeField] private float _scaleMultiplier = 1.12f;
    [SerializeField] private float _duration = 0.18f;
    [Tooltip("0 a 1 en el tiempo; el valor es cuánto del agrandado se aplica (sube y vuelve a 0).")]
    [SerializeField]
    private AnimationCurve _curveAnimation = new(
        new Keyframe(0f, 0f), new Keyframe(0.35f, 1f), new Keyframe(1f, 0f));

    private readonly Dictionary<Transform, Vector3> _originals = new();
    private readonly Dictionary<Transform, Coroutine> _running = new();

    private void OnEnable() => GameEvents.OnFeedbackRaised += ClickPopAnimator_OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackRaised -= ClickPopAnimator_OnFeedback;

    private void ClickPopAnimator_OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        if (e.Feedback != Feedback.Click || e.Source == null) return;

        if (_running.TryGetValue(e.Source, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
            e.Source.localScale = _originals[e.Source];   // vuelve a la escala real antes de repetir
        }
        else _originals[e.Source] = e.Source.localScale;

        _running[e.Source] = StartCoroutine(Pop(e.Source));
    }
    private IEnumerator Pop(Transform target)
    {
        Vector3 baseScale = _originals[target];

        for (float t = 0f; t < _duration; t += Time.unscaledDeltaTime)
        {
            if (target == null) yield break;
            float k = _curveAnimation.Evaluate(t / _duration);
            target.localScale = baseScale * (1f + (_scaleMultiplier - 1f) * k);
            yield return null;
        }

        if (target != null) target.localScale = baseScale;
        _running.Remove(target);
    }
}