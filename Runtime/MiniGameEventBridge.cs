#if EVENTMANAGER_MGM
using UnityEngine;
using MiniGameManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and MiniGameManager.
    /// Enable define <c>EVENTMANAGER_MGM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when MiniGameManager raises
    /// its lifecycle events:
    /// <list type="bullet">
    /// <item><c>"minigame.started"</c> — <see cref="GameEvent.stringValue"/> = mini-game id</item>
    /// <item><c>"minigame.completed"</c> — <see cref="GameEvent.stringValue"/> = mini-game id,
    ///       <see cref="GameEvent.intValue"/> = score, <see cref="GameEvent.floatValue"/> = timestamp</item>
    /// <item><c>"minigame.aborted"</c> — <see cref="GameEvent.stringValue"/> = mini-game id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Mini Game Event Bridge")]
    [DisallowMultipleComponent]
    public class MiniGameEventBridge : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [SerializeField] private string startedEventName   = "minigame.started";
        [SerializeField] private string completedEventName = "minigame.completed";
        [SerializeField] private string abortedEventName   = "minigame.aborted";

        // ─── References ──────────────────────────────────────────────────────────
        private EventManager _events;
        private MiniGameManager.Runtime.MiniGameManager _mgr;

        // ─── Unity ───────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<MiniGameManager.Runtime.MiniGameManager>()
                      ?? FindFirstObjectByType<MiniGameManager.Runtime.MiniGameManager>();

            if (_events == null) Debug.LogWarning("[MiniGameEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[MiniGameEventBridge] MiniGameManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null)
            {
                _mgr.OnMiniGameStarted   += OnStarted;
                _mgr.OnMiniGameCompleted += OnCompleted;
                _mgr.OnMiniGameAborted   += OnAborted;
            }
        }

        private void OnDisable()
        {
            if (_mgr != null)
            {
                _mgr.OnMiniGameStarted   -= OnStarted;
                _mgr.OnMiniGameCompleted -= OnCompleted;
                _mgr.OnMiniGameAborted   -= OnAborted;
            }
        }

        // ─── Handlers ────────────────────────────────────────────────────────────
        private void OnStarted(string id) =>
            _events?.Fire(new GameEvent(startedEventName, id));

        private void OnCompleted(MiniGameResult result) =>
            _events?.Fire(new GameEvent(completedEventName, result.miniGameId, result.score)
                          { floatValue = result.timestamp });

        private void OnAborted(string id) =>
            _events?.Fire(new GameEvent(abortedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_MGM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Mini Game Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class MiniGameEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
