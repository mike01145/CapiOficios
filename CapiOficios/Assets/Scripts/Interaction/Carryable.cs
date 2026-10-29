using UnityEngine;

public enum ItemType { Beans, Portafilter, Tamper, Cup, MilkJug }

[RequireComponent(typeof(Highlightable))]
public class Carryable : MonoBehaviour, IClickable
{
    [SerializeField] private ItemType _type;
    [Tooltip("Para lo que sale de un dispenser (los granos): al soltarlo desaparece en vez de volver.")]
    [SerializeField] private bool _destroyOnReturn;

    public ItemType Type => _type;
    public bool GetBusy { get; set; }   // true mientras una máquina lo está usando
    public bool GetIsHeld => PlayerHand.Instance != null && PlayerHand.Instance.GetHeld == this;

    private Transform _homeParent;
    private Vector3 _homeLocalPos;
    private Quaternion _homeLocalRot;
    private Collider[] _colliders;

    private void Awake()
    {
        _homeParent = transform.parent;
        _homeLocalPos = transform.localPosition;
        _homeLocalRot = transform.localRotation;
        _colliders = GetComponentsInChildren<Collider>();
    }

    public void Configure(ItemType type, bool destroy) { _type = type; _destroyOnReturn = destroy; }

    public void SetColliders(bool on)
    {
        foreach (var c in _colliders) if (c != null) c.enabled = on;
    }

    public virtual void OnPickedUp()
    {
        SetColliders(false); // para que no tape el raycast mientras lo llevas
        transform.rotation = _homeParent != null ? _homeParent.rotation * _homeLocalRot : _homeLocalRot;
    }

    // Lo llama el tacho: deja el objeto vacío / limpio
    public virtual void ResetState() { }

    public virtual void OnClicked()
    {
        Debug.Log($"OnClicked {name} | tipo {GetType().Name} | Held: {PlayerHand.Instance.GetHeld} | Busy: {GetBusy}");
        var hand = PlayerHand.Instance;
        if (GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }
        if (hand.GetHeld == null) hand.Pick(this);
        else hand.Say("Tenés las manos ocupadas.");
    }

    public void ReturnHome()
    {
        if (_destroyOnReturn) { Destroy(gameObject); return; }
        transform.SetParent(_homeParent, false);
        transform.SetLocalPositionAndRotation(_homeLocalPos, _homeLocalRot);
        SetColliders(true);
    }
}