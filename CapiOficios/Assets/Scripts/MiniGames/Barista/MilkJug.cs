using UnityEngine;

public enum JugContent { None, Milk, Chocolate }
public class MilkJug : Carryable
{
    [Header("Indicadores (los armás vos)")]
    [SerializeField] private GameObject _milkVisual;
    [SerializeField] private GameObject _hotVisual;

    public bool GetHasMilk { get; private set; }
    public bool GetHot { get; private set; }
    public bool GetIsEmpty => !GetHasMilk && !GetHot;

    private void Start() => Refresh();

    public void SetHot()
    {
        if (!GetHasMilk) return;
        GetHot = true;
        Refresh();
    }

    public override void ResetState()
    {
        GetHasMilk = false;
        GetHot = false;
        Refresh();
    }

    private void Refresh()
    {
        if (_milkVisual != null) _milkVisual.SetActive(GetHasMilk);
        if (_hotVisual != null) _hotVisual.SetActive(GetHot);
    }

    public override void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;
        if (held != null && held.Type == ItemType.MilkCarton) { Fill(hand, held); return; }
        base.OnClicked();
    }

    private void Fill(PlayerHand hand, Carryable carton)
    {
        if (GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }
        if (GetHasMilk) { hand.Say("La jarra ya tiene leche."); return; }

        hand.ReleaseHeld();
        GetBusy = true;
        StartCoroutine(PourHelper.Pour(carton, transform,
            onPoured: () =>
            {
                GetHasMilk = true;
                GetHot = false;
                Refresh();
                GameEvents.Raise(Feedback.Fill, transform);
                GameEvents.CompleteStep(GameStep.FillJug);
                hand.Say("Jarra cargada con leche.");
            },
            onDone: () => GetBusy = false));
    }
}