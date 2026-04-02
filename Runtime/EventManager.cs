using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EventManager.Runtime
{
    /// <summary>
    /// <b>EventManager</b> is a global, named-channel event bus.
    /// <para>
    /// <b>Responsibilities:</b>
    /// <list type="number">
    /// <item>Let any system fire a <see cref="GameEvent"/> by name without hard references.</item>
    /// <item>Let any system subscribe or unsubscribe typed handlers by channel name.</item>
    /// <item>Support one-shot subscriptions via <see cref="Once"/>.</item>
    /// <item>Track a history of recent events for Editor inspection and debugging.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Setup:</b> Add to a persistent manager GameObject. All other systems find it via
    /// <c>FindFirstObjectByType</c> or a direct Inspector reference.
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Event Manager")]
    [DisallowMultipleComponent]
    public class EventManager : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────────
        [Tooltip("Maximum number of events kept in the history log.")]
        [SerializeField] private int historyCapacity = 50;

        [Tooltip("Log every fired event to the Unity Console.")]
        [SerializeField] private bool verboseLogging = false;

        [Tooltip("If true, definitions and sequences are also loaded from persistentDataPath/Events/ and persistentDataPath/EventSequences/.")]
        [SerializeField] private bool loadFromPersistentDataPath = true;

        [Header("Loaded data (read-only, set at runtime)")]
        [SerializeField] private List<string> loadedDefinitionIds  = new List<string>();
        [SerializeField] private List<string> loadedSequenceIds    = new List<string>();

        // ─── State ───────────────────────────────────────────────────────────────
        private readonly Dictionary<string, List<Action<GameEvent>>> _handlers =
            new Dictionary<string, List<Action<GameEvent>>>();

        private readonly Dictionary<string, List<Action<GameEvent>>> _onceHandlers =
            new Dictionary<string, List<Action<GameEvent>>>();

        private readonly List<GameEvent> _history = new List<GameEvent>();

        private readonly Dictionary<string, EventDefinitionData> _definitions =
            new Dictionary<string, EventDefinitionData>();

        private readonly Dictionary<string, EventSequenceData> _sequences =
            new Dictionary<string, EventSequenceData>();

        private Coroutine _activeSequence;

        // ─── Events ──────────────────────────────────────────────────────────────

        /// <summary>Fired when a sequence starts. Parameter: sequence id.</summary>
        public event Action<string> OnSequenceStarted;
        /// <summary>Fired when a sequence completes. Parameter: sequence id.</summary>
        public event Action<string> OnSequenceCompleted;

        // ─── Unity lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            LoadAllDefinitions();
            LoadAllSequences();
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>Fire a <see cref="GameEvent"/> to all subscribers of its channel.</summary>
        public void Fire(GameEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.name))
            {
                Debug.LogWarning("[EventManager] Attempted to fire a null or unnamed event.");
                return;
            }

            if (verboseLogging)
                Debug.Log($"[EventManager] Fire: {evt}");

            // History
            _history.Add(evt);
            if (_history.Count > historyCapacity)
                _history.RemoveAt(0);

            // Persistent handlers
            if (_handlers.TryGetValue(evt.name, out var handlers))
            {
                // Copy list to avoid modification during iteration
                var snapshot = new List<Action<GameEvent>>(handlers);
                foreach (var handler in snapshot)
                {
                    try { handler?.Invoke(evt); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventManager] Handler threw on event '{evt.name}': {ex}");
                    }
                }
            }

            // One-shot handlers
            if (_onceHandlers.TryGetValue(evt.name, out var onceList))
            {
                var snapshot = new List<Action<GameEvent>>(onceList);
                onceList.Clear();
                foreach (var handler in snapshot)
                {
                    try { handler?.Invoke(evt); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventManager] Once-handler threw on event '{evt.name}': {ex}");
                    }
                }
            }
        }

        /// <summary>Fire a simple named event with no payload.</summary>
        public void Fire(string eventName) => Fire(new GameEvent(eventName));

        /// <summary>Fire an event with a string payload.</summary>
        public void Fire(string eventName, string stringValue) =>
            Fire(new GameEvent(eventName, stringValue));

        /// <summary>Fire an event with an integer payload.</summary>
        public void Fire(string eventName, int intValue) =>
            Fire(new GameEvent(eventName, intValue));

        /// <summary>Fire an event with a float payload.</summary>
        public void Fire(string eventName, float floatValue) =>
            Fire(new GameEvent(eventName, floatValue));

        // ─── Subscription ────────────────────────────────────────────────────────

        /// <summary>Subscribe <paramref name="handler"/> to the named channel.</summary>
        public void On(string eventName, Action<GameEvent> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (!_handlers.TryGetValue(eventName, out var list))
            {
                list = new List<Action<GameEvent>>();
                _handlers[eventName] = list;
            }

            if (!list.Contains(handler))
                list.Add(handler);
        }

        /// <summary>Unsubscribe <paramref name="handler"/> from the named channel.</summary>
        public void Off(string eventName, Action<GameEvent> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (_handlers.TryGetValue(eventName, out var list))
                list.Remove(handler);
        }

        /// <summary>
        /// Subscribe <paramref name="handler"/> to fire exactly once, then auto-unsubscribe.
        /// </summary>
        public void Once(string eventName, Action<GameEvent> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (!_onceHandlers.TryGetValue(eventName, out var list))
            {
                list = new List<Action<GameEvent>>();
                _onceHandlers[eventName] = list;
            }

            list.Add(handler);
        }

        /// <summary>Remove all persistent subscribers for the named channel.</summary>
        public void ClearChannel(string eventName)
        {
            _handlers.Remove(eventName);
            _onceHandlers.Remove(eventName);
        }

        /// <summary>Remove all subscribers for all channels.</summary>
        public void ClearAll()
        {
            _handlers.Clear();
            _onceHandlers.Clear();
        }

        // ─── Query ───────────────────────────────────────────────────────────────

        /// <summary>Returns the number of persistent subscribers for the named channel.</summary>
        public int SubscriberCount(string eventName)
        {
            return _handlers.TryGetValue(eventName, out var list) ? list.Count : 0;
        }

        /// <summary>Returns a snapshot of the recent event history.</summary>
        public IReadOnlyList<GameEvent> GetHistory() => _history;

        /// <summary>Returns a snapshot of all registered channel names that have at least one subscriber.</summary>
        public IReadOnlyCollection<string> GetActiveChannels() => _handlers.Keys;

        /// <summary>Clear the event history log.</summary>
        public void ClearHistory() => _history.Clear();

        // ─── JSON Loading ─────────────────────────────────────────────────────────

        /// <summary>
        /// Loads all event definition JSON files from <c>Resources/Events/</c> and the external Events folder.
        /// Call again at runtime to reload after mod changes.
        /// </summary>
        public void LoadAllDefinitions()
        {
            _definitions.Clear();
            loadedDefinitionIds.Clear();

            var resourceAssets = Resources.LoadAll<TextAsset>("Events");
            foreach (var asset in resourceAssets)
                RegisterDefinitionFromJson(asset.text);

            if (loadFromPersistentDataPath)
            {
                string dir = Path.Combine(Application.persistentDataPath, "Events");
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
                    {
                        try { RegisterDefinitionFromJson(File.ReadAllText(file)); }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Failed to load definition from {file}: {ex.Message}");
                        }
                    }
                }
            }

            Debug.Log($"[EventManager] Loaded {_definitions.Count} event definition(s).");
        }

        private void RegisterDefinitionFromJson(string json)
        {
            try
            {
                var def = JsonUtility.FromJson<EventDefinitionData>(json);
                if (def == null || string.IsNullOrEmpty(def.id)) return;
                def.rawJson = json;
                _definitions[def.id] = def;
                if (!loadedDefinitionIds.Contains(def.id))
                    loadedDefinitionIds.Add(def.id);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventManager] Failed to parse definition JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads all event sequence JSON files from <c>Resources/EventSequences/</c> and the external EventSequences folder.
        /// Call again at runtime to reload after mod changes.
        /// </summary>
        public void LoadAllSequences()
        {
            _sequences.Clear();
            loadedSequenceIds.Clear();

            var resourceAssets = Resources.LoadAll<TextAsset>("EventSequences");
            foreach (var asset in resourceAssets)
                RegisterSequenceFromJson(asset.text);

            if (loadFromPersistentDataPath)
            {
                string dir = Path.Combine(Application.persistentDataPath, "EventSequences");
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
                    {
                        try { RegisterSequenceFromJson(File.ReadAllText(file)); }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Failed to load sequence from {file}: {ex.Message}");
                        }
                    }
                }
            }

            Debug.Log($"[EventManager] Loaded {_sequences.Count} event sequence(s).");
        }

        private void RegisterSequenceFromJson(string json)
        {
            try
            {
                var seq = JsonUtility.FromJson<EventSequenceData>(json);
                if (seq == null || string.IsNullOrEmpty(seq.id)) return;
                seq.rawJson = json;
                _sequences[seq.id] = seq;
                if (!loadedSequenceIds.Contains(seq.id))
                    loadedSequenceIds.Add(seq.id);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventManager] Failed to parse sequence JSON: {ex.Message}");
            }
        }

        // ─── Sequence Firing ──────────────────────────────────────────────────────

        /// <summary>
        /// Fire the named event sequence. Steps are executed in order, each after its
        /// <c>delayBefore</c> seconds. If a sequence is already running it is stopped first.
        /// </summary>
        public void FireSequence(string id)
        {
            if (!_sequences.TryGetValue(id, out var seq))
            {
                Debug.LogWarning($"[EventManager] Sequence '{id}' not found.");
                return;
            }
            FireSequence(seq);
        }

        /// <summary>Fire an <see cref="EventSequenceData"/> directly (e.g. built at runtime).</summary>
        public void FireSequence(EventSequenceData sequence)
        {
            if (sequence == null) return;
            if (_activeSequence != null) StopCoroutine(_activeSequence);
            _activeSequence = StartCoroutine(FireSequenceCoroutine(sequence));
        }

        /// <summary>Stop the currently running sequence coroutine, if any.</summary>
        public void StopSequence()
        {
            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                _activeSequence = null;
            }
        }

        private IEnumerator FireSequenceCoroutine(EventSequenceData sequence)
        {
            OnSequenceStarted?.Invoke(sequence.id);

            if (sequence.steps != null)
            {
                foreach (var step in sequence.steps)
                {
                    if (step.delayBefore > 0f)
                        yield return new WaitForSeconds(step.delayBefore);

                    if (!string.IsNullOrEmpty(step.eventName))
                    {
                        var evt = new GameEvent(step.eventName, step.stringValue, step.intValue);
                        evt.floatValue = step.floatValue;
                        Fire(evt);
                    }
                }
            }

            _activeSequence = null;
            OnSequenceCompleted?.Invoke(sequence.id);
        }

        // ─── Definition / Sequence Query ─────────────────────────────────────────

        /// <summary>Returns the <see cref="EventDefinitionData"/> for the given id, or null.</summary>
        public EventDefinitionData GetDefinition(string id) =>
            _definitions.TryGetValue(id, out var def) ? def : null;

        /// <summary>Returns all loaded event definitions.</summary>
        public IReadOnlyDictionary<string, EventDefinitionData> GetAllDefinitions() => _definitions;

        /// <summary>Returns the <see cref="EventSequenceData"/> for the given id, or null.</summary>
        public EventSequenceData GetSequence(string id) =>
            _sequences.TryGetValue(id, out var seq) ? seq : null;

        /// <summary>Returns all loaded event sequences.</summary>
        public IReadOnlyDictionary<string, EventSequenceData> GetAllSequences() => _sequences;

        /// <summary>
        /// Fire the named event using default payloads from its <see cref="EventDefinitionData"/>,
        /// if a definition is loaded for it.  Falls back to a payload-less fire if no definition exists.
        /// </summary>
        public void FireWithDefaults(string eventName)
        {
            if (_definitions.TryGetValue(eventName, out var def))
                Fire(new GameEvent(def.id, def.defaultStringValue, def.defaultIntValue) { floatValue = def.defaultFloatValue });
            else
                Fire(eventName);
        }
    }
}
