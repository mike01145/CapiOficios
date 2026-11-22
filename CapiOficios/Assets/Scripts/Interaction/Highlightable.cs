using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Highlightable : MonoBehaviour
{
    [Tooltip("Material de contorno (shader 'inverted hull'). Se agrega al objeto mientras está resaltado.")]
    [Header("Modo QuickOutline")]
    [SerializeField] private Outline _quickOutline;
    [SerializeField] private bool _highlightOnHover = false;

    [Header("Modo Tint")]
    [SerializeField] private Color _tintColor = new(1f, 0.85f, 0.4f);
    [Tooltip("Nombre de la propiedad de color del shader. En URP/Lit es _BaseColor.")]
    [SerializeField] private string _colorProperty = "_BaseColor";
    [SerializeField] private Color _guideColor = new(1f, .8f, .2f);

    [Header("Eventos")]
    [SerializeField] private UnityEvent _onClick;
    [SerializeField] private UnityEvent _onHoverEnter;
    [SerializeField] private UnityEvent _onHoverExit;

    public static Func<Highlightable, bool> ClickFilter;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _block;
    private bool _guided;
    private bool _hovered;
    private bool _selected;
    private bool _applied;
    private bool _warned;
    private bool _helped;

    private void Awake() => EnsureInit();
    private void Update()
    {
        if (!_guided) return;
        EnsureInit();
        float t = Mathf.PingPong(Time.unscaledTime * 2f, 1f) * 0.7f;
        foreach (var r in _renderers)
        {
            if (r == null) continue;
            Material m = r.sharedMaterial;
            if (!HasColorProperty(m, _colorProperty)) continue;
            Color o = m.GetColor(_colorProperty);
            r.GetPropertyBlock(_block);
            _block.SetColor(_colorProperty, Color.Lerp(o, _guideColor, t));
            r.SetPropertyBlock(_block);
        }
    }
    public void SetHovered(bool value)
    {
        if (_hovered == value) return;
        _hovered = value;
        GameEvents.SetHovered(transform, value);
        (value ? _onHoverEnter : _onHoverExit)?.Invoke();
        Refresh();
    }
    public void SetHelp(bool value)
    {
        EnsureInit();
        if (_helped == value) return;
        _helped = value;
        if (_quickOutline != null) _quickOutline.enabled = value;
    }
    private void EnsureInit()
    {
        if (_renderers != null) return;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _block = new MaterialPropertyBlock();
        if (_quickOutline != null) _quickOutline.enabled = false;
    }
    public void SetGuide(bool value)
    {
        EnsureInit();
        if (_guided == value) return;
        _guided = value;
        Refresh();
    }
    public void SetSelected(bool value)
    {
        if (_selected == value) return;
        _selected = value;
        Refresh();
    }

    public void Click()
    {
        if (ClickFilter != null && !ClickFilter(this)) return;
        GameEvents.Raise(Feedback.Click, transform);
        foreach (var c in GetComponents<IClickable>()) c.OnClicked();
        _onClick?.Invoke();
    }

    private void Refresh()
    {
        EnsureInit();
        bool shouldBeOn = (_hovered && _highlightOnHover) || _selected || _guided;
        if (shouldBeOn == _applied) return;
        _applied = shouldBeOn;
        ApplyTint(shouldBeOn);
    }
    private void ApplyTint(bool on)
    {
        foreach (var r in _renderers)
        {
            if (!on) { r.SetPropertyBlock(null); continue; }

            Material m = r.sharedMaterial;
            if (!HasColorProperty(m, _colorProperty))
            {
                if (!_warned)
                    _warned = true;
                continue;
            }

            Color original = m.GetColor(_colorProperty);
            r.GetPropertyBlock(_block);
            _block.SetColor(_colorProperty, original * _tintColor);
            r.SetPropertyBlock(_block);
        }
    }
    private static bool HasColorProperty(Material m, string prop)
    {
        if (m == null || m.shader == null) return false;
        Shader sh = m.shader;
        int index = sh.FindPropertyIndex(prop);
        return index >= 0 && sh.GetPropertyType(index) == UnityEngine.Rendering.ShaderPropertyType.Color;
    }

    private void OnDisable()
    {

        if (_hovered) GameEvents.SetHovered(transform, false);
        _hovered = false;
        _selected = false;
        _guided = false;
        _helped = false;
        if (_quickOutline != null) _quickOutline.enabled = false;
        if (_applied) { _applied = false; ApplyTint(false); }
    }
}
