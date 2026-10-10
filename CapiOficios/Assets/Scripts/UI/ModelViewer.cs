using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Ponelo en el mismo objeto que el RawImage donde querés ver el modelo 3D
[RequireComponent(typeof(RawImage))]
public class ModelViewer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("Referencias")]
    [SerializeField] private Camera _viewCamera;
    [SerializeField] private Transform[] _models;
    [Header("Rotación")]
    [SerializeField] private float _rotateSpeed = 0.4f;
    [SerializeField] private float _autoRotateSpeed = 20f;
    [SerializeField] private float _idleTime = 2f;
    [Header("Zoom (rueda del mouse)")]
    [SerializeField] private bool _allowZoom = true;
    [SerializeField] private float _minFov = 20f;
    [SerializeField] private float _maxFov = 50f;
    [Header("Calidad")]
    [SerializeField, Range(0.5f, 2f)] private float _resolutionScale = 1f;
    private RawImage _display;
    private RenderTexture _rt;
    private Transform _active;
    private Quaternion[] _startRotations;
    private bool _dragging;
    private float _idleTimer;
    private void Awake()
    {
        _display = GetComponent<RawImage>();
        _startRotations = new Quaternion[_models.Length];
        for (int i = 0; i < _models.Length; i++)
        {
            if (_models[i] == null) continue;
            _startRotations[i] = _models[i].localRotation;
            _models[i].gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        CreateRenderTexture();
        ShowModel(0);
    }
    private void CreateRenderTexture()
    {
        Rect r = ((RectTransform)transform).rect;
        float scale = _resolutionScale * (Application.isMobilePlatform ? 1f : Mathf.Min(2f, Screen.dpi > 0 ? Screen.dpi / 96f : 1f));
        int w = Mathf.Max(64, Mathf.RoundToInt(r.width * scale));
        int h = Mathf.Max(64, Mathf.RoundToInt(r.height * scale));

        _rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
        _viewCamera.targetTexture = _rt;
        _viewCamera.aspect = (float)w / h;
        _display.texture = _rt;
    }
    // Llamalo cuando cambia el paso: ShowModel(indiceDelPaso). Con -1 (o índice sin modelo) se oculta.
    public void ShowModel(int index)
    {
        if (_active != null) _active.gameObject.SetActive(false);
        _active = null;

        bool valid = index >= 0 && index < _models.Length && _models[index] != null;
        _display.enabled = valid;
        _viewCamera.enabled = valid;
        if (!valid) return;

        _active = _models[index];
        _active.localRotation = _startRotations[index];
        _active.gameObject.SetActive(true);
        _idleTimer = 0f;
    }
    private void Update()
    {
        if (_active == null || _dragging || _autoRotateSpeed <= 0f) return;

        _idleTimer += Time.unscaledDeltaTime;
        if (_idleTimer >= _idleTime)
            _active.Rotate(Vector3.up, _autoRotateSpeed * Time.unscaledDeltaTime, Space.World);
    }
    public void OnBeginDrag(PointerEventData e) { _dragging = true; }
    public void OnDrag(PointerEventData e)
    {
        if (_active == null) return;
        _active.Rotate(Vector3.up, -e.delta.x * _rotateSpeed, Space.World);
        _active.Rotate(_viewCamera.transform.right, e.delta.y * _rotateSpeed, Space.World);
    }
    public void OnEndDrag(PointerEventData e) { _dragging = false; _idleTimer = 0f; }
    public void OnScroll(PointerEventData e)
    {
        if (!_allowZoom) return;
        _viewCamera.fieldOfView = Mathf.Clamp(_viewCamera.fieldOfView - e.scrollDelta.y * 2f, _minFov, _maxFov);
    }
    private void OnDestroy()
    {
        if (_rt != null) { _viewCamera.targetTexture = null; _rt.Release(); Destroy(_rt); }
    }
}