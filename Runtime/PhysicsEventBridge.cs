#if EVENTMANAGER_PHY
using UnityEngine;
using PhysicsManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and PhysicsManager.
    /// Enable define <c>EVENTMANAGER_PHY</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when PhysicsManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"physics.impact"</c>          — <see cref="GameEvent.floatValue"/> = impulse magnitude</item>
    ///   <item><c>"physics.profile.changed"</c> — <see cref="GameEvent.stringValue"/> = new profile id</item>
    ///   <item><c>"physics.paused"</c>           — no payload</item>
    ///   <item><c>"physics.resumed"</c>          — no payload</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Physics Event Bridge")]
    [DisallowMultipleComponent]
    public class PhysicsEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a physics impact occurs.")]
        [SerializeField] private string impactEventName         = "physics.impact";

        [Tooltip("Event name fired when the active physics profile changes.")]
        [SerializeField] private string profileChangedEventName = "physics.profile.changed";

        [Tooltip("Event name fired when physics simulation is paused.")]
        [SerializeField] private string pausedEventName         = "physics.paused";

        [Tooltip("Event name fired when physics simulation is resumed.")]
        [SerializeField] private string resumedEventName        = "physics.resumed";

        private EventManager             _events;
        private PhysicsManager.Runtime.PhysicsManager _physics;

        private void Awake()
        {
            _events  = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _physics = GetComponent<PhysicsManager.Runtime.PhysicsManager>()
                       ?? FindFirstObjectByType<PhysicsManager.Runtime.PhysicsManager>();

            if (_events  == null) Debug.LogWarning("[PhysicsEventBridge] EventManager not found.");
            if (_physics == null) Debug.LogWarning("[PhysicsEventBridge] PhysicsManager not found.");
        }

        private void OnEnable()
        {
            if (_physics != null)
            {
                _physics.OnImpact         += OnImpact;
                _physics.OnProfileChanged += OnProfileChanged;
                _physics.OnPhysicsPaused  += OnPhysicsPaused;
                _physics.OnPhysicsResumed += OnPhysicsResumed;
            }
        }

        private void OnDisable()
        {
            if (_physics != null)
            {
                _physics.OnImpact         -= OnImpact;
                _physics.OnProfileChanged -= OnProfileChanged;
                _physics.OnPhysicsPaused  -= OnPhysicsPaused;
                _physics.OnPhysicsResumed -= OnPhysicsResumed;
            }
        }

        private void OnImpact(ImpactData data)
            => _events?.Fire(new GameEvent(impactEventName) { floatValue = data.impulse });

        private void OnProfileChanged(string previous, string next)
            => _events?.Fire(new GameEvent(profileChangedEventName, next));

        private void OnPhysicsPaused()
            => _events?.Fire(new GameEvent(pausedEventName));

        private void OnPhysicsResumed()
            => _events?.Fire(new GameEvent(resumedEventName));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — define <c>EVENTMANAGER_PHY</c> to activate this bridge.</summary>
    public class PhysicsEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
