#if EVENTMANAGER_VID
using UnityEngine;
using VideoManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and VideoManager.
    /// Enable define <c>EVENTMANAGER_VID</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when VideoManager raises
    /// its lifecycle events:
    /// <list type="bullet">
    /// <item><c>"video.started"</c>   – <see cref="GameEvent.stringValue"/> = video id/resource</item>
    /// <item><c>"video.completed"</c> – <see cref="GameEvent.stringValue"/> = video id/resource</item>
    /// <item><c>"video.stopped"</c>   – <see cref="GameEvent.stringValue"/> = video id/resource</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Video Event Bridge")]
    [DisallowMultipleComponent]
    public class VideoEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string startedEventName   = "video.started";
        [SerializeField] private string completedEventName = "video.completed";
        [SerializeField] private string stoppedEventName   = "video.stopped";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private VideoManager.Runtime.VideoManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<VideoManager.Runtime.VideoManager>()
                      ?? FindFirstObjectByType<VideoManager.Runtime.VideoManager>();

            if (_events == null) Debug.LogWarning("[VideoEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[VideoEventBridge] VideoManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null)
            {
                _mgr.OnVideoStarted   += OnStarted;
                _mgr.OnVideoCompleted += OnCompleted;
                _mgr.OnVideoStopped   += OnStopped;
            }
        }

        private void OnDisable()
        {
            if (_mgr != null)
            {
                _mgr.OnVideoStarted   -= OnStarted;
                _mgr.OnVideoCompleted -= OnCompleted;
                _mgr.OnVideoStopped   -= OnStopped;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnStarted(string id) =>
            _events?.Fire(new GameEvent(startedEventName, id));

        private void OnCompleted(string id) =>
            _events?.Fire(new GameEvent(completedEventName, id));

        private void OnStopped(string id) =>
            _events?.Fire(new GameEvent(stoppedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_VID in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Video Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class VideoEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
