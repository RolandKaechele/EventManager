#if EVENTMANAGER_ENM
using UnityEngine;
using EnemyManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and EnemyManager.
    /// Enable define <c>EVENTMANAGER_ENM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when EnemyManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"enemy.spawned"</c>         — <see cref="GameEvent.stringValue"/> = instance id</item>
    ///   <item><c>"enemy.defeated"</c>        — <see cref="GameEvent.stringValue"/> = instance id</item>
    ///   <item><c>"enemy.wave.started"</c>    — <see cref="GameEvent.stringValue"/> = wave id</item>
    ///   <item><c>"enemy.wave.completed"</c>  — <see cref="GameEvent.stringValue"/> = wave id</item>
    ///   <item><c>"enemy.wave.aborted"</c>    — <see cref="GameEvent.stringValue"/> = wave id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Enemy Event Bridge")]
    [DisallowMultipleComponent]
    public class EnemyEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when an enemy is spawned.")]
        [SerializeField] private string spawnedEventName       = "enemy.spawned";

        [Tooltip("Event name fired when an enemy is defeated.")]
        [SerializeField] private string defeatedEventName      = "enemy.defeated";

        [Tooltip("Event name fired when a wave starts.")]
        [SerializeField] private string waveStartedEventName   = "enemy.wave.started";

        [Tooltip("Event name fired when a wave is completed.")]
        [SerializeField] private string waveCompletedEventName = "enemy.wave.completed";

        [Tooltip("Event name fired when a wave is aborted.")]
        [SerializeField] private string waveAbortedEventName   = "enemy.wave.aborted";

        private EventManager _events;
        private EnemyManager.Runtime.EnemyManager _enemies;

        private void Awake()
        {
            _events  = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _enemies = GetComponent<EnemyManager.Runtime.EnemyManager>()
                       ?? FindFirstObjectByType<EnemyManager.Runtime.EnemyManager>();

            if (_events  == null) Debug.LogWarning("[EnemyEventBridge] EventManager not found.");
            if (_enemies == null) Debug.LogWarning("[EnemyEventBridge] EnemyManager not found.");
        }

        private void OnEnable()
        {
            if (_enemies != null)
            {
                _enemies.OnEnemySpawned    += OnEnemySpawned;
                _enemies.OnEnemyDefeated   += OnEnemyDefeated;
                _enemies.OnWaveStarted     += OnWaveStarted;
                _enemies.OnWaveCompleted   += OnWaveCompleted;
                _enemies.OnWaveAborted     += OnWaveAborted;
            }
        }

        private void OnDisable()
        {
            if (_enemies != null)
            {
                _enemies.OnEnemySpawned    -= OnEnemySpawned;
                _enemies.OnEnemyDefeated   -= OnEnemyDefeated;
                _enemies.OnWaveStarted     -= OnWaveStarted;
                _enemies.OnWaveCompleted   -= OnWaveCompleted;
                _enemies.OnWaveAborted     -= OnWaveAborted;
            }
        }

        private void OnEnemySpawned(string enemyId, string instanceId)
            => _events?.Fire(new GameEvent(spawnedEventName, instanceId));

        private void OnEnemyDefeated(string enemyId, string instanceId)
            => _events?.Fire(new GameEvent(defeatedEventName, instanceId));

        private void OnWaveStarted(string waveId)
            => _events?.Fire(new GameEvent(waveStartedEventName, waveId));

        private void OnWaveCompleted(string waveId)
            => _events?.Fire(new GameEvent(waveCompletedEventName, waveId));

        private void OnWaveAborted(string waveId)
            => _events?.Fire(new GameEvent(waveAbortedEventName, waveId));
    }
}
#else
// Define EVENTMANAGER_ENM in Player Settings › Scripting Define Symbols to enable this bridge.
#endif
