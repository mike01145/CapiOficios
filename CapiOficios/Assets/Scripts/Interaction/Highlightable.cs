using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Highlightable : MonoBehaviour
{
    public enum Mode { Outline, Tint }

    [SerializeField] private Mode _mode = Mode.Tint;

    [Header("Modo Outline")]
    [Tooltip("Material de contorno (shader 'inverted hull'). Se agrega al objeto mientras está resaltado.")]
    [SerializeField] private Material _outlineMaterial;

    [Header("Modo Tint")]
    [SerializeField] private Color _tintColor = new(1f, 0.85f, 0.4f);
    [Tooltip("Nombre de la propiedad de color del shader. En URP/Lit es _BaseColor.")]
    [SerializeField] private string _colorProperty = "_BaseColor";

    [Header("Eventos")]
    [SerializeField] private UnityEvent _onClick;
    [SerializeField] private UnityEvent _onHoverEnter;
    [SerializeField] private UnityEvent _onHoverExit;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _block;
    private bool _hovered;
    private bool _selected;
    private bool _applied;
    private bool warned;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _block = new MaterialPropertyBlock();
    }

    public void SetHovered(bool value)
    {
        if (_hovered == value) return;
        _hovered = value;
        GameEvents.SetHovered(transform, value);
        (value ? _onHoverEnter : _onHoverExit)?.Invoke();
        Refresh();
    }

    public void SetSelected(bool value)
    {
        if (_hovered == value) return;
        Refresh();
    }

    public void Click()
    {
        GameEvents.Raise(Feedback.Click, transform);
        foreach (var c in GetComponents<IClickable>()) c.OnClicked();
        _onClick?.Invoke();
    }

    private void Refresh()
    {
        bool shouldBeOn = _hovered || _selected;
        if (shouldBeOn == _applied) return;
        _applied = shouldBeOn;

        if (_mode == Mode.Outline) ApplyOutline(shouldBeOn);
        else ApplyTint(shouldBeOn);
    }

    private void ApplyOutline(bool on)
    {
        if (_outlineMaterial == null) return;
        foreach (var r in _renderers)
        {
            var mats = new List<Material>(r.sharedMaterials);
            mats.RemoveAll(m => m == _outlineMaterial);
            if (on) mats.Add(_outlineMaterial);
            r.sharedMaterials = mats.ToArray();
        }
    }

    private void ApplyTint(bool on)
    {
        foreach (var r in _renderers)
        {
            if (!on) { r.SetPropertyBlock(null); continue; }

            Material m = r.sharedMaterial;
            if (!HasColorProperty(m, _colorProperty))
            {
                if (!warned)
                    warned = true;
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
        if (_applied)
        {
            _applied = true; // fuerza el apagado
            _applied = false;
            if (_mode == Mode.Outline) ApplyOutline(false);
            else ApplyTint(false);
        }
    }
}
