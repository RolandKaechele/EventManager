#if EVENTMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and MapLoaderFramework.
    /// Enable define <c>EVENTMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when the framework raises its own events:
    /// <list type="bullet">
    /// <item><c>"map.loaded"</c> — <see cref="GameEvent.stringValue"/> = map id</item>
    /// <item><c>"chapter.changed"</c> — <see cref="GameEvent.intValue"/> = new chapter,
    ///       <see cref="GameEvent.stringValue"/> = previous chapter as string</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Map Loader Event Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderEventBridge : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [Tooltip("Event name fired when a map finishes loading.")]
        [SerializeField] private string mapLoadedEventName = "map.loaded";

        [Tooltip("Event name fired when the active chapter changes.")]
        [SerializeField] private string chapterChangedEventName = "chapter.changed";

        // ─── References ──────────────────────────────────────────────────────────
        private EventManager _events;
        private MapLoaderFramework.Runtime.MapLoaderFramework _framework;

        // ─── Unity ───────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events    = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _framework = GetComponent<MapLoaderFramework.Runtime.MapLoaderFramework>()
                         ?? FindFirstObjectByType<MapLoaderFramework.Runtime.MapLoaderFramework>();

            if (_events == null)
                Debug.LogWarning("[MapLoaderEventBridge] EventManager not found.");
            if (_framework == null)
                Debug.LogWarning("[MapLoaderEventBridge] MapLoaderFramework not found — event bridge disabled.");
        }

        private void OnEnable()
        {
            if (_framework != null)
            {
                _framework.OnMapLoaded      += OnMapLoaded;
                _framework.OnChapterChanged += OnChapterChanged;
            }
        }

        private void OnDisable()
        {
            if (_framework != null)
            {
                _framework.OnMapLoaded      -= OnMapLoaded;
                _framework.OnChapterChanged -= OnChapterChanged;
            }
        }

        // ─── Handlers ────────────────────────────────────────────────────────────
        private void OnMapLoaded(MapData mapData)
        {
            if (_events == null || mapData == null) return;
            _events.Fire(new GameEvent(mapLoadedEventName, mapData.id));
        }

        private void OnChapterChanged(int previous, int current)
        {
            if (_events == null) return;
            _events.Fire(new GameEvent(chapterChangedEventName, previous.ToString(), current));
        }
    }
}
#else
// EVENTMANAGER_MLF not defined — bridge is inactive.
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_MLF in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Map Loader Event Bridge")]
    public class MapLoaderEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake()
        {
            UnityEngine.Debug.Log("[MapLoaderEventBridge] MapLoaderFramework integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_MLF to enable it.");
        }
    }
}
#endif
