using UnityEngine;

public enum DispenserKind { Beans, Milk, Water }

// Frasco de granos, caja de leche o canilla de agua caliente
[RequireComponent(typeof(Highlightable))]
public class Dispenser : MonoBehaviour, IClickable
{
    [SerializeField] private DispenserKind _kind;

    [Header("Solo para Beans")]
    [Tooltip("Objeto que se agarra al tocar el frasco (con Carryable: Type = Beans y Destroy On Return). Vacío = se crea una esfera marrón.")]
    [SerializeField] private Carryable _scoopPrefab;
    [SerializeField] private float _fallbackScale = 0.12f;

    public void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;

        switch (_kind)
        {
            case DispenserKind.Beans:
                {
                    if (held != null) { hand.Say("Tenés las manos ocupadas."); return; }
                    Carryable scoop = _scoopPrefab != null
                        ? Instantiate(_scoopPrefab, transform.position, Quaternion.identity)
                        : CreateFallbackScoop();
                    hand.Pick(scoop);
                    hand.Say("Tomaste granos de café. Llevalos al molinillo.");
                    break;
                }
            case DispenserKind.Milk:
                {
                    var jug = held as MilkJug;
                    if (jug == null) hand.Say("Necesitás la jarra para cargar leche.");
                    else if (jug.GetMilk != MilkQuality.None) hand.Say("La jarra ya tiene leche.");
                    else { jug.Fill(); hand.Say("Jarra cargada con leche."); }
                    break;
                }
            case DispenserKind.Water:
                {
                    var cup = held as Cup;
                    if (cup == null) hand.Say("Necesitás una taza para agregar agua caliente.");
                    else if (cup.GetHasWater) hand.Say("La taza ya tiene agua.");
                    else { cup.AddWater(); hand.Say("Agregaste agua caliente."); }
                    break;
                }
        }
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