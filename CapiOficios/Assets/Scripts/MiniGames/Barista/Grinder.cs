using System.Collections;
using UnityEngine;

public class Grinder : ItemSlot
{
    [SerializeField] private float _grindSeconds = 3f;
    [SerializeField] private bool _autoGrindOnPlace = true;
    [SerializeField] private GameObject _beansInHopper;   // opcional: granos visibles en la tolva

    public bool GetHasBeans { get; private set; }
    private bool _grinding;

    private void Start()
    {
        if (_beansInHopper != null) _beansInHopper.SetActive(false);
    }

    public override void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;
        if (held != null && held.Type == ItemType.Beans) { LoadBeans(hand); return; }
        base.OnClicked();
    }
    private void LoadBeans(PlayerHand hand)
    {
        Debug.Log("Granos cargados. HasBeans = " + GetHasBeans);
        if (GetHasBeans) { hand.Say("La tolva ya tiene granos."); return; }
        hand.Consume();
        GetHasBeans = true;
        if (_beansInHopper != null) _beansInHopper.SetActive(true);
        GameEvents.Raise(Feedback.BeansLoaded, transform);
        GameEvents.CompleteStep(GameStep.LoadBeans);
        hand.Say("Granos cargados en el molinillo.");
    }
    public void OnHopperClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;

        if (held != null && held.Type == ItemType.Beans) { LoadBeans(hand); return; }
        if (held != null) { hand.Say("En la tolva solo se cargan granos."); return; }

        hand.Say(GetHasBeans
            ? "La tolva ya tiene granos. Poné el portafiltro abajo para moler."
            : "Traé granos del frasco para cargar la tolva.");
    }

    protected override bool Accepts(Carryable c, out string why)
    {
        if (!base.Accepts(c, out why)) return false;
        var pf = c as Portafilter;
        if (pf == null) { why = "Ahí va el portafiltro."; return false; }
        if (pf.GetCurrentState != PortafilterState.Empty)
        {
            why = "El portafiltro ya tiene café. Descartalo primero."; return false;
        }
        return true;
    }
    protected override void OnPlaced(Carryable c)
    {
        var pf = c as Portafilter;
        if (pf == null) return;

        if (!_autoGrindOnPlace) return;

        if (GetHasBeans) StartGrind(pf);
        else PlayerHand.Instance.Say("Portafiltro colocado. Falta cargar granos en la tolva.");
    }
    protected override void OnUsedEmptyHanded()
    {
        var hand = PlayerHand.Instance;
        if (_grinding) return;

        var pf = GetPlaced as Portafilter;
        if (!GetHasBeans) hand.Say("Falta cargar granos en la tolva.");
        else if (pf == null) hand.Say("Poné el portafiltro debajo de la salida.");
        else StartCoroutine(Grind(pf));
    }
    private void StartGrind(Portafilter pf)
    {
        if (_grinding || pf == null) return;
        StartCoroutine(Grind(pf));
    }
    private IEnumerator Grind(Portafilter pf)
    {
        _grinding = true;
        pf.GetBusy = true;
        GameEvents.Raise(Feedback.GrindStart, transform);

        for (float t = 0f; t < _grindSeconds; t += Time.deltaTime)
        {
            float p = t / _grindSeconds;
            GameEvents.ShowProgress(transform, "Moliendo café", p, Mathf.RoundToInt(p * 100f) + "%");
            yield return null;
        }

        GameEvents.ClearProgress(transform);
        pf.SetState(PortafilterState.Ground);
        pf.GetBusy = false;
        GetHasBeans = false;
        if (_beansInHopper != null) _beansInHopper.SetActive(false);
        _grinding = false;

        GameEvents.Raise(Feedback.GrindEnd, transform);
        GameEvents.CompleteStep(GameStep.Grind);
        PlayerHand.Instance.Say("¡Café molido!");
    }
}