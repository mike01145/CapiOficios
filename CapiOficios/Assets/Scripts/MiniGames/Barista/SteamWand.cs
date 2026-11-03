using System.Collections;
using UnityEngine;
public class SteamWand : ItemSlot
{
    [SerializeField] private float _heatPerSecond = 12f;
    [SerializeField] private float _minGood = 60f;
    [SerializeField] private float _maxGood = 90f;
    [SerializeField] private float _maxTemp = 100f;
    [SerializeField] private ParticleSystem _steamFx;   // opcional
    [SerializeField] private float _greenHoldTime = 3f;
    [SerializeField] private float _overheatDuration = 5f;

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
        // ETAPA 1: Calentamiento normal hasta 60°
        while (_currentTemp < _minGood)
        {
            _currentTemp += _heatPerSecond * Time.deltaTime;
            _currentTemp = Mathf.Min(_currentTemp, _minGood);

            ShowTemperature();
            yield return null;
        }

        // ETAPA 2: Zona verde, aumenta más rápido hasta 90°
        float greenSpeed = _heatPerSecond * 1.5f;

        while (_currentTemp < _maxGood)
        {
            _currentTemp += greenSpeed * Time.deltaTime;
            _currentTemp = Mathf.Min(_currentTemp, _maxGood);

            ShowTemperature();
            yield return null;
        }

        // ETAPA 3: Fill completamente verde durante 5 segundos
        _currentTemp = _maxGood;
        ShowTemperature();

        yield return new WaitForSeconds(_greenHoldTime);

        // ETAPA 4: Sobrecalentamiento hasta quemarse
        float elapsed = 0f;
        float overheatDuration = _overheatDuration;

        while (elapsed < overheatDuration)
        {
            elapsed += Time.deltaTime;

            _currentTemp = Mathf.Lerp(
                _maxGood,
                _maxTemp,
                elapsed / overheatDuration
            );

            ShowTemperature();
            yield return null;
        }

        _currentTemp = _maxTemp;
        ShowTemperature();

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
        float progress = Mathf.Clamp01(_currentTemp / _maxGood);

        GameEvents.ShowProgress(transform, "Calentando leche", progress, Mathf.RoundToInt(_currentTemp) + "°C", c, _minGood / _maxGood, _maxGood / _maxGood);
    }
}