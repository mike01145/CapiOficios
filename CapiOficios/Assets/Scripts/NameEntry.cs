using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private Button _buttonConfirm;
    [SerializeField] private TextMeshProUGUI _errorText; // opcional: "Escribí tu nombre"

    [Header("Opciones")]
    [SerializeField] private int _maxLength = 20;
    [SerializeField] private UnityEvent _onNameConfirmed; // opcional: para pasar a la siguiente escena
    private void Start()
    {
        _inputFieldName.characterLimit = _maxLength;
        _inputFieldName.text = PlayerProfile.Name;     // si ya había un nombre, lo muestra
        if (_errorText != null) _errorText.gameObject.SetActive(false);

        _buttonConfirm.onClick.AddListener(Confirm);
    }
    void Update()
    {
        if (_buttonConfirm == null || _inputFieldName == null) return;
        _buttonConfirm.gameObject.SetActive(!string.IsNullOrEmpty(_inputFieldName.text.Trim()));
    }
    // También sirve conectarlo al evento "On End Edit" del input para confirmar con Enter
    public void Confirm()
    {
        string name = _inputFieldName.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(true);
                _errorText.text = "Escribí tu nombre";
            }
            return;
        }
        if (name.Length > _maxLength)
        {
            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(true);
                _errorText.text = $"El nombre no puede tener más de {_maxLength} caracteres";
            }
            return;
        }
        // se guarda el nombre y se puede pasar a la siguiente escena
        PlayerProfile.SetName(name);
        _onNameConfirmed?.Invoke();
    }

}