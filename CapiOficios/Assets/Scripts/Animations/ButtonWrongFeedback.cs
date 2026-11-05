using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonWrongFeedback : MonoBehaviour
{
    public enum AfterAction { DisableInteraction, DisableGameObject }

    [SerializeField] private Image _target;
    [SerializeField] private Sprite _wrongSprite;            // imagen de "mal"
    [SerializeField] private Color _wrongColor = new(0.85f, 0.25f, 0.25f);
    [SerializeField] private float _shakeAmount = 10f;       // en píxeles
    [SerializeField] private float _shakeSpeed = 50f;
    [SerializeField] private float _showTime = 1.2f;         // segundos con la imagen de error
    [SerializeField] private AfterAction _afterAction = AfterAction.DisableInteraction;
    [SerializeField] private UnityEvent _onFinished;         // opcional

    private Button _button;
    private bool _playing;
    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_target == null) _target = GetComponent<Image>();

        _button.onClick.AddListener(Play);
    }
    void OnEnable()
    {
        _button.interactable = true;
    }
    public void Play()
    {
        if (_playing) return;
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        _playing = true;
        _button.interactable = false;

        Color originalColor = _target.color;
        Sprite originalSprite = _target.sprite;
        Vector3 basePos = transform.localPosition;

        _target.color = _wrongColor;
        if (_wrongSprite != null) _target.sprite = _wrongSprite;

        // Shake + espera
        for (float t = 0f; t < _showTime; t += Time.unscaledDeltaTime)
        {
            float fade = 1f - (t / _showTime);   // el temblor se va apagando
            float offset = Mathf.Sin(t * _shakeSpeed) * _shakeAmount * fade;
            transform.localPosition = basePos + new Vector3(offset, 0f, 0f);
            yield return null;
        }
        transform.localPosition = basePos;

        // Restauro el aspecto (por si después se vuelve a activar)
        _target.color = originalColor;
        _target.sprite = originalSprite;

        _playing = false;
        _onFinished?.Invoke();

        if (_afterAction == AfterAction.DisableGameObject)
            gameObject.SetActive(false);
        // si es DisableInteraction, queda con interactable = false
    }
}