using UnityEngine;

public class MilkJug : Carryable
{
    [SerializeField] private GameObject _milkVisual;   // opcional

    public MilkQuality GetMilk { get; private set; }     // None = vacía
    public bool GetFoamed { get; private set; }
    public float GetTemperature { get; private set; } = 5f;

    private void Start() => Refresh();

    public void Fill()
    {
        GetMilk = MilkQuality.Cold;
        GetFoamed = false;
        GetTemperature = 5f;
        Refresh();
    }

    // Menos de 60 = fría, 60 a 90 = en su punto, más de 90 = quemada
    public void SetTemperature(float temp, bool foam)
    {
        GetTemperature = temp;
        GetFoamed = foam && temp >= 60f && temp <= 90f;
        GetMilk = temp > 90f ? MilkQuality.Burnt : (temp >= 60f ? MilkQuality.Good : MilkQuality.Cold);
        Refresh();
    }

    public override void ResetState()
    {
        GetMilk = MilkQuality.None;
        GetFoamed = false;
        GetTemperature = 5f;
        Refresh();
    }

    private void Refresh()
    {
        if (_milkVisual != null)
            _milkVisual.SetActive(GetMilk != MilkQuality.None);
    }
}