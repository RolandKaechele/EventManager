using System;
using UnityEngine;

namespace EventManager.Runtime
{
    /// <summary>
    /// A named game event with optional typed payloads.
    /// All fields are optional; consumers read only the fields relevant to them.
    /// </summary>
    [Serializable]
    public class GameEvent
    {
        /// <summary>The event name / channel key, e.g. <c>"map.loaded"</c>, <c>"player.died"</c>.</summary>
        public string name;

        /// <summary>Optional string payload — map id, sequence id, item id, etc.</summary>
        public string stringValue;

        /// <summary>Optional integer payload — chapter index, quantity, score delta, etc.</summary>
        public int intValue;

        /// <summary>Optional float payload — damage amount, time remaining, etc.</summary>
        public float floatValue;

        /// <summary>Optional arbitrary object payload. Not serialized by JsonUtility.</summary>
        [NonSerialized]
        public object objectValue;

        // ── Constructors ──────────────────────────────────────────────────────────

        public GameEvent() { }

        public GameEvent(string name)
        {
            this.name = name;
        }

        public GameEvent(string name, string stringValue)
        {
            this.name        = name;
            this.stringValue = stringValue;
        }

        public GameEvent(string name, int intValue)
        {
            this.name     = name;
            this.intValue = intValue;
        }

        public GameEvent(string name, float floatValue)
        {
            this.name       = name;
            this.floatValue = floatValue;
        }

        public GameEvent(string name, string stringValue, int intValue)
        {
            this.name        = name;
            this.stringValue = stringValue;
            this.intValue    = intValue;
        }

        public override string ToString() =>
            $"GameEvent({name}, str={stringValue}, int={intValue}, float={floatValue})";
    }
}
