using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Highlightable))]
public class ServePoint : MonoBehaviour, IClickable
{
    [SerializeField] private Drink _order = Drink.Espresso;
    [SerializeField] private UnityEvent _onDelivered;
    [SerializeField] private UnityEvent _onWrongDrink;

    public Drink GetCurrentOrder => _order;

    // Para cambiar el pedido por código o desde un botón (0 = Espresso, 1 = Americano, 2 = Latte, 3 = Cappuccino)
    public void SetOrder(int drinkIndex) => _order = (Drink)drinkIndex;

    public void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var cup = hand.GetHeld as Cup;

        if (cup == null) { hand.Say("Entregá una taza con la bebida preparada."); return; }
        if (cup.GetIsEmpty) { hand.Say("La taza está vacía."); return; }

        if (cup.Matches(_order))
        {
            hand.ReleaseHeld();
            cup.ResetState();
            cup.ReturnHome();
            GameEvents.Raise(Feedback.Delivered, transform);
            GameEvents.CompleteStep(GameStep.Deliver);
            hand.Say("¡Pedido entregado: " + _order + "!");
            _onDelivered?.Invoke();
        }
        else
        {
            GameEvents.Raise(Feedback.WrongDrink, transform);
            hand.Say("Eso no es un " + _order + ". Revisá los ingredientes o descartalo.");
            _onWrongDrink?.Invoke();
        }
    }
}