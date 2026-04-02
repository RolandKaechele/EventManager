#if EVENTMANAGER_DM
using UnityEngine;
using DialogueManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and DialogueManager.
    /// Enable define <c>EVENTMANAGER_DM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when DialogueManager raises its own events:
    /// <list type="bullet">
    /// <item><c>"dialogue.started"</c>   — <see cref="GameEvent.stringValue"/> = sequence id</item>
    /// <item><c>"dialogue.completed"</c> — <see cref="GameEvent.stringValue"/> = sequence id</item>
    /// <item><c>"dialogue.node"</c>      — <see cref="GameEvent.stringValue"/> = sequence id,
    ///       <see cref="GameEvent.objectValue"/> = node id (string)</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Dialogue Event Bridge")]
    [DisallowMultipleComponent]
    public class DialogueEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a dialogue sequence starts.")]
        [SerializeField] private string startedEventName   = "dialogue.started";

        [Tooltip("Event name fired when a dialogue sequence ends.")]
        [SerializeField] private string completedEventName = "dialogue.completed";

        [Tooltip("Event name fired when a dialogue node is shown. Set empty to disable.")]
        [SerializeField] private string nodeEventName      = "dialogue.node";

        private EventManager _events;
        private DialogueManager.Runtime.DialogueManager _dialogue;

        private void Awake()
        {
            _events   = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _dialogue = GetComponent<DialogueManager.Runtime.DialogueManager>()
                        ?? FindFirstObjectByType<DialogueManager.Runtime.DialogueManager>();

            if (_events   == null) Debug.LogWarning("[DialogueEventBridge] EventManager not found.");
            if (_dialogue == null) Debug.LogWarning("[DialogueEventBridge] DialogueManager not found.");
        }

        private void OnEnable()
        {
            if (_dialogue != null)
            {
                _dialogue.OnDialogueStarted   += OnStarted;
                _dialogue.OnDialogueCompleted += OnCompleted;
                _dialogue.OnNodeShown         += OnNodeShown;
            }
        }

        private void OnDisable()
        {
            if (_dialogue != null)
            {
                _dialogue.OnDialogueStarted   -= OnStarted;
                _dialogue.OnDialogueCompleted -= OnCompleted;
                _dialogue.OnNodeShown         -= OnNodeShown;
            }
        }

        private void OnStarted(string sequenceId) =>
            _events?.Fire(new GameEvent(startedEventName, sequenceId));

        private void OnCompleted(string sequenceId) =>
            _events?.Fire(new GameEvent(completedEventName, sequenceId));

        private void OnNodeShown(string sequenceId, string nodeId)
        {
            if (_events == null || string.IsNullOrEmpty(nodeEventName)) return;
            var evt = new GameEvent(nodeEventName, sequenceId) { objectValue = nodeId };
            _events.Fire(evt);
        }
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_DM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Dialogue Event Bridge")]
    public class DialogueEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[DialogueEventBridge] DialogueManager integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_DM to enable it.");
    }
}
#endif
