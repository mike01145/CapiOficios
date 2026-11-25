
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBubble : MonoBehaviour
{
    [SerializeField] private RectTransform _bubble;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Configuracion")]
    [SerializeField] private float _maxWidth = 300f;
    [SerializeField] private float _paddingX = 40f;
    [SerializeField] private float _paddingY = 24f;

    public void SetText(string message)
    {
        _text.text = message;

        // Primero calculamos el ancho natural del texto
        _text.ForceMeshUpdate();

        float preferredWidth =
            _text.GetPreferredValues(message).x;

        // Limitamos el ancho, sin establecer un minimo
        float finalWidth = Mathf.Min(
            preferredWidth + _paddingX,
            _maxWidth
        );

        // Aplicamos el ancho disponible al texto
        float textWidth = finalWidth - _paddingX;

        // Calculamos la altura necesaria para los renglones
        float textHeight = _text.GetPreferredValues(
            message,
            textWidth,
            Mathf.Infinity
        ).y;

        // Actualizamos el tamaño del globo
        _bubble.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            finalWidth
        );

        _bubble.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            textHeight + _paddingY
        );
    }
}
