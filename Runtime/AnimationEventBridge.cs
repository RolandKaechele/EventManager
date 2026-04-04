#if EVENTMANAGER_ANM
using UnityEngine;
using AnimationManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and AnimationManager.
    /// Enable define <c>EVENTMANAGER_ANM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when AnimationManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"animation.started"</c>   — <see cref="GameEvent.stringValue"/> = animation id</item>
    ///   <item><c>"animation.stopped"</c>   — <see cref="GameEvent.stringValue"/> = animation id</item>
    ///   <item><c>"animation.completed"</c> — <see cref="GameEvent.stringValue"/> = animation id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Animation Event Bridge")]
    [DisallowMultipleComponent]
    public class AnimationEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when an animation starts.")]
        [SerializeField] private string startedEventName   = "animation.started";

        [Tooltip("Event name fired when an animation is stopped.")]
        [SerializeField] private string stoppedEventName   = "animation.stopped";

        [Tooltip("Event name fired when an animation completes naturally.")]
        [SerializeField] private string completedEventName = "animation.completed";

        private EventManager _events;
        private AnimationManager.Runtime.AnimationManager _anim;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _anim   = GetComponent<AnimationManager.Runtime.AnimationManager>()
                      ?? FindFirstObjectByType<AnimationManager.Runtime.AnimationManager>();

            if (_events == null) Debug.LogWarning("[AnimationEventBridge] EventManager not found.");
            if (_anim   == null) Debug.LogWarning("[AnimationEventBridge] AnimationManager not found.");
        }

        private void OnEnable()
        {
            if (_anim != null)
            {
                _anim.OnAnimationStarted   += OnStarted;
                _anim.OnAnimationStopped   += OnStopped;
                _anim.OnAnimationCompleted += OnCompleted;
            }
        }

        private void OnDisable()
        {
            if (_anim != null)
            {
                _anim.OnAnimationStarted   -= OnStarted;
                _anim.OnAnimationStopped   -= OnStopped;
                _anim.OnAnimationCompleted -= OnCompleted;
            }
        }

        private void OnStarted(string id)   => _events?.Fire(new GameEvent(startedEventName,   id));
        private void OnStopped(string id)   => _events?.Fire(new GameEvent(stoppedEventName,   id));
        private void OnCompleted(string id) => _events?.Fire(new GameEvent(completedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — enable define <c>EVENTMANAGER_ANM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Animation Event Bridge")]
    public class AnimationEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationEventBridge] Bridge disabled — add EVENTMANAGER_ANM to Scripting Define Symbols.");
    }
}
#endif
