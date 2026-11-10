using System.Collections;
using UnityEngine;

public enum SlideFrom { Down, Up, Left, Right }
[RequireComponent(typeof(RectTransform))]
public class SlidePanel : MonoBehaviour
{
    [SerializeField] private SlideFrom _slidefrom = SlideFrom.Down;
    [SerializeField] private float _duration = 0.35f;
    [Tooltip("0 = automático (sale desde fuera de la pantalla)")]
    [SerializeField] private float _slideDistance = 0f;

    private RectTransform _rt;
    private Vector2 _targetPos;
    private Coroutine _running;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _targetPos = _rt.anchoredPosition;   // la posición final es la del editor
    }

    private void OnEnable()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(Slide(true));
    }

    // Para cerrar CON animación usá esto en lugar de SetActive(false)
    public void Close()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(Slide(false));
    }
    // Para cerrar SIN animación usá esto en lugar de SetActive(false)
    private Vector2 OffscreenOffset()
    {
        Vector2 dir;
        switch (_slidefrom)
        {
            case SlideFrom.Up: dir = Vector2.up; break;
            case SlideFrom.Left: dir = Vector2.left; break;
            case SlideFrom.Right: dir = Vector2.right; break;
            default: dir = Vector2.down; break;
        }

        float distance = _slideDistance;
        if (distance <= 0f)
        {
            var canvas = GetComponentInParent<Canvas>();
            Rect r = canvas != null ? ((RectTransform)canvas.rootCanvas.transform).rect : new Rect(0, 0, 1000, 1000);
            distance = dir.y != 0f ? r.height : r.width;
        }
        return dir * distance;
    }

    private IEnumerator Slide(bool opening)
    {
        Vector2 away = _targetPos + OffscreenOffset();
        Vector2 start = opening ? away : _targetPos;
        Vector2 end = opening ? _targetPos : away;

        _rt.anchoredPosition = start;

        for (float t = 0f; t < _duration; t += Time.unscaledDeltaTime)
        {
            float k = t / _duration;
            k = 1f - Mathf.Pow(1f - k, 3f); // suave al final (ease out)
            _rt.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
            yield return null;
        }

        _rt.anchoredPosition = end;
        _running = null;

        if (!opening)
        {
            _rt.anchoredPosition = _targetPos; // lo deja listo para la próxima vez
            gameObject.SetActive(false);
        }
    }
}