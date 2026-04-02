using System;
using System.Collections.Generic;
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

        // ─── State ───────────────────────────────────────────────────────────────
        private readonly Dictionary<string, List<Action<GameEvent>>> _handlers =
            new Dictionary<string, List<Action<GameEvent>>>();

        private readonly Dictionary<string, List<Action<GameEvent>>> _onceHandlers =
            new Dictionary<string, List<Action<GameEvent>>>();

        private readonly List<GameEvent> _history = new List<GameEvent>();

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
    }
}
