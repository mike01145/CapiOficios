using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSuccessFeedback : MonoBehaviour
{
    [SerializeField] private Image _target;                 // si queda vacío usa la Image del botón
    [SerializeField] private Color _successColor = new Color(0.30f, 0.65f, 0.35f);
    [SerializeField] private float _popScale = 1.12f;       // tamaño del "pop"
    [SerializeField] private float _popTime = 0.12f;
    [SerializeField] private float _delayAfter = 0.25f;     // pausa antes de ejecutar la función
    [SerializeField] private bool _restoreAfter = true;
    [SerializeField] private UnityEvent _onFinished;        // acá va Confirm()

    private Button _button;
    private bool _playing;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_target == null) _target = GetComponent<Image>();

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

        Color original = _target.color;
        Vector3 baseScale = transform.localScale;

        _target.color = _successColor;

        // Pop: crece y vuelve
        yield return Scale(baseScale, baseScale * _popScale, _popTime);
        yield return Scale(baseScale * _popScale, baseScale, _popTime);

        yield return new WaitForSecondsRealtime(_delayAfter);

        _onFinished?.Invoke();

        if (_restoreAfter)
        {
            _target.color = original;
            transform.localScale = baseScale;
        }
        _playing = false;
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