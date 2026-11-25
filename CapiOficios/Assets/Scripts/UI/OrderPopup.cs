using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderPopup : MonoBehaviour
{
    [SerializeField] private GameObject _overlay;
    [SerializeField] private RectTransform _card;
    [SerializeField] private TextMeshProUGUI _message;
    [SerializeField] private Image _icon;                       // opcional
    [SerializeField] private Button _startButton;
    [SerializeField] private string[] _drinkNames = { "Espresso", "Latte", "Mocha" };  // orden del enum Drink
    [SerializeField] private Sprite[] _icons;                   // opcional, mismo orden
    [SerializeField] private string _format = "¡Un cliente pidió un {0}!";
    [SerializeField] private float _slideSeconds = 0.45f;
    [SerializeField] private float _offscreen = 900f;

    private Vector2 _shownPos;
    private Action _onStart;
    private bool _busy;

    private void Awake()
    {
        _shownPos = _card.anchoredPosition;
        _overlay.SetActive(false);
        _startButton.onClick.AddListener(OnStart);
    }

    public void Show(Drink drink, Action onStart)
    {
        _onStart = onStart;
        int i = (int)drink;
        _message.text = string.Format(_format, i < _drinkNames.Length ? _drinkNames[i] : drink.ToString());
        if (_icon != null)
        {
            bool has = _icons != null && i < _icons.Length && _icons[i] != null;
            _icon.gameObject.SetActive(has);
            if (has) _icon.sprite = _icons[i];
        }
        _overlay.SetActive(true);
        StartCoroutine(Slide(_shownPos + Vector2.down * _offscreen, _shownPos, true, null));
    }

    private void OnStart()
    {
        if (_busy) return;
        var cb = _onStart;
        StartCoroutine(Slide(_shownPos, _shownPos + Vector2.down * _offscreen, false, () =>
        {
            _overlay.SetActive(false);
            cb?.Invoke();
        }));
    }

    private IEnumerator Slide(Vector2 from, Vector2 to, bool overshoot, Action done)
    {
        _busy = true;
        for (float t = 0f; t < _slideSeconds; t += Time.unscaledDeltaTime)
        {
            float p = t / _slideSeconds;
            float k = overshoot ? EaseOutBack(p) : p * p;
            _card.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            yield return null;
        }
        _card.anchoredPosition = to;
        _busy = false;
        done?.Invoke();
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}