using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NameTextLabel : MonoBehaviour
{
    [SerializeField] private string _phaseBeforeName = " ";   // lo que va entre el texto y el nombre
    [SerializeField] private string _phaseAfterName = "";       // opcional, por ejemplo "!"

    private TextMeshProUGUI _label;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        PlayerProfile.OnNameConfirmed += NameTextLable_OnNameConfirmed;
        NameTextLable_OnNameConfirmed(null, new PlayerProfile.NameConfirmedEventArgs { Name = PlayerProfile.Name });
    }
    private void OnDisable()
    {
        PlayerProfile.OnNameConfirmed -= NameTextLable_OnNameConfirmed;
    }
    private void NameTextLable_OnNameConfirmed(object sender, PlayerProfile.NameConfirmedEventArgs e)
    {
        _label.text = _phaseBeforeName + e.Name + _phaseAfterName;
    }
    public void ShowName(string name)
    {
        if (_label == null) _label = GetComponent<TextMeshProUGUI>();
        _label.text = _phaseBeforeName + name + _phaseAfterName;
    }
}
