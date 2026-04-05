#if EVENTMANAGER_BOOT
using UnityEngine;
using BootStartupManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and BootStartupManager.
    /// Enable define <c>EVENTMANAGER_BOOT</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires a named game event on the <see cref="EventManager"/> bus when the boot sequence finishes:
    /// <list type="bullet">
    /// <item><c>"boot.complete"</c> – no payload</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Boot Event Bridge")]
    [DisallowMultipleComponent]
    public class BootEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string bootCompleteEventName = "boot.complete";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private BootStartupManager.Runtime.BootStartupManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<BootStartupManager.Runtime.BootStartupManager>()
                      ?? FindFirstObjectByType<BootStartupManager.Runtime.BootStartupManager>();

            if (_events == null) Debug.LogWarning("[BootEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[BootEventBridge] BootStartupManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null) _mgr.OnBootComplete += OnBootComplete;
        }

        private void OnDisable()
        {
            if (_mgr != null) _mgr.OnBootComplete -= OnBootComplete;
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnBootComplete() =>
            _events?.Fire(new GameEvent(bootCompleteEventName));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_BOOT in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Boot Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class BootEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
