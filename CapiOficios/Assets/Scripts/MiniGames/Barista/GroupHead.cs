using System.Collections;
using UnityEngine;

public class GroupHead : ItemSlot
{
    [SerializeField] private float lockAngle = 40f;
    [SerializeField] private float lockSeconds = 0.4f;

    protected override bool Accepts(Carryable c, out string why)
    {
        if (!base.Accepts(c, out why)) return false;
        var pf = c as Portafilter;
        if (pf == null) { why = "Ahí va el portafiltro."; return false; }

        switch (pf.GetCurrentState)
        {
            case PortafilterState.Tamped: return true;
            case PortafilterState.Empty: why = "El portafiltro está vacío."; break;
            case PortafilterState.Ground: why = "Falta compactar el café con el tamper."; break;
            default: why = "Ese café ya se usó. Descartalo en el tacho."; break;
        }
        return false;
    }

    protected override void OnPlaced(Carryable c)
    {
        var pf = c as Portafilter;
        if (pf != null) StartCoroutine(LockRoutine(pf));
    }

    protected override void OnRemoved(Carryable c)
    {
        var pf = c as Portafilter;
        if (pf != null && pf.GetCurrentState == PortafilterState.Locked)
            pf.SetState(PortafilterState.Tamped);
    }

    private IEnumerator LockRoutine(Portafilter pf)
    {
        pf.GetBusy = true;
        Quaternion from = pf.transform.rotation;
        Quaternion to = from * Quaternion.Euler(0f, lockAngle, 0f);

        for (float t = 0f; t < lockSeconds; t += Time.deltaTime)
        {
            pf.transform.rotation = Quaternion.Slerp(from, to, t / lockSeconds);
            yield return null;
        }

        pf.transform.rotation = to;
        pf.SetState(PortafilterState.Locked);
        pf.GetBusy = false;
        PlayerHand.Instance.Say("Portafiltro asegurado.");
    }
}