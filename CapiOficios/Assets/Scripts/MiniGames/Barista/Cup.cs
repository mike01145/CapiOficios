using UnityEngine;

public enum Drink { Espresso, Americano, Latte, Cappuccino }
public enum MilkQuality { None, Cold, Good, Burnt }

public class Cup : Carryable
{
    [Header("Visuales opcionales")]
    [SerializeField] private GameObject _espressoVisual;
    [SerializeField] private GameObject _waterVisual;
    [SerializeField] private GameObject _milkVisual;
    [SerializeField] private GameObject _foamVisual;

    public bool GetHasEspresso { get; private set; }
    public bool GetHasWater { get; private set; }
    public bool GetFoam { get; private set; }
    public MilkQuality GetMilk { get; private set; }
    public bool GetIsEmpty => !GetHasEspresso && !GetHasWater && GetMilk == MilkQuality.None;

    private void Start() => Refresh();

    public void AddEspresso() { GetHasEspresso = true; Refresh(); }
    public void AddWater() { GetHasWater = true; Refresh(); }
    public void AddMilk(MilkQuality quality, bool foam) { GetMilk = quality; GetFoam = foam; Refresh(); }

    public override void ResetState()
    {
        GetHasEspresso = false;
        GetHasWater = false;
        GetFoam = false;
        GetMilk = MilkQuality.None;
        Refresh();
    }

    public bool Matches(Drink d)
    {
        return d switch
        {
            Drink.Espresso => GetHasEspresso && !GetHasWater && GetMilk == MilkQuality.None,
            Drink.Americano => GetHasEspresso && GetHasWater && GetMilk == MilkQuality.None,
            Drink.Latte => GetHasEspresso && !GetHasWater && GetMilk == MilkQuality.Good && !GetFoam,
            Drink.Cappuccino => GetHasEspresso && !GetHasWater && GetMilk == MilkQuality.Good && GetFoam,
            _ => false,
        };
    }

    private void Refresh()
    {
        if (_espressoVisual != null) _espressoVisual.SetActive(GetHasEspresso);
        if (_waterVisual != null) _waterVisual.SetActive(GetHasWater);
        if (_milkVisual != null) _milkVisual.SetActive(GetMilk != MilkQuality.None);
        if (_foamVisual != null) _foamVisual.SetActive(GetFoam && GetMilk != MilkQuality.None);
    }

    public override void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var jug = hand.GetHeld as MilkJug;
        if (jug != null) { Pour(hand, jug); return; }
        base.OnClicked();
    }

    // Con la jarra en la mano, click en la taza = verter la leche
    private void Pour(PlayerHand hand, MilkJug jug)
    {
        if (GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }

        if (jug.GetMilk == MilkQuality.None) { hand.Say("La jarra está vacía."); return; }
        if (GetMilk != MilkQuality.None) { hand.Say("La taza ya tiene leche."); return; }

        MilkQuality q = jug.GetMilk;
        AddMilk(q, jug.GetFoamed);
        jug.ResetState();
        GameEvents.Raise(Feedback.Pour, transform);

        if (q == MilkQuality.Good)
        {
            GameEvents.CompleteStep(GameStep.PourMilk);
            hand.Say("Leche vertida.");
        }
        else if (q == MilkQuality.Cold) hand.Say("Vertiste leche fría. Quizás convenga descartar la taza.");
        else hand.Say("Vertiste leche quemada. Conviene descartar la taza.");
    }
}