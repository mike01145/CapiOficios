using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchingGame : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform _lineContainer;
    [SerializeField] private Sprite _arrowSprite;

    [Header("Rondas (opcional)")]
    [SerializeField] private GameObject[] _rounds;

    [Header("Errores")]
    [SerializeField] private int _maxFails = 3; // al fallar MÁS de este número se abre el panel
    [SerializeField] private GameObject _failPanel;
    [SerializeField] private UnityEvent _onTooManyFails;

    [Header("Estilo")]
    [SerializeField] private float _thickness = 12f;
    [SerializeField] private float _headSize = 40f;
    [SerializeField] private float _correctDelay = 0.35f;  // cuánto se ve la flecha verde antes de desaparecer
    [SerializeField] private Color _dragColor = Color.white;
    [SerializeField] private Color _correctColor = new Color(0.30f, 0.65f, 0.35f);
    [SerializeField] private Color _wrongColor = new Color(0.85f, 0.25f, 0.25f);

    [Header("Eventos")]
    [SerializeField] private UnityEvent _onCompleted;

    private class Link
    {
        public RectTransform line, head;
        public Image lineImg, headImg;
        public ConnectorSource source;
    }

    private Link _currentLink;
    private Sprite _triangle;
    private int _roundIndex;
    private int _remainingInRound;
    private int _fails;
    private bool _locked;

    private void Awake()
    {
        _triangle = _arrowSprite != null ? _arrowSprite : CreateTriangleSprite();
        if (_failPanel != null) _failPanel.SetActive(false);
        ShowRound(0);
    }

    // ---------- Rondas ----------
    private bool HasRounds => _rounds != null && _rounds.Length > 0;

    private ConnectorSource[] AllSources() => HasRounds ? GatherFromRounds<ConnectorSource>()
        : FindObjectsByType<ConnectorSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private ConnectorTarget[] AllTargets() => HasRounds ? GatherFromRounds<ConnectorTarget>()
        : FindObjectsByType<ConnectorTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private T[] GatherFromRounds<T>() where T : Component
    {
        var list = new List<T>();
        foreach (var r in _rounds) list.AddRange(r.GetComponentsInChildren<T>(true));
        return list.ToArray();
    }

    private void ShowRound(int index)
    {
        _roundIndex = index;
        if (HasRounds)
        {
            for (int i = 0; i < _rounds.Length; i++) _rounds[i].SetActive(i == index);
            _remainingInRound = _rounds[index].GetComponentsInChildren<ConnectorSource>(true).Length;
        }
        else
        {
            _remainingInRound = AllSources().Length;
        }
    }

    private void OnLinkSolved()
    {
        _remainingInRound--;
        if (_remainingInRound > 0) return;

        if (HasRounds && _roundIndex + 1 < _rounds.Length) ShowRound(_roundIndex + 1);
        else _onCompleted?.Invoke();
    }

    // ---------- Llamado desde ConnectorSource ----------
    public void BeginLink(ConnectorSource source, PointerEventData e)
    {
        if (_locked || source.Connected) return;
        _currentLink = CreateLink(_dragColor);
        _currentLink.source = source;
        UpdateLink(e);
    }

    public void UpdateLink(PointerEventData e)
    {
        if (_currentLink == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_lineContainer, e.position, e.pressEventCamera, out Vector2 p);
        Draw(_currentLink, LocalCenter(_currentLink.source.transform as RectTransform), p);
    }

    public void EndLink(PointerEventData e)
    {
        if (_currentLink == null) return;

        ConnectorTarget hit = null;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(e, results);
        foreach (var r in results)
        {
            hit = r.gameObject.GetComponentInParent<ConnectorTarget>();
            if (hit != null) break;
        }

        Link link = _currentLink;
        _currentLink = null;

        if (hit != null && !hit.Solved && hit.id == link.source.id)
        {
            // CORRECTO: la flecha se ve verde un momento y después desaparece todo
            hit.Solved = true;
            link.source.Connected = true;
            Draw(link, LocalCenter(link.source.transform as RectTransform), LocalCenter(hit.transform as RectTransform));
            SetColor(link, _correctColor);
            StartCoroutine(SolveRoutine(link, hit));
        }
        else
        {
            // INCORRECTO: el destino parpadea en rojo y la flecha se desvanece
            if (hit != null && !hit.Solved)
            {
                hit.Blink(_wrongColor);
                RegisterFail();
            }
            StartCoroutine(FadeAndDestroy(link));
        }
    }

    private void RegisterFail()
    {
        _fails++;
        if (_fails > _maxFails)
        {
            _locked = true;
            if (_failPanel != null) _failPanel.SetActive(true);
            _onTooManyFails?.Invoke();
        }
    }

    private IEnumerator SolveRoutine(Link link, ConnectorTarget target)
    {
        yield return new WaitForSecondsRealtime(_correctDelay);
        Destroy(link.line.gameObject);
        link.source.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
        OnLinkSolved();
    }

    // Llamalo desde el panel (por ejemplo, botón "Reintentar")
    public void ResetGame()
    {
        StopAllCoroutines();
        if (_currentLink != null) { Destroy(_currentLink.line.gameObject); _currentLink = null; }
        for (int i = _lineContainer.childCount - 1; i >= 0; i--) Destroy(_lineContainer.GetChild(i).gameObject);

        foreach (var s in AllSources()) { s.Connected = false; s.gameObject.SetActive(true); }
        foreach (var t in AllTargets()) { t.Solved = false; t.gameObject.SetActive(true); }

        _fails = 0;
        _locked = false;
        if (_failPanel != null) _failPanel.SetActive(false);
        ShowRound(0);
    }

    // ---------- Dibujo ----------
    private Link CreateLink(Color color)
    {
        var l = new Link();

        var lineGO = new GameObject("Line", typeof(RectTransform), typeof(Image));
        l.line = lineGO.GetComponent<RectTransform>();
        l.line.SetParent(_lineContainer, false);
        l.line.pivot = new Vector2(0f, 0.5f);
        l.lineImg = lineGO.GetComponent<Image>();
        l.lineImg.raycastTarget = false;

        var headGO = new GameObject("Head", typeof(RectTransform), typeof(Image));
        l.head = headGO.GetComponent<RectTransform>();
        l.head.SetParent(l.line, false);
        l.head.pivot = new Vector2(1f, 0.5f);
        l.headImg = headGO.GetComponent<Image>();
        l.headImg.sprite = _triangle;
        l.headImg.raycastTarget = false;

        SetColor(l, color);
        return l;
    }

    private void Draw(Link l, Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;
        float dist = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        l.line.localPosition = a;
        l.line.localRotation = Quaternion.Euler(0, 0, angle);
        l.line.sizeDelta = new Vector2(Mathf.Max(0f, dist - _headSize * 0.5f), _thickness);

        l.head.anchorMin = l.head.anchorMax = new Vector2(1f, 0.5f);
        l.head.anchoredPosition = new Vector2(_headSize * 0.5f, 0f);
        l.head.sizeDelta = new Vector2(_headSize, _headSize);
    }

    private void SetColor(Link l, Color c) { l.lineImg.color = c; l.headImg.color = c; }

    private Vector2 LocalCenter(RectTransform rt)
    {
        Vector3 world = rt.TransformPoint(rt.rect.center);
        return _lineContainer.InverseTransformPoint(world);
    }

    private IEnumerator FadeAndDestroy(Link l)
    {
        SetColor(l, _wrongColor);
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            var c = _wrongColor; c.a = 1f - t / 0.4f;
            SetColor(l, c);
            yield return null;
        }
        Destroy(l.line.gameObject);
    }

    private Sprite CreateTriangleSprite()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float halfHeight = (1f - x / (float)size) * size * 0.5f;
                bool inside = Mathf.Abs(y - size * 0.5f) <= halfHeight;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
