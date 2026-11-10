using System;
using UnityEngine;

public class ParticleReactor : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string name;
        [Tooltip("Con qué evento arranca.")]
        public Feedback playOn;
        [Tooltip("Con qué evento se detiene (None = no se detiene por evento; sirve para ráfagas).")]
        public Feedback stopOn;
        public ParticleSystem system;
        [Tooltip("Mueve el sistema a la posición del objeto que generó el evento.")]
        public bool moveToSource;
        [Tooltip("Solo reacciona a eventos de este objeto (vacío = de cualquiera).")]
        public Transform onlyFrom;
    }

    [SerializeField] private Entry[] _entries;

    private void OnEnable() => GameEvents.OnFeedbackRaised += ParticleReactor_OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackRaised -= ParticleReactor_OnFeedback;

    private void ParticleReactor_OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        foreach (var entry in _entries)
        {
            if (entry.system == null) continue;

            bool fromOk = entry.onlyFrom == null || (e.Source != null && (e.Source == entry.onlyFrom || e.Source.IsChildOf(entry.onlyFrom)));
            if (!fromOk) continue;

            if (entry.playOn != Feedback.None && entry.playOn == e.Feedback)
            {
                if (entry.moveToSource && e.Source != null) entry.system.transform.position = e.Source.position;
                entry.system.Play();
            }
            if (entry.stopOn != Feedback.None && entry.stopOn == e.Feedback) entry.system.Stop();
        }
    }
}