#if EVENTMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and CutsceneManager.
    /// Enable define <c>EVENTMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when the CutsceneManager raises its own events:
    /// <list type="bullet">
    /// <item><c>"cutscene.started"</c>   — <see cref="GameEvent.stringValue"/> = sequence id</item>
    /// <item><c>"cutscene.completed"</c> — <see cref="GameEvent.stringValue"/> = sequence id</item>
    /// <item><c>"cutscene.skipped"</c>   — <see cref="GameEvent.stringValue"/> = sequence id</item>
    /// <item><c>"cutscene.event"</c>     — <see cref="GameEvent.stringValue"/> = sequence id,
    ///       <see cref="GameEvent.name"/> overridden by custom event key when <see cref="forwardCustomEvents"/> is true</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Cutscene Event Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneEventBridge : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [Tooltip("Event name fired when a cutscene sequence starts.")]
        [SerializeField] private string cutsceneStartedEventName = "cutscene.started";

        [Tooltip("Event name fired when a cutscene sequence completes.")]
        [SerializeField] private string cutsceneCompletedEventName = "cutscene.completed";

        [Tooltip("Event name fired when a cutscene sequence is skipped.")]
        [SerializeField] private string cutsceneSkippedEventName = "cutscene.skipped";

        [Tooltip("If true, CutsceneManager.OnCustomEvent is forwarded directly as a GameEvent " +
                 "using the custom event key as the event name.")]
        [SerializeField] private bool forwardCustomEvents = true;

        // ─── References ──────────────────────────────────────────────────────────
        private EventManager _events;
        private CutsceneManager.Runtime.CutsceneManager _cutscene;

        // ─── Unity ───────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events   = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _cutscene = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                        ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_events == null)
                Debug.LogWarning("[CutsceneEventBridge] EventManager not found.");
            if (_cutscene == null)
                Debug.LogWarning("[CutsceneEventBridge] CutsceneManager not found — event bridge disabled.");
        }

        private void OnEnable()
        {
            if (_cutscene != null)
            {
                _cutscene.OnSequenceStarted   += OnSequenceStarted;
                _cutscene.OnSequenceCompleted += OnSequenceCompleted;
                _cutscene.OnSequenceSkipped   += OnSequenceSkipped;
                _cutscene.OnCustomEvent       += OnCustomEvent;
            }
        }

        private void OnDisable()
        {
            if (_cutscene != null)
            {
                _cutscene.OnSequenceStarted   -= OnSequenceStarted;
                _cutscene.OnSequenceCompleted -= OnSequenceCompleted;
                _cutscene.OnSequenceSkipped   -= OnSequenceSkipped;
                _cutscene.OnCustomEvent       -= OnCustomEvent;
            }
        }

        // ─── Handlers ────────────────────────────────────────────────────────────
        private void OnSequenceStarted(string sequenceId)
        {
            if (_events == null) return;
            _events.Fire(new GameEvent(cutsceneStartedEventName, sequenceId));
        }

        private void OnSequenceCompleted(string sequenceId)
        {
            if (_events == null) return;
            _events.Fire(new GameEvent(cutsceneCompletedEventName, sequenceId));
        }

        private void OnSequenceSkipped(string sequenceId)
        {
            if (_events == null) return;
            _events.Fire(new GameEvent(cutsceneSkippedEventName, sequenceId));
        }

        private void OnCustomEvent(string sequenceId, string customKey)
        {
            if (_events == null) return;

            if (forwardCustomEvents && !string.IsNullOrEmpty(customKey))
            {
                // Forward custom event key directly as the event name
                _events.Fire(new GameEvent(customKey, sequenceId));
            }
            else
            {
                _events.Fire(new GameEvent("cutscene.event", sequenceId));
            }
        }
    }
}
#else
// EVENTMANAGER_CSM not defined — bridge is inactive.
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_CSM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Cutscene Event Bridge")]
    public class CutsceneEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake()
        {
            UnityEngine.Debug.Log("[CutsceneEventBridge] CutsceneManager integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_CSM to enable it.");
        }
    }
}
#endif
