#if EVENTMANAGER_AIM
using UnityEngine;
using AiManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and AiManager.
    /// Enable define <c>EVENTMANAGER_AIM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when AiManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"ai.alertlevel.changed"</c>  — <see cref="GameEvent.intValue"/> = new <see cref="AiAlertLevel"/> cast to int</item>
    ///   <item><c>"ai.boss.phase.changed"</c>  — <see cref="GameEvent.stringValue"/> = boss id, <see cref="GameEvent.intValue"/> = new phase</item>
    ///   <item><c>"ai.frozen"</c>              — fired when all AI agents are frozen</item>
    ///   <item><c>"ai.unfrozen"</c>            — fired when all AI agents are unfrozen</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/AI Event Bridge")]
    [DisallowMultipleComponent]
    public class AiEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when the global alert level changes.")]
        [SerializeField] private string alertLevelChangedEventName = "ai.alertlevel.changed";

        [Tooltip("Event name fired when a boss phase advances.")]
        [SerializeField] private string bossPhasChangedEventName   = "ai.boss.phase.changed";

        [Tooltip("Event name fired when all AI agents are frozen.")]
        [SerializeField] private string frozenEventName            = "ai.frozen";

        [Tooltip("Event name fired when all AI agents are unfrozen.")]
        [SerializeField] private string unfrozenEventName          = "ai.unfrozen";

        private EventManager _events;
        private AiManager.Runtime.AiManager _ai;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _ai     = GetComponent<AiManager.Runtime.AiManager>()
                      ?? FindFirstObjectByType<AiManager.Runtime.AiManager>();

            if (_events == null) Debug.LogWarning("[AiEventBridge] EventManager not found.");
            if (_ai     == null) Debug.LogWarning("[AiEventBridge] AiManager not found.");
        }

        private void OnEnable()
        {
            if (_ai != null)
            {
                _ai.OnAlertLevelChanged += OnAlertLevelChanged;
                _ai.OnBossPhaseChanged  += OnBossPhaseChanged;
                _ai.OnAiFrozenChanged   += OnAiFrozenChanged;
            }
        }

        private void OnDisable()
        {
            if (_ai != null)
            {
                _ai.OnAlertLevelChanged -= OnAlertLevelChanged;
                _ai.OnBossPhaseChanged  -= OnBossPhaseChanged;
                _ai.OnAiFrozenChanged   -= OnAiFrozenChanged;
            }
        }

        private void OnAlertLevelChanged(AiAlertLevel previous, AiAlertLevel next)
        {
            _events?.Fire(new GameEvent(alertLevelChangedEventName) { intValue = (int)next });
        }

        private void OnBossPhaseChanged(string bossId, int phase)
        {
            _events?.Fire(new GameEvent(bossPhasChangedEventName, bossId) { intValue = phase });
        }

        private void OnAiFrozenChanged(bool frozen)
        {
            _events?.Fire(frozen ? frozenEventName : unfrozenEventName);
        }
    }
}
#else
// Define EVENTMANAGER_AIM in Player Settings › Scripting Define Symbols to enable this bridge.
#endif
