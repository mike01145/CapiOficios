using UnityEngine;

[RequireComponent(typeof(Highlightable))]
public class TrashCan : MonoBehaviour, IClickable
{
    public void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var item = hand.GetHeld;

        if (item == null) { hand.Say("No tenés nada para descartar."); return; }

        item.ResetState();
        hand.ReleaseHeld();
        item.ReturnHome();
        hand.Say("Descartado. Volvió a su lugar, vacío.");
    }
}