#if EVENTMANAGER_STM
using UnityEngine;
using StateManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and StateManager.
    /// Enable define <c>EVENTMANAGER_STM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when StateManager transitions:
    /// <list type="bullet">
    ///   <item><c>"state.changed"</c> — <see cref="GameEvent.stringValue"/> = new <see cref="AppState"/> name.</item>
    ///   <item><c>"state.pushed"</c>  — <see cref="GameEvent.stringValue"/> = pushed state name.</item>
    ///   <item><c>"state.popped"</c>  — <see cref="GameEvent.stringValue"/> = popped state name.</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/State Event Bridge")]
    [DisallowMultipleComponent]
    public class StateEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on any state change.")]
        [SerializeField] private string stateChangedEventName = "state.changed";

        [Tooltip("Event name fired when a state is pushed.")]
        [SerializeField] private string statePushedEventName = "state.pushed";

        [Tooltip("Event name fired when a state is popped.")]
        [SerializeField] private string statePoppedEventName = "state.popped";

        private EventManager _events;
        private StateManager.Runtime.StateManager _state;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _state  = GetComponent<StateManager.Runtime.StateManager>()
                      ?? FindFirstObjectByType<StateManager.Runtime.StateManager>();

            if (_events == null) Debug.LogWarning("[StateEventBridge] EventManager not found.");
            if (_state  == null) Debug.LogWarning("[StateEventBridge] StateManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null)
            {
                _state.OnStateChanged += OnStateChanged;
                _state.OnStatePushed  += OnStatePushed;
                _state.OnStatePopped  += OnStatePopped;
            }
        }

        private void OnDisable()
        {
            if (_state != null)
            {
                _state.OnStateChanged -= OnStateChanged;
                _state.OnStatePushed  -= OnStatePushed;
                _state.OnStatePopped  -= OnStatePopped;
            }
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            _events?.Fire(new GameEvent(stateChangedEventName) { stringValue = next.ToString() });
        }

        private void OnStatePushed(AppState pushed)
        {
            _events?.Fire(new GameEvent(statePushedEventName) { stringValue = pushed.ToString() });
        }

        private void OnStatePopped(AppState popped)
        {
            _events?.Fire(new GameEvent(statePoppedEventName) { stringValue = popped.ToString() });
        }
    }
}
#endif
