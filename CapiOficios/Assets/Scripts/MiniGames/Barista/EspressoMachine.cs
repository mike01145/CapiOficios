using System.Collections;
using UnityEngine;

public class EspressoMachine : MonoBehaviour, IClickable
{
    [SerializeField] private GroupHead _groupHead;
    [SerializeField] private ItemSlot _cupSlot;          // lugar bajo la salida (acepta Cup)
    [SerializeField] private SteamWand _steamWand;       // opcional
    [SerializeField] private float _extractSeconds = 8f;
    [SerializeField] private GameObject _coffeeStream;   // opcional: chorrito de café

    private bool _busy;

    private void Start()
    {
        if (_coffeeStream != null) _coffeeStream.SetActive(false);
    }

    public void OnClicked()
    {
        var hand = PlayerHand.Instance;
        var held = hand.GetHeld;

        if (held == null) { hand.Say("Usá el botón de preparar para extraer el café."); return; }

        switch (held.Type)
        {
            case ItemType.Cup: _cupSlot.TryPlace(held); break;
            case ItemType.Portafilter: _groupHead.TryPlace(held); break;
            case ItemType.MilkJug:
                if (_steamWand != null) _steamWand.TryPlace(held);
                else hand.Say("Eso no va en la máquina.");
                break;
            default: hand.Say("Eso no va en la máquina."); break;
        }
    }

    // Conectalo al botón (UnityEvent del Highlightable del botón, o un Button de UI)
    public void StartExtraction()
    {
        if (_busy) return;
        var hand = PlayerHand.Instance;

        var pf = _groupHead.GetPlaced as Portafilter;
        var cup = _cupSlot.GetPlaced as Cup;

        if (pf == null || pf.GetCurrentState != PortafilterState.Locked)
        {
            hand.Say("Falta el portafiltro preparado (con el café compactado) en el cabezal.");
            return;
        }
        if (cup == null) { hand.Say("Poné una taza debajo de la salida."); return; }
        if (cup.GetHasEspresso) { hand.Say("Esa taza ya tiene espresso."); return; }

        StartCoroutine(Extract(pf, cup));
    }

    private IEnumerator Extract(Portafilter pf, Cup cup)
    {
        _busy = true;
        pf.GetBusy = true;
        cup.GetBusy = true;
        GameEvents.Raise(Feedback.ExtractStart, transform);
        if (_coffeeStream != null) _coffeeStream.SetActive(true);

        for (float t = 0f; t < _extractSeconds; t += Time.deltaTime)
        {
            float p = t / _extractSeconds;
            GameEvents.ShowProgress(transform, "Extrayendo espresso", p, Mathf.RoundToInt(p * 100f) + "%");
            yield return null;
        }

        GameEvents.ClearProgress(transform);
        if (_coffeeStream != null) _coffeeStream.SetActive(false);

        cup.AddEspresso();
        pf.SetState(PortafilterState.Spent);
        pf.GetBusy = false;
        cup.GetBusy = false;
        _busy = false;

        GameEvents.Raise(Feedback.ExtractEnd, transform);
        GameEvents.CompleteStep(GameStep.Extract);
        PlayerHand.Instance.Say("¡Espresso listo!");
    }
}