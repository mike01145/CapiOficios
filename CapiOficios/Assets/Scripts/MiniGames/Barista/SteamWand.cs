using System.Collections;
using UnityEngine;
public class SteamWand : ItemSlot
{
    [SerializeField] private ParticleSystem _steamFx;   // opcional
    [SerializeField] private float _steamSeconds = 1f;

    protected override bool Accepts(Carryable c, out string why)
    {
        if (!base.Accepts(c, out why)) return false;
        var jug = c as MilkJug;
        if (jug == null) { why = "Ahí va la jarra."; return false; }
        if (jug.GetIsEmpty) { why = "La jarra está vacía. Cargala primero."; return false; }
        return true;
    }

    protected override void OnUsedEmptyHanded()
    {
        var jug = GetPlaced as MilkJug;
        if (jug == null) { PlayerHand.Instance.Warn("Poné la jarra bajo la varilla de vapor.", transform); return; }
        if (jug.GetHot) { PlayerHand.Instance.Say("Ya está caliente."); return; }

        jug.SetHot();
        StartCoroutine(SteamRoutine());
        GameEvents.CompleteStep(GameStep.HeatMilk);
        PlayerHand.Instance.Say("¡Listo, bien caliente!");
    }

    private IEnumerator SteamRoutine()
    {
        GameEvents.Raise(Feedback.SteamStart, transform);
        if (_steamFx != null) _steamFx.Play();
        yield return new WaitForSecondsRealtime(_steamSeconds);
        if (_steamFx != null) _steamFx.Stop();
        GameEvents.Raise(Feedback.SteamEnd, transform);
    }
}