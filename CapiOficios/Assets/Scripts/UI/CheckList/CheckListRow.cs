using TMPro;
using UnityEngine;

public class ChecklistRow : MonoBehaviour
{
    public enum State { Pending, Current, Done }

    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private GameObject _tick;            // opcional: tilde
    [SerializeField] private GameObject _currentMarker;   // opcional: flecha o resaltado
    [SerializeField] private Color _pendingColor = new(0.15f, 0.12f, 0.10f);
    [SerializeField] private Color _currentColor = new(0.10f, 0.35f, 0.65f);
    [SerializeField] private Color _doneColor = new(0.45f, 0.45f, 0.45f);

    public void Setup(string text) => _label.text = text;
    public void SetState(State s)
    {
        _label.color = s == State.Done ? _doneColor : s == State.Current ? _currentColor : _pendingColor;
        _label.fontStyle = s == State.Done ? FontStyles.Strikethrough
                         : s == State.Current ? FontStyles.Bold : FontStyles.Normal;
        if (_tick != null) _tick.SetActive(s == State.Done);
        if (_currentMarker != null) _currentMarker.SetActive(s == State.Current);
    }
}