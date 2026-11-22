using UnityEngine;

public enum Drink { Espresso, Latte, Mocha }

public class Cup : Carryable
{
    [Header("Indicadores (los armás vos)")]
    [SerializeField] private GameObject _espressoVisual;
    [SerializeField] private GameObject _milkVisual;
    [SerializeField] private GameObject _chocolateVisual;

    public bool GetHasEspresso { get; private set; }
    public bool GetHasMilk { get; private set; }
    public bool GetHasChocolate { get; private set; }
    public bool GetIsEmpty => !GetHasEspresso && !GetHasMilk && !GetHasChocolate;

    private void Start() => Refresh();

    public void AddEspresso() { GetHasEspresso = true; Refresh(); }

    public override void ResetState()
    {
        GetHasEspresso = false;
        GetHasMilk = false;
        GetHasChocolate = false;
        Refresh();
    }

    public bool Matches(Drink d)
    {
        return d switch
        {
            Drink.Espresso => GetHasEspresso && !GetHasMilk && !GetHasChocolate,
            Drink.Latte => GetHasEspresso && GetHasMilk && !GetHasChocolate,
            Drink.Mocha => GetHasEspresso && GetHasMilk && GetHasChocolate,
            _ => false,
        };
    }

    private void Refresh()
    {
        if (_espressoVisual != null) _espressoVisual.SetActive(GetHasEspresso);
        if (_milkVisual != null) _milkVisual.SetActive(GetHasMilk);
        if (_chocolateVisual != null) _chocolateVisual.SetActive(GetHasChocolate);
    }

    public override void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;
        if (held is MilkJug jug) { PourMilk(hand, jug); return; }
        if (held != null && held.Type == ItemType.ChocolateBox) { AddChocolate(hand, held); return; }
        base.OnClicked();
    }

    private void PourMilk(PlayerHand hand, MilkJug jug)
    {
        if (GetBusy || jug.GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }
        if (!jug.GetHasMilk) { hand.Say("La jarra está vacía."); return; }
        if (!jug.GetHot) { hand.Say("Primero calentá la jarra tocando la varilla de vapor."); return; }
        if (GetHasMilk) { hand.Say("La taza ya tiene leche."); return; }

        hand.ReleaseHeld();
        GetBusy = true;
        jug.GetBusy = true;
        StartCoroutine(PourHelper.Pour(jug, transform,
            onPoured: () =>
            {
                GetHasMilk = true;
                Refresh();
                jug.ResetState();
                GameEvents.Raise(Feedback.Pour, transform);
                GameEvents.CompleteStep(GameStep.PourJug);
                hand.Say("Vertiste la leche en la taza.");
            },
            onDone: () => { GetBusy = false; jug.GetBusy = false; }));
    }

    private void AddChocolate(PlayerHand hand, Carryable box)
    {
        if (GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }
        if (!GetHasMilk) { hand.Say("El chocolate va al final, sobre la leche."); return; }
        if (GetHasChocolate) { hand.Say("La taza ya tiene chocolate."); return; }

        hand.ReleaseHeld();
        GetBusy = true;
        StartCoroutine(PourHelper.Pour(box, transform,
            onPoured: () =>
            {
                GetHasChocolate = true;
                Refresh();
                GameEvents.Raise(Feedback.Pour, transform);
                GameEvents.CompleteStep(GameStep.AddChocolate);
                hand.Say("Agregaste chocolate.");
            },
            onDone: () => GetBusy = false));
    }
}