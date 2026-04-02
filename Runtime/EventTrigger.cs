using UnityEngine;

namespace EventManager.Runtime
{
    public enum EventTriggerMode
    {
        OnStart,
        OnEnable,
        OnDisable,
        OnDestroy,
        OnTriggerEnter,
        OnTriggerExit,
        OnInteract
    }

    /// <summary>
    /// Fires a <see cref="GameEvent"/> or a named <see cref="EventSequenceData"/> in response to
    /// common Unity scene lifecycle events or physics triggers without requiring any code.
    /// </summary>
    [AddComponentMenu("EventManager/Event Trigger")]
    public class EventTrigger : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [Tooltip("Name of the event to fire, or leave empty when using Sequence mode.")]
        [SerializeField] private string eventName;

        [Tooltip("If set, fire this sequence id instead of a single event.")]
        [SerializeField] private string sequenceId;

        [Tooltip("Optional string payload attached to the event (ignored in Sequence mode).")]
        [SerializeField] private string stringPayload;

        [Tooltip("Optional integer payload attached to the event (ignored in Sequence mode).")]
        [SerializeField] private int intPayload;

        [SerializeField] private EventTriggerMode triggerMode = EventTriggerMode.OnTriggerEnter;

        [Tooltip("Only fire once per scene lifetime.")]
        [SerializeField] private bool fireOnce = true;

        [Tooltip("Collider tag filter for OnTriggerEnter / OnTriggerExit modes.")]
        [SerializeField] private string triggerTag = "Player";

        // ─── Internal ────────────────────────────────────────────────────────────
        private EventManager _events;
        private bool _fired;

        private void Awake()
        {
            _events = FindFirstObjectByType<EventManager>();
            if (_events == null)
                Debug.LogWarning("[EventTrigger] No EventManager found in scene.");
        }

        private void Start()
        {
            if (triggerMode == EventTriggerMode.OnStart) Fire();
        }

        private void OnEnable()
        {
            if (triggerMode == EventTriggerMode.OnEnable) Fire();
        }

        private void OnDisable()
        {
            if (triggerMode == EventTriggerMode.OnDisable) Fire();
        }

        private void OnDestroy()
        {
            if (triggerMode == EventTriggerMode.OnDestroy) Fire();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerMode != EventTriggerMode.OnTriggerEnter) return;
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Fire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerMode != EventTriggerMode.OnTriggerEnter) return;
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Fire();
        }

        private void OnTriggerExit(Collider other)
        {
            if (triggerMode != EventTriggerMode.OnTriggerExit) return;
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Fire();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (triggerMode != EventTriggerMode.OnTriggerExit) return;
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Fire();
        }

        /// <summary>Manually fire the event (for OnInteract mode or external calls).</summary>
        public void Interact() => Fire();

        private void Fire()
        {
            if (fireOnce && _fired) return;
            if (_events == null) return;

            _fired = true;

            // Sequence mode
            if (!string.IsNullOrEmpty(sequenceId))
            {
                _events.FireSequence(sequenceId);
                return;
            }

            // Single-event mode
            if (string.IsNullOrEmpty(eventName)) return;
            var evt = new GameEvent(eventName, stringPayload, intPayload);
            _events.Fire(evt);
        }
    }
}
