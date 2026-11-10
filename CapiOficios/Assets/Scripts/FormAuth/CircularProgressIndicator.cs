using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float _speed = 360f; // grados por segundo

    private RectTransform _rect;

    private void Awake() => _rect = (RectTransform)transform;

    private void OnEnable() => _rect.localRotation = Quaternion.identity;

    private void Update()
    {
        _rect.Rotate(0f, 0 - _speed * Time.unscaledDeltaTime, 0f);
    }
}