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
        _label.text = _phaseBeforeName + PlayerProfile.Name + _phaseAfterName;
    }
}
