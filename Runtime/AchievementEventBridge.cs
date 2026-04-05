#if EVENTMANAGER_ACH
using UnityEngine;
using AchievementManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and AchievementManager.
    /// Enable define <c>EVENTMANAGER_ACH</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when AchievementManager raises
    /// its lifecycle events:
    /// <list type="bullet">
    /// <item><c>"achievement.unlocked"</c> – <see cref="GameEvent.stringValue"/> = achievement id</item>
    /// <item><c>"achievement.progress"</c> – <see cref="GameEvent.stringValue"/> = achievement id,
    ///       <see cref="GameEvent.intValue"/> = current progress,
    ///       <see cref="GameEvent.floatValue"/> = progress target</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Achievement Event Bridge")]
    [DisallowMultipleComponent]
    public class AchievementEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string unlockedEventName  = "achievement.unlocked";
        [SerializeField] private string progressEventName  = "achievement.progress";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private AchievementManager.Runtime.AchievementManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<AchievementManager.Runtime.AchievementManager>()
                      ?? FindFirstObjectByType<AchievementManager.Runtime.AchievementManager>();

            if (_events == null) Debug.LogWarning("[AchievementEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[AchievementEventBridge] AchievementManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null)
            {
                _mgr.OnAchievementUnlocked += OnUnlocked;
                _mgr.OnProgressUpdated     += OnProgress;
            }
        }

        private void OnDisable()
        {
            if (_mgr != null)
            {
                _mgr.OnAchievementUnlocked -= OnUnlocked;
                _mgr.OnProgressUpdated     -= OnProgress;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnUnlocked(string id) =>
            _events?.Fire(new GameEvent(unlockedEventName, id));

        private void OnProgress(string id, int current, int target) =>
            _events?.Fire(new GameEvent(progressEventName, id, current) { floatValue = target });
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_ACH in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Achievement Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class AchievementEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
