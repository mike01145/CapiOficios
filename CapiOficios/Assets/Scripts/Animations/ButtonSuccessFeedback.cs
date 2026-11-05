using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSuccessFeedback : MonoBehaviour
{
    [SerializeField] private Image _target; // si queda vacío usa la Image del botón
    [SerializeField] private Sprite _successSprite;
    [SerializeField] private Color _successColor = new(0.30f, 0.65f, 0.35f);
    [SerializeField] private GameObject _successMessage;
    [SerializeField] private float _popScale = 1.12f; // tamaño del "pop"
    [SerializeField] private float _popTime = 0.12f;
    [SerializeField] private float _showTime = 1.5f;
    [SerializeField] private bool _restoreAfter = true;
    [SerializeField] private UnityEvent _onFinished;  // acá va Confirm()

    private Button _button;
    private bool _playing;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_target == null) _target = GetComponent<Image>();
        if (_successMessage != null) _successMessage.SetActive(false);

        _button.onClick.AddListener(Play);
    }

    // Conectá ESTE método al OnClick del botón
    public void Play()
    {
        if (_playing) return;
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        _playing = true;
        _button.interactable = false;

        Color original = _target.color;
        Sprite originalSprite = _target.sprite;
        Vector3 baseScale = transform.localScale;

        _target.color = _successColor;
        if (_successSprite != null) _target.sprite = _successSprite;
        if (_successMessage != null) _successMessage.SetActive(true);

        // Pop: crece y vuelve
        yield return Scale(baseScale, baseScale * _popScale, _popTime);
        yield return Scale(baseScale * _popScale, baseScale, _popTime);

        yield return new WaitForSecondsRealtime(_showTime);

        if (_successMessage != null) _successMessage.SetActive(false);


        if (_restoreAfter)
        {
            _target.color = original;
            _target.sprite = originalSprite;
            transform.localScale = baseScale;
        }
        _button.interactable = true;
        _playing = false;

        _onFinished?.Invoke();
    }

    private IEnumerator Scale(Vector3 from, Vector3 to, float time)
    {
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(from, to, t / time);
            yield return null;
        }
        transform.localScale = to;
    }
}