using System;
using UnityEngine;

// Cosas que "pasaron" en el juego. Sonidos, partículas y animaciones reaccionan a estas.
public enum Feedback
{
    None = 0,
    Click, Pickup, Place, Return, Refused,
    BeansLoaded, GrindStart, GrindEnd,
    Tamp, PortafilterLocked,
    ExtractStart, ExtractEnd,
    SteamStart, SteamEnd,
    Fill, Pour,
    Discard, Delivered, WrongDrink
}

// Pasos de la receta (para la lista con tildes y la flecha de ayuda)
public enum GameStep
{
    None = 0,
    LoadBeans, Grind, Tamp, LockPortafilter, Extract,
    HeatMilk, PourMilk, AddWater, Deliver
}

public readonly struct ProgressInfo
{
    public readonly Transform Source;
    public readonly string Title;
    public readonly float Value;   // 0 a 1
    public readonly string Text;
    public readonly Color Color;

    public ProgressInfo(Transform source, string title, float value, string text, Color color)
    {
        Source = source;
        Title = title;
        Value = value;
        Text = text;
        Color = color;
    }
}

// Punto único de comunicación. Los objetos del juego SOLO emiten eventos acá;
// los sonidos, animaciones y la UI se suscriben y reaccionan por su cuenta.
public static class GameEvents
{
    public static readonly Color DefaultProgressColor = new(0.30f, 0.65f, 0.35f);
    public static event EventHandler<MessageEventArgs> OnMessage;                 // aviso para el jugador
    public static event EventHandler<MessageEventArgs> OnHeldChanged;             // nombre de lo que lleva en la mano ("" = nada)
    public static event EventHandler<FeedbackEventArgs> OnFeedbackRaised;
    public static event EventHandler<HoveredEventArgs> OnHovered;        // objeto, entró (true) / salió (false)
    public static event EventHandler<StepCompletedEventArgs> OnStepCompleted;
    public static event EventHandler<ProgressEventArgs> OnProgressChanged;
    public static event EventHandler<ProgressClearedEventArgs> OnProgressCleared;

    public class MessageEventArgs : EventArgs
    {
        public string Text { get; }
        public string ItemName { get; }
        public MessageEventArgs(string text, string itemName = "")
        {
            Text = text;
            ItemName = itemName;
        }
    }
    public class FeedbackEventArgs : EventArgs
    {
        public Feedback Feedback { get; }
        public Transform Source { get; }
        public FeedbackEventArgs(Feedback feedback, Transform source)
        {
            Feedback = feedback;
            Source = source;
        }
    }
    public class ProgressEventArgs : EventArgs
    {
        public ProgressInfo Info { get; }
        public ProgressEventArgs(ProgressInfo info) => Info = info;
    }
    public class HoveredEventArgs : EventArgs
    {
        public Transform Target { get; }
        public bool On { get; }
        public HoveredEventArgs(Transform target, bool on)
        {
            Target = target;
            On = on;
        }
    }
    public class StepCompletedEventArgs : EventArgs
    {
        public GameStep Step { get; }
        public StepCompletedEventArgs(GameStep step) => Step = step;
    }
    public class ProgressClearedEventArgs : EventArgs
    {
        public Transform Source { get; }
        public ProgressClearedEventArgs(Transform source) => Source = source;
    }
    public static void ShowMessage(string text) => OnMessage?.Invoke(null, new MessageEventArgs(text));
    public static void SetHeld(string itemName) => OnHeldChanged?.Invoke(null, new MessageEventArgs(itemName));
    public static void Raise(Feedback feedback, Transform source = null) => OnFeedbackRaised?.Invoke(null, new FeedbackEventArgs(feedback, source));
    public static void SetHovered(Transform target, bool on) => OnHovered?.Invoke(null, new HoveredEventArgs(target, on));
    public static void CompleteStep(GameStep step) => OnStepCompleted?.Invoke(null, new StepCompletedEventArgs(step));
    public static void ClearProgress(Transform source) => OnProgressCleared?.Invoke(null, new ProgressClearedEventArgs(source));

    public static void ShowProgress(Transform source, string title, float value, string text, Color? color = null)
        => OnProgressChanged?.Invoke(null, new ProgressEventArgs(new ProgressInfo(source, title, value, text, color ?? DefaultProgressColor)));

    // Por si desactivás "Domain Reload" en Enter Play Mode: limpia suscriptores viejos
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        OnMessage = null;
        OnHeldChanged = null;
        OnFeedbackRaised = null;
        OnHovered = null;
        OnStepCompleted = null;
        OnProgressChanged = null;
        OnProgressCleared = null;
    }
}