using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CongrastPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private CanvasGroup _group;        // opcional: para el fade
    [SerializeField] private float _fadeSeconds = 0.4f;
    [SerializeField] private float _delay = 0.5f;       // pausa antes de mostrarlo
    [SerializeField] private UnityEvent _onContinue;

    private void Awake()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    // Conectalo al On Finished del TutorialManager
    public void Show() => StartCoroutine(ShowRoutine());

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForSecondsRealtime(_delay);
        _panel.SetActive(true);

        if (_group != null)
        {
            _group.alpha = 0f;
            for (float t = 0f; t < _fadeSeconds; t += Time.unscaledDeltaTime)
            {
                _group.alpha = t / _fadeSeconds;
                yield return null;
            }
            _group.alpha = 1f;
        }
    }

    // Conectalo al botón "Continuar"
    public void Continue()
    {
        _panel.SetActive(false);
        _onContinue?.Invoke();
    }
}
