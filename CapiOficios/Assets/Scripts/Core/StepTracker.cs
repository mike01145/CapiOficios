using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StepDef
{
    public GameStep step;
    public string label;
    [Tooltip("Objeto al que apunta la flecha de ayuda cuando este es el próximo paso.")]
    public Transform hintTarget;
}

public class StepTracker : MonoBehaviour
{
    [SerializeField] private List<StepDef> _listSteps = new();
    public event EventHandler OnChanged;
    public IReadOnlyList<StepDef> Steps => _listSteps;

    // Estado de cada paso (true = hecho)
    // Donde esta la lista de los pasos a realizar y tachar
    private bool[] _listStepsDone;

    private void Awake() => _listStepsDone = new bool[_listSteps.Count];
    //TODO Tener cuidado con los OnEnables aveces dan error, puede solucionarse con un Start().
    private void OnEnable() => GameEvents.OnStepCompleted += StepTracker_OnStepCompleted;
    private void OnDisable() => GameEvents.OnStepCompleted -= StepTracker_OnStepCompleted;

    private void StepTracker_OnStepCompleted(object sender, GameEvents.StepCompletedEventArgs e)
    {
        for (int i = 0; i < _listSteps.Count; i++)
        {
            if (_listSteps[i].step == e.Step && !_listStepsDone[i])
            {
                _listStepsDone[i] = true;
                OnChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
    }
    public bool IsDone(int index) => index >= 0 && index < _listStepsDone.Length && _listStepsDone[index];

    // Primer paso sin hacer (-1 si ya está todo)
    public int NextIndex()
    {
        for (int i = 0; i < _listSteps.Count; i++) if (!_listStepsDone[i]) return i;
        return -1;
    }

    public Transform NextTarget()
    {
        int i = NextIndex();
        return i >= 0 ? _listSteps[i].hintTarget : null;
    }

    // Para un botón de "Nuevo pedido"
    public void ResetAll()
    {
        Array.Clear(_listStepsDone, 0, _listStepsDone.Length);
        OnChanged?.Invoke(this, EventArgs.Empty);
    }
}