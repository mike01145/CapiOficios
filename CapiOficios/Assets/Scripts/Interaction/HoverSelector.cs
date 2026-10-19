using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HoverSelector : MonoBehaviour
{
    [SerializeField] private Camera _cam;                 // vacío = Main Camera
    [Tooltip("Capas que se pueden resaltar. Conviene una capa 'Interactable' para que los objetos decorativos no bloqueen el rayo.")]
    [SerializeField] private LayerMask _layerMask = ~0;
    [SerializeField] private float _maxDistance = 100f;
    [SerializeField] private bool _ignoreWhenOverUI = true;
    [Tooltip("Si está activo, el objeto clickeado queda resaltado hasta que hagas click en otro o en el vacío.")]
    [SerializeField] private bool _keepSelected = false;

    private Highlightable _hovered;
    private Highlightable _selected;

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        Highlightable hit = null;

        bool overUI = _ignoreWhenOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!overUI)
        {
            Ray ray = _cam.ScreenPointToRay(PointerPosition());
            if (Physics.Raycast(ray, out RaycastHit info, _maxDistance, _layerMask))
                hit = info.collider.GetComponentInParent<Highlightable>();
        }

        // Hover
        if (hit != _hovered)
        {
            if (_hovered != null) _hovered.SetHovered(false);
            _hovered = hit;
            if (_hovered != null) _hovered.SetHovered(true);
        }

        // Click
        if (!overUI && Clicked())
        {
            if (hit != null)
            {
                if (_keepSelected)
                {
                    if (_selected != null) _selected.SetSelected(false);
                    _selected = hit;
                    _selected.SetSelected(true);
                }
                hit.Click();
            }
            else if (_keepSelected && _selected != null)
            {
                _selected.SetSelected(false);
                _selected = null;
            }
        }
    }

    private static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private static bool Clicked()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }
}