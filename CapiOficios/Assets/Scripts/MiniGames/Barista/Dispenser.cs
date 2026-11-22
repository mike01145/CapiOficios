using UnityEngine;

public enum DispenserKind { Beans, Milk, Water }

// Frasco de granos, caja de leche o canilla de agua caliente
[RequireComponent(typeof(Highlightable))]
public class Dispenser : MonoBehaviour, IClickable
{
    [Tooltip("Objeto que se agarra al tocar el frasco (con Carryable: Type = Beans y Destroy On Return). Vacío = se crea una esfera marrón.")]
    [SerializeField] private Carryable _scoopPrefab;
    [SerializeField] private float _fallbackScale = 0.12f;

    public void OnClicked()
    {
        var hand = PlayerHand.Instance;

        if (hand.GetHeld != null) { hand.Say("Tenés las manos ocupadas."); return; }

        Carryable scoop = _scoopPrefab != null
            ? Instantiate(_scoopPrefab, transform.position, Quaternion.identity)
            : CreateFallbackScoop();
        hand.Pick(scoop);
        hand.Say("Tomaste granos de café. Llevalos al molinillo.");
    }

    private Carryable CreateFallbackScoop()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Granos";
        go.layer = gameObject.layer;
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * _fallbackScale;
        go.GetComponent<Renderer>().material.color = new Color(0.35f, 0.2f, 0.1f);
        var c = go.AddComponent<Carryable>();
        c.Configure(ItemType.Beans, true);
        return c;
    }
}