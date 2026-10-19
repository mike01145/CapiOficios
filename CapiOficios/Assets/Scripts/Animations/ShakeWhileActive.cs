using System;
using UnityEngine;

public class ShakeWhileActive : MonoBehaviour
{
    [SerializeField] private Feedback _startOn = Feedback.GrindStart;
    [SerializeField] private Feedback _stopOn = Feedback.GrindEnd;
    [Tooltip("Solo reacciona a eventos que vengan de este objeto (o sus hijos). Vacío = este mismo objeto.")]
    [SerializeField] private Transform _source;
    [Tooltip("Lo que vibra. Vacío = este objeto. Conviene el modelo visual, no el que tiene los colliders.")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _amplitude = 0.01f;
    [SerializeField] private float _frequency = 35f;

    private bool _shaking;
    private Vector3 _basePosition;

    private void Awake()
    {
        if (_source == null) _source = transform;
        if (_target == null) _target = transform;
    }

    private void OnEnable() => GameEvents.OnFeedbackRaised += ShakeWhileActive_OnFeedback;
    private void OnDisable()
    {
        GameEvents.OnFeedbackRaised -= ShakeWhileActive_OnFeedback;
        if (_shaking) StopShake();
    }
    private void ShakeWhileActive_OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        if (e.Source == null || !(e.Source == _source || e.Source.IsChildOf(_source))) return;

        if (e.Feedback == _startOn && !_shaking)
        {
            _shaking = true;
            _basePosition = _target.localPosition;
        }
        else if (e.Feedback == _stopOn && _shaking) StopShake();
    }
    private void Update()
    {
        if (!_shaking) return;
        float t = Time.unscaledTime * _frequency;
        Vector3 jitter = new Vector3(
            Mathf.Sin(t * 1.3f),
            Mathf.Sin(t * 1.9f + 1f),
            Mathf.Sin(t * 1.1f + 2f)) * _amplitude;
        _target.localPosition = _basePosition + jitter;
    }

    private void StopShake()
    {
        _shaking = false;
        _target.localPosition = _basePosition;
    }
}