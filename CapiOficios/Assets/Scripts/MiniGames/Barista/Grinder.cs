using System.Collections;
using UnityEngine;

public class Grinder : ItemSlot
{
    [SerializeField] private float _grindSeconds = 3f;
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
        hand.Say("Granos cargados en el molinillo.");
    }

    protected override bool Accepts(Carryable c, out string why)
    {
        if (!base.Accepts(c, out why)) return false;
        var pf = c as Portafilter;
        if (pf == null) { why = "Ahí va el portafiltro."; return false; }
        if (pf.GetCurrentState != PortafilterState.Empty) { why = "El portafiltro ya tiene café. Descartalo primero."; return false; }
        return true;
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

    private IEnumerator Grind(Portafilter pf)
    {
        _grinding = true;
        pf.GetBusy = true;

        for (float t = 0f; t < _grindSeconds; t += Time.deltaTime)
        {
            float p = t / _grindSeconds;
            ProgressHUD.Display("Moliendo café", p, Mathf.RoundToInt(p * 100f) + "%");
            yield return null;
        }

        ProgressHUD.Clear();
        pf.SetState(PortafilterState.Ground);
        pf.GetBusy = false;
        GetHasBeans = false;
        if (_beansInHopper != null) _beansInHopper.SetActive(false);
        _grinding = false;
        PlayerHand.Instance.Say("¡Café molido!");
    }
}