#if EVENTMANAGER_INP
using UnityEngine;
using InputManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and InputManager.
    /// Enable define <c>EVENTMANAGER_INP</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when InputManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"input.profileChanged"</c> — <see cref="GameEvent.stringValue"/> = new profile id</item>
    ///   <item><c>"input.blocked"</c>        — fired when all input is blocked</item>
    ///   <item><c>"input.unblocked"</c>      — fired when input is restored</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Input Event Bridge")]
    [DisallowMultipleComponent]
    public class InputEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on input profile change.")]
        [SerializeField] private string profileChangedEventName = "input.profileChanged";

        [Tooltip("Event name fired when input is blocked.")]
        [SerializeField] private string blockedEventName = "input.blocked";

        [Tooltip("Event name fired when input is unblocked.")]
        [SerializeField] private string unblockedEventName = "input.unblocked";

        private EventManager _events;
        private InputManager.Runtime.InputManager _input;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _input  = GetComponent<InputManager.Runtime.InputManager>()
                      ?? FindFirstObjectByType<InputManager.Runtime.InputManager>();

            if (_events == null) Debug.LogWarning("[InputEventBridge] EventManager not found.");
            if (_input  == null) Debug.LogWarning("[InputEventBridge] InputManager not found.");
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnProfileChanged += OnProfileChanged;
                _input.OnInputBlocked   += OnInputBlocked;
                _input.OnInputUnblocked += OnInputUnblocked;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnProfileChanged -= OnProfileChanged;
                _input.OnInputBlocked   -= OnInputBlocked;
                _input.OnInputUnblocked -= OnInputUnblocked;
            }
        }

        private void OnProfileChanged(string id) => _events?.Fire(new GameEvent(profileChangedEventName, id));
        private void OnInputBlocked()             => _events?.Fire(new GameEvent(blockedEventName));
        private void OnInputUnblocked()           => _events?.Fire(new GameEvent(unblockedEventName));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — enable define <c>EVENTMANAGER_INP</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Input Event Bridge")]
    public class InputEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputEventBridge] Bridge disabled — add EVENTMANAGER_INP to Scripting Define Symbols.");
    }
}
#endif
