using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Ponelo en cada destino de la DERECHA (con el mismo id que su botón).
// Tiene que tener una Image (o cualquier Graphic) para poder parpadear.
public class ConnectorTarget : MonoBehaviour
{
    public int id;
    [HideInInspector] public bool Solved;
    private Graphic _graphic;
    private Color _original;
    private Coroutine _blinking;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        if (_graphic == null) _graphic = GetComponentInChildren<Graphic>();
        if (_graphic != null) _original = _graphic.color;
    }

    public void Blink(Color color, int times = 3, float interval = 0.12f)
    {
        if (_graphic == null) return;
        if (_blinking != null) { StopCoroutine(_blinking); _graphic.color = _original; }
        _blinking = StartCoroutine(BlinkRoutine(color, times, interval));
    }

    private IEnumerator BlinkRoutine(Color color, int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            _graphic.color = color;
            yield return new WaitForSecondsRealtime(interval);
            _graphic.color = _original;
            yield return new WaitForSecondsRealtime(interval);
        }
        _blinking = null;
    }

    private void OnDisable()
    {
        if (_graphic != null) _graphic.color = _original;
        _blinking = null;
    }
}