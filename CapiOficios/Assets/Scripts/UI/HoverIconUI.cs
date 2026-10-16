using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Muestra el ícono (HoverIcon) del objeto sobre el que está el mouse, siguiéndolo en pantalla.
public class HoverIconUI : MonoBehaviour
{
    [SerializeField] private RectTransform _iconRoot;   // lo que se muestra y se mueve (con Image y/o texto adentro)
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _labelText;       // opcional
    [SerializeField] private Camera _cam;                // vacío = Main Camera

    private Canvas _rootCanvas;
    private RectTransform _canvasRect;
    private HoverIcon _current;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        _canvasRect = (RectTransform)_rootCanvas.transform;
        if (_cam == null) _cam = Camera.main;
        _iconRoot.gameObject.SetActive(false);
    }

    private void OnEnable() => GameEvents.OnHovered += HoverIconUI_OnHovered;
    private void OnDisable() => GameEvents.OnHovered -= HoverIconUI_OnHovered;

    private void HoverIconUI_OnHovered(object sender, GameEvents.HoveredEventArgs e)
    {
        if (!e.On)
        {
            // Solo oculta si el que salió es el que estábamos mostrando
            if (_current != null && e.Target.GetComponentInParent<HoverIcon>() == _current) Hide();
            return;
        }

        var icon = e.Target.GetComponentInParent<HoverIcon>();
        if (icon == null) { Hide(); return; }

        _current = icon;
        _iconImage.sprite = icon.GetIcon;
        _iconImage.enabled = icon.GetIcon != null;
        if (_labelText != null) _labelText.text = icon.GetLabel;
        _iconRoot.gameObject.SetActive(true);
        Reposition();
    }

    private void Hide()
    {
        _current = null;
        _iconRoot.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_current != null) Reposition();
    }

    private void Reposition()
    {
        Vector3 world = _current.transform.position + _current.GetWorldOffset;
        Vector3 screen = _cam.WorldToScreenPoint(world);
        if (screen.z < 0f) { _iconRoot.gameObject.SetActive(false); return; }

        _iconRoot.gameObject.SetActive(true);
        Camera uiCam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, uiCam, out Vector2 local);
        _iconRoot.position = _canvasRect.TransformPoint(local);
    }
}