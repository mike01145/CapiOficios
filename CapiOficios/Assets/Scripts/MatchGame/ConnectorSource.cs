
using UnityEngine;
using UnityEngine.EventSystems;

// Ponelo en cada botón de la IZQUIERDA
public class ConnectorSource : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int id;
    [SerializeField] private MatchingGame _game;
    [HideInInspector] public bool Connected;

    public void OnPointerDown(PointerEventData e) => _game.BeginLink(this, e);
    public void OnDrag(PointerEventData e) => _game.UpdateLink(e);
    public void OnPointerUp(PointerEventData e) => _game.EndLink(e);
}