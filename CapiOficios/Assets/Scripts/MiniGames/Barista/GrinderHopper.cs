using UnityEngine;

[RequireComponent(typeof(Highlightable))]
public class GrinderHopper : MonoBehaviour, IClickable
{
    [SerializeField] private Grinder _grinder;   // vacío = se busca en los padres

    private void Awake()
    {
        if (_grinder == null) _grinder = GetComponentInParent<Grinder>();
    }

    public void OnClicked()
    {
        if (_grinder != null) _grinder.OnHopperClicked();
    }
}