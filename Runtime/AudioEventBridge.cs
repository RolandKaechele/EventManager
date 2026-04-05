#if EVENTMANAGER_AU
using UnityEngine;
using AudioManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and AudioManager.
    /// Enable define <c>EVENTMANAGER_AU</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when AudioManager raises
    /// its lifecycle events:
    /// <list type="bullet">
    /// <item><c>"audio.music.started"</c> – <see cref="GameEvent.stringValue"/> = track id/resource</item>
    /// <item><c>"audio.music.stopped"</c> – no payload</item>
    /// <item><c>"audio.track.changed"</c> – <see cref="GameEvent.stringValue"/> = new track id/resource</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Audio Event Bridge")]
    [DisallowMultipleComponent]
    public class AudioEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string musicStartedEventName = "audio.music.started";
        [SerializeField] private string musicStoppedEventName = "audio.music.stopped";
        [SerializeField] private string trackChangedEventName = "audio.track.changed";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private AudioManager.Runtime.AudioManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<AudioManager.Runtime.AudioManager>()
                      ?? FindFirstObjectByType<AudioManager.Runtime.AudioManager>();

            if (_events == null) Debug.LogWarning("[AudioEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[AudioEventBridge] AudioManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null)
            {
                _mgr.OnMusicStarted  += OnMusicStarted;
                _mgr.OnMusicStopped  += OnMusicStopped;
                _mgr.OnTrackChanged  += OnTrackChanged;
            }
        }

        private void OnDisable()
        {
            if (_mgr != null)
            {
                _mgr.OnMusicStarted  -= OnMusicStarted;
                _mgr.OnMusicStopped  -= OnMusicStopped;
                _mgr.OnTrackChanged  -= OnTrackChanged;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnMusicStarted(string trackId) =>
            _events?.Fire(new GameEvent(musicStartedEventName, trackId));

        private void OnMusicStopped() =>
            _events?.Fire(new GameEvent(musicStoppedEventName));

        private void OnTrackChanged(string trackId) =>
            _events?.Fire(new GameEvent(trackChangedEventName, trackId));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_AU in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Audio Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class AudioEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
