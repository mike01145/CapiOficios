using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private float _seconds = 2.2f;
    [Tooltip("Opcional: texto fijo que muestra qué llevás en la mano.")]
    [SerializeField] private TMP_Text _heldLabel;
    [SerializeField] private string _heldPrefix = "En la mano: ";

    private Coroutine routine;

    private void OnEnable()
    {
        GameEvents.OnMessage += MessageUI_Show;
        GameEvents.OnHeldChanged += MessageUI_SetHeld;

        if (_messageText != null) _messageText.text = "";
        if (_heldLabel != null) _heldLabel.text = "";
    }
    private void OnDisable()
    {
        GameEvents.OnMessage -= MessageUI_Show;
        GameEvents.OnHeldChanged -= MessageUI_SetHeld;
    }

    private void MessageUI_SetHeld(object sender, GameEvents.MessageEventArgs e)
    {
        if (_heldLabel == null) return;
        _heldLabel.text = string.IsNullOrEmpty(e.ItemName) ? "" : _heldPrefix + e.ItemName;
    }

    private void MessageUI_Show(object sender, GameEvents.MessageEventArgs e)
    {
        if (_messageText == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(e.Text));
    }
    private IEnumerator ShowRoutine(string text)
    {
        _messageText.text = text;
        yield return new WaitForSecondsRealtime(_seconds);
        _messageText.text = "";
        routine = null;
    }
}