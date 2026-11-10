using UnityEngine;

public class HoverIcon : MonoBehaviour
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _labelText;
    [Tooltip("Dónde aparece el ícono, respecto de este objeto (en metros del mundo).")]
    [SerializeField] private Vector3 _worldOffset = new(0f, 0.3f, 0f);

    public Sprite GetIcon => _icon;
    public string GetLabel => _labelText;
    public Vector3 GetWorldOffset => _worldOffset;
}