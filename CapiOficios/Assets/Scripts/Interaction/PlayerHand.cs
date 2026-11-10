using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand Instance { get; private set; }

    [SerializeField] private Camera _cam; // vacio = Main Camera
    [SerializeField] private float _liftTowardCamera = 0.15f; // el objeto "flota" un poco hacia la camara
    [SerializeField] private float _followSharpness = 25f;
    public Carryable GetHeld { get; private set; }

    private float _depth;
    private Coroutine _hintRoutine;

    private void Awake()
    {
        Instance = this;
        if (_cam == null) _cam = Camera.main;
    }

    public bool Pick(Carryable c)
    {
        if (GetHeld != null || c == null) return false;
        GetHeld = c;
        _depth = Vector3.Dot(c.transform.position - _cam.transform.position, _cam.transform.forward);
        c.OnPickedUp();
        GameEvents.SetHeld(c.name);
        GameEvents.Raise(Feedback.Pickup, c.transform);
        Say("Agarraste: " + c.name + " (click derecho para soltar)");
        return true;
    }

    // Suelta lo que sostiene SIN moverlo (lo usan los slots al apoyarlo)
    public void ReleaseHeld()
    {
        GetHeld = null;
        GameEvents.SetHeld("");
    }

    // Lo que sostiene desaparece (por ejemplo, los granos al cargar el molinillo)
    public void Consume()
    {
        if (GetHeld == null) return;
        var c = GetHeld;
        GetHeld = null;
        Destroy(c.gameObject);
        GameEvents.SetHeld("");
    }

    public void ReturnHeldHome()
    {
        if (GetHeld == null) return;
        var c = GetHeld;
        GetHeld = null;
        c.ReturnHome();
        GameEvents.SetHeld("");
        GameEvents.Raise(Feedback.Return, c.transform);
    }

    // Botón de UI opcional para celulares (no hay click derecho)
    public void DropHeld()
    {
        if (GetHeld == null) return;
        ReturnHeldHome();
        Say("Lo dejaste en su lugar.");
    }
    public void Say(string message) => GameEvents.ShowMessage(message);
    public void Warn(string message, Transform source = null)
    {
        GameEvents.ShowMessage(message);
        GameEvents.Raise(Feedback.Refused, source);
    }

    private void Update()
    {
        if (GetHeld == null) return;

        if (CancelPressed()) { DropHeld(); return; }

        Ray ray = _cam.ScreenPointToRay(PointerPosition());
        float along = _depth / Mathf.Max(0.01f, Vector3.Dot(ray.direction, _cam.transform.forward));
        Vector3 target = ray.origin + ray.direction * along - _cam.transform.forward * _liftTowardCamera;
        float k = 1f - Mathf.Exp(-_followSharpness * Time.unscaledDeltaTime);
        GetHeld.transform.position = Vector3.Lerp(GetHeld.transform.position, target, k);
    }

    private static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        var p = UnityEngine.InputSystem.Pointer.current;
        return p != null ? p.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private static bool CancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var m = UnityEngine.InputSystem.Mouse.current;
        var k = UnityEngine.InputSystem.Keyboard.current;
        return (m != null && m.rightButton.wasPressedThisFrame) || (k != null && k.escapeKey.wasPressedThisFrame);
#else
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}