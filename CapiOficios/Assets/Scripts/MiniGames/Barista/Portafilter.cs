using UnityEngine;
public enum PortafilterState { Empty, Ground, Tamped, Locked, Spent }
public class Portafilter : Carryable
{
    [Header("Visuales opcionales")]
    [SerializeField] private GameObject _looseCoffee;   // café molido suelto
    [SerializeField] private GameObject _tampedCoffee;  // café compactado

    public PortafilterState GetCurrentState { get; private set; }

    private void Start() => Refresh();

    public void SetState(PortafilterState s) { GetCurrentState = s; Refresh(); }
    public override void ResetState() => SetState(PortafilterState.Empty);

    private void Refresh()
    {
        if (_looseCoffee != null) _looseCoffee.SetActive(GetCurrentState == PortafilterState.Ground);
        if (_tampedCoffee != null)
            _tampedCoffee.SetActive(GetCurrentState == PortafilterState.Tamped
            || GetCurrentState == PortafilterState.Locked
            || GetCurrentState == PortafilterState.Spent);
    }

    public override void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;
        if (held != null && held.Type == ItemType.Tamper) { Tamp(hand, held); return; }
        base.OnClicked();
    }

    private void Tamp(PlayerHand hand, Carryable tamper)
    {
        if (GetBusy) { hand.Say("Está en uso, esperá a que termine."); return; }

        switch (GetCurrentState)
        {
            case PortafilterState.Empty:
                hand.Say("No hay café molido para compactar.");
                break;
            case PortafilterState.Ground:
                SetState(PortafilterState.Tamped);
                hand.ReleaseHeld();
                tamper.ReturnHome();   // el tamper vuelve solo a su lugar
                GameEvents.Raise(Feedback.Tamp, transform);
                GameEvents.CompleteStep(GameStep.Tamp);
                hand.Say("¡Café compactado y nivelado!");
                break;
            default:
                hand.Say("El café ya está compactado.");
                break;
        }
    }
}