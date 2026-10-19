using System.Collections;
using UnityEngine;
public class SteamWand : ItemSlot
{
    [SerializeField] private float _heatPerSecond = 12f;
    [SerializeField] private float _minGood = 60f;
    [SerializeField] private float _maxGood = 90f;
    [SerializeField] private float _maxTemp = 100f;
    [SerializeField] private ParticleSystem _steamFx;   // opcional

    private bool _foamMode;
    private bool _heating;
    private float _currentTemp;
    private Coroutine _routine;

    // Conectalo a un Toggle de UI ("Espumar") para el capuchino
    public void SetFoamMode(bool on) => _foamMode = on;

    protected override bool Accepts(Carryable c, out string why)
    {
        if (!base.Accepts(c, out why)) return false;
        var jug = c as MilkJug;
        if (jug == null) { why = "Ahí va la jarra de leche."; return false; }
        if (jug.GetMilk == MilkQuality.None) { why = "La jarra está vacía. Cargala con leche primero."; return false; }
        if (jug.GetMilk == MilkQuality.Burnt) { why = "Esa leche está quemada, descartala."; return false; }
        if (jug.GetMilk == MilkQuality.Good) { why = "La leche ya está caliente."; return false; }
        return true;
    }

    protected override void OnUsedEmptyHanded()
    {
        var jug = GetPlaced as MilkJug;
        if (jug == null) { PlayerHand.Instance.Warn("Poné la jarra de leche bajo la varilla de vapor.", transform); return; }

        if (_heating) StopHeating(jug);
        else _routine = StartCoroutine(Heat(jug));
    }

    private IEnumerator Heat(MilkJug jug)
    {
        _heating = true;
        jug.GetBusy = true;
        GameEvents.Raise(Feedback.SteamStart, transform);
        if (_steamFx != null) _steamFx.Play();

        _currentTemp = jug.GetTemperature;
        while (_currentTemp < _maxTemp)
        {
            _currentTemp = Mathf.Min(_maxTemp, _currentTemp + _heatPerSecond * Time.deltaTime);
            ShowTemperature();
            yield return null;
        }

        Finish(jug); // llegó al máximo: se quemó
    }

    private void StopHeating(MilkJug jug)
    {
        if (_routine != null) StopCoroutine(_routine);
        Finish(jug);
    }

    private void Finish(MilkJug jug)
    {
        _heating = false;
        _routine = null;
        jug.GetBusy = false;
        if (_steamFx != null) _steamFx.Stop();
        GameEvents.ClearProgress(transform);
        GameEvents.Raise(Feedback.SteamEnd, transform);

        jug.SetTemperature(_currentTemp, _foamMode);

        string t = Mathf.RoundToInt(_currentTemp) + "°C";
        var hand = PlayerHand.Instance;
        if (_currentTemp < _minGood) hand.Say("Todavía está fría (" + t + "). Seguí calentando.");
        else if (_currentTemp <= _maxGood)
        {
            GameEvents.CompleteStep(GameStep.HeatMilk);
            hand.Say("¡Leche en su punto! (" + t + ")");
        }
        else hand.Say("¡Se quemó la leche (" + t + ")! Descartala en el tacho.");
    }

    private void ShowTemperature()
    {
        Color c;
        if (_currentTemp < _minGood) c = new Color(0.35f, 0.6f, 1f); // fría
        else if (_currentTemp <= _maxGood) c = new Color(0.3f, 0.75f, 0.35f); // en su punto
        else c = new Color(0.9f, 0.25f, 0.25f); // demasiado caliente

        GameEvents.ShowProgress(transform, "Calentando leche", _currentTemp / _maxTemp, Mathf.RoundToInt(_currentTemp) + "°C", c);
    }
}