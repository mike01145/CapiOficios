using System;
using System.Collections;
using UnityEngine;

public static class PourHelper
{
    // El item ya tiene que estar suelto (ReleaseHeld). Se mueve sobre el target, se inclina,
    // llama a onPoured a mitad de camino y al final vuelve a su lugar.
    public static IEnumerator Pour(Carryable item, Transform target, Action onPoured, Action onDone = null,
                                   float height = 0.25f, float tiltDeg = 100f, float seconds = 0.9f)
    {
        Vector3 startPos = item.transform.position;
        Quaternion startRot = item.transform.rotation;
        Vector3 overPos = target.position + Vector3.up * height;
        Quaternion tilted = Quaternion.Euler(0f, 0f, tiltDeg) * startRot;

        float t = 0f;
        bool poured = false;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / seconds);
            float k = p < 0.4f ? p / 0.4f : p < 0.7f ? 1f : 1f - (p - 0.7f) / 0.3f;   // ir, esperar, volver
            k = Mathf.SmoothStep(0f, 1f, k);
            item.transform.SetPositionAndRotation(Vector3.Lerp(startPos, overPos, k), Quaternion.Slerp(startRot, tilted, k));

            if (!poured && p >= 0.55f) { poured = true; onPoured?.Invoke(); }
            yield return null;
        }

        if (!poured) onPoured?.Invoke();
        onDone?.Invoke();
        item.ReturnHome();
    }
}