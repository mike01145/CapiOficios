using UnityEngine;

[RequireComponent(typeof(Highlightable))]
public class ItemSlot : MonoBehaviour, IClickable
{
    [Tooltip("Punto donde queda apoyado el objeto. Vacio = el centro de este objeto.")]
    [SerializeField] private Transform _snapPoint;
    [Tooltip("Que tipos acepta. Vacio = acepta todos.")]
    [SerializeField] private ItemType[] _accepts;

    public Carryable GetPlaced { get; private set; }
    public Transform GetSnap => _snapPoint != null ? _snapPoint : transform;

    protected virtual void Update()
    {
        // Si alguien agarro lo que estaba apoyado, el slot queda libre
        if (GetPlaced != null && GetPlaced.GetIsHeld)
        {
            var old = GetPlaced;
            GetPlaced = null;
            OnRemoved(old);
        }
    }

    public virtual void OnClicked()
    {
        var held = PlayerHand.Instance.GetHeld;
        if (held != null) TryPlace(held);
        else OnUsedEmptyHanded();
    }

    public bool TryPlace(Carryable held)
    {
        var hand = PlayerHand.Instance;
        if (GetPlaced != null) { hand.Say("Ya hay algo ahi."); return false; }
        if (!Accepts(held, out string why)) { hand.Say(why); return false; }

        hand.ReleaseHeld();
        GetPlaced = held;
        held.transform.SetPositionAndRotation(GetSnap.position, GetSnap.rotation);
        held.SetColliders(true);
        GameEvents.Raise(Feedback.Place, GetSnap);
        OnPlaced(held);
        return true;
    }

    protected virtual bool Accepts(Carryable carryable, out string why)
    {
        why = "Eso no va ahi.";
        if (_accepts == null || _accepts.Length == 0) return true;
        foreach (var t in _accepts) if (t == carryable.Type) return true;
        return false;
    }

    protected virtual void OnPlaced(Carryable carryable) { }
    protected virtual void OnRemoved(Carryable carryable) { }
    protected virtual void OnUsedEmptyHanded() { }   // click con la mano vacía = "usar" la estación
}