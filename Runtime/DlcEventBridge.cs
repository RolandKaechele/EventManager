#if EVENTMANAGER_DLC
using UnityEngine;
using DlcManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and DlcManager.
    /// Enable define <c>EVENTMANAGER_DLC</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when DlcManager raises
    /// ownership events:
    /// <list type="bullet">
    /// <item><c>"dlc.unlocked"</c> — <see cref="GameEvent.stringValue"/> = pack id</item>
    /// <item><c>"dlc.revoked"</c>  — <see cref="GameEvent.stringValue"/> = pack id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/DLC Event Bridge")]
    [DisallowMultipleComponent]
    public class DlcEventBridge : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [SerializeField] private string unlockedEventName = "dlc.unlocked";
        [SerializeField] private string revokedEventName  = "dlc.revoked";

        // ─── References ──────────────────────────────────────────────────────────
        private EventManager _events;
        private DlcManager.Runtime.DlcManager _dlc;

        // ─── Unity ───────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _dlc    = GetComponent<DlcManager.Runtime.DlcManager>()
                      ?? FindFirstObjectByType<DlcManager.Runtime.DlcManager>();

            if (_events == null) Debug.LogWarning("[DlcEventBridge] EventManager not found.");
            if (_dlc    == null) Debug.LogWarning("[DlcEventBridge] DlcManager not found.");
        }

        private void OnEnable()
        {
            if (_dlc != null)
            {
                _dlc.OnPackUnlocked += OnUnlocked;
                _dlc.OnPackRevoked  += OnRevoked;
            }
        }

        private void OnDisable()
        {
            if (_dlc != null)
            {
                _dlc.OnPackUnlocked -= OnUnlocked;
                _dlc.OnPackRevoked  -= OnRevoked;
            }
        }

        // ─── Handlers ────────────────────────────────────────────────────────────
        private void OnUnlocked(string packId) =>
            _events?.Fire(new GameEvent(unlockedEventName, packId));

        private void OnRevoked(string packId) =>
            _events?.Fire(new GameEvent(revokedEventName, packId));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_DLC in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/DLC Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DlcEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
