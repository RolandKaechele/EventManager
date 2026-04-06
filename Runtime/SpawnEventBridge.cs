#if EVENTMANAGER_SPM
using UnityEngine;
using SpawnManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and SpawnManager.
    /// Enable define <c>EVENTMANAGER_SPM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when SpawnManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"spawn.spawned"</c>         — <see cref="GameEvent.stringValue"/> = definition id</item>
    ///   <item><c>"spawn.despawned"</c>        — <see cref="GameEvent.stringValue"/> = definition id</item>
    ///   <item><c>"spawn.wave.completed"</c>  — <see cref="GameEvent.stringValue"/> = wave id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Spawn Event Bridge")]
    [DisallowMultipleComponent]
    public class SpawnEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a prefab is spawned.")]
        [SerializeField] private string spawnedEventName       = "spawn.spawned";

        [Tooltip("Event name fired when a prefab is despawned.")]
        [SerializeField] private string despawnedEventName     = "spawn.despawned";

        [Tooltip("Event name fired when a spawn wave completes.")]
        [SerializeField] private string waveCompletedEventName = "spawn.wave.completed";

        private EventManager              _events;
        private SpawnManager.Runtime.SpawnManager _spawns;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _spawns = GetComponent<SpawnManager.Runtime.SpawnManager>()
                      ?? FindFirstObjectByType<SpawnManager.Runtime.SpawnManager>();

            if (_events == null) Debug.LogWarning("[SpawnEventBridge] EventManager not found.");
            if (_spawns == null) Debug.LogWarning("[SpawnEventBridge] SpawnManager not found.");
        }

        private void OnEnable()
        {
            if (_spawns != null)
            {
                _spawns.OnSpawned       += OnSpawned;
                _spawns.OnDespawned     += OnDespawned;
                _spawns.OnWaveCompleted += OnWaveCompleted;
            }
        }

        private void OnDisable()
        {
            if (_spawns != null)
            {
                _spawns.OnSpawned       -= OnSpawned;
                _spawns.OnDespawned     -= OnDespawned;
                _spawns.OnWaveCompleted -= OnWaveCompleted;
            }
        }

        private void OnSpawned(string defId, string instanceId, GameObject go)
            => _events?.Fire(new GameEvent(spawnedEventName, defId));

        private void OnDespawned(string defId, string instanceId)
            => _events?.Fire(new GameEvent(despawnedEventName, defId));

        private void OnWaveCompleted(string waveId)
            => _events?.Fire(new GameEvent(waveCompletedEventName, waveId));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — define <c>EVENTMANAGER_SPM</c> to activate this bridge.</summary>
    public class SpawnEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
