#if EVENTMANAGER_ASM
using UnityEngine;
using AssetManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and AssetManager.
    /// Enable define <c>EVENTMANAGER_ASM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires a named game event on the <see cref="EventManager"/> bus when AssetManager raises
    /// its lifecycle event:
    /// <list type="bullet">
    /// <item><c>"asset.loaded"</c> – <see cref="GameEvent.stringValue"/> = asset id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Asset Event Bridge")]
    [DisallowMultipleComponent]
    public class AssetEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string assetLoadedEventName = "asset.loaded";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private AssetManager.Runtime.AssetManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<AssetManager.Runtime.AssetManager>()
                      ?? FindFirstObjectByType<AssetManager.Runtime.AssetManager>();

            if (_events == null) Debug.LogWarning("[AssetEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[AssetEventBridge] AssetManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null) _mgr.OnAssetLoaded += OnAssetLoaded;
        }

        private void OnDisable()
        {
            if (_mgr != null) _mgr.OnAssetLoaded -= OnAssetLoaded;
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnAssetLoaded(string id) =>
            _events?.Fire(new GameEvent(assetLoadedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_ASM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Asset Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class AssetEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
