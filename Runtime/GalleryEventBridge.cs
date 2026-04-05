#if EVENTMANAGER_GAL
using UnityEngine;
using GalleryManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and GalleryManager.
    /// Enable define <c>EVENTMANAGER_GAL</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires a named game event on the <see cref="EventManager"/> bus when GalleryManager raises
    /// its lifecycle event:
    /// <list type="bullet">
    /// <item><c>"gallery.entry.unlocked"</c> – <see cref="GameEvent.stringValue"/> = entry id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Gallery Event Bridge")]
    [DisallowMultipleComponent]
    public class GalleryEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string entryUnlockedEventName = "gallery.entry.unlocked";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private GalleryManager.Runtime.GalleryManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<GalleryManager.Runtime.GalleryManager>()
                      ?? FindFirstObjectByType<GalleryManager.Runtime.GalleryManager>();

            if (_events == null) Debug.LogWarning("[GalleryEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[GalleryEventBridge] GalleryManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null) _mgr.OnEntryUnlocked += OnEntryUnlocked;
        }

        private void OnDisable()
        {
            if (_mgr != null) _mgr.OnEntryUnlocked -= OnEntryUnlocked;
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnEntryUnlocked(string id) =>
            _events?.Fire(new GameEvent(entryUnlockedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_GAL in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Gallery Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class GalleryEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
