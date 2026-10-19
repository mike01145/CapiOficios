using System;
using UnityEngine;

public class HintArrow : MonoBehaviour
{
    [SerializeField] private StepTracker _steptracker;
    [SerializeField] private float _idleSeconds = 10f;
    [SerializeField] private Vector3 _offset = new(0f, 0.3f, 0f);
    [SerializeField] private float _bounceHeight = 0.05f;
    [SerializeField] private float _bounceSpeed = 6f;
    [Tooltip("Tamaño de la flecha en el mundo.")]
    [SerializeField] private float _size = 0.12f;
    [SerializeField] private Color _color = new(1f, 0.8f, 0.2f);
    [Tooltip("Opcional: un sprite de flecha apuntando hacia abajo, con el pivote en la punta.")]
    [SerializeField] private Sprite _arrowSprite;

    private SpriteRenderer _sr;
    private Camera _cam;
    private float _lastActivity;

    private void Awake()
    {
        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sprite = _arrowSprite != null ? _arrowSprite : CreateArrowSprite();
        _sr.color = _color;
        _sr.sortingOrder = 100;
        _sr.enabled = false;
        transform.localScale = Vector3.one * _size;
    }

    private void Start() => _cam = Camera.main;

    private void OnEnable()
    {
        _lastActivity = Time.unscaledTime;
        GameEvents.OnFeedbackRaised += HintArrowUI_OnActivity;
    }

    private void OnDisable() => GameEvents.OnFeedbackRaised -= HintArrowUI_OnActivity;
    private void HintArrowUI_OnActivity(object sender, GameEvents.FeedbackEventArgs e) => _lastActivity = Time.unscaledTime;

    private void Update()
    {
        Transform target = _steptracker != null ? _steptracker.NextTarget() : null;
        bool show = target != null && Time.unscaledTime - _lastActivity >= _idleSeconds;
        _sr.enabled = show;
        if (!show || _cam == null) return;

        float bounce = Mathf.Abs(Mathf.Sin(Time.unscaledTime * _bounceSpeed)) * _bounceHeight;
        transform.SetPositionAndRotation(target.position + _offset + Vector3.up * bounce, _cam.transform.rotation);
    }

    // Triángulo apuntando hacia abajo, con el pivote en la punta
    private static Sprite CreateArrowSprite()
    {
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                bool inside = Mathf.Abs(x - n * 0.5f) <= y * 0.5f;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0f), n);
    }
}