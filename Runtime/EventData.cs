using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventManager.Runtime
{
    // -------------------------------------------------------------------------
    // EventDefinitionData
    // -------------------------------------------------------------------------

    /// <summary>
    /// Declares a named event channel with default payloads and metadata.
    /// Authored in JSON and stored in <c>Resources/Events/</c>.
    /// </summary>
    [Serializable]
    public class EventDefinitionData
    {
        /// <summary>Unique event channel name, e.g. <c>"player.died"</c>.</summary>
        public string id;

        /// <summary>Human-readable label shown in the Editor.</summary>
        public string label;

        /// <summary>Description of when and why this event is fired.</summary>
        public string description;

        /// <summary>Default string payload (used when firing without an explicit value).</summary>
        public string defaultStringValue;

        /// <summary>Default integer payload.</summary>
        public int defaultIntValue;

        /// <summary>Default float payload.</summary>
        public float defaultFloatValue;

        /// <summary>Optional tags for filtering or grouping events in the Editor.</summary>
        public string[] tags;

        /// <summary>Raw JSON stored during deserialisation (non-serialised).</summary>
        [NonSerialized] public string rawJson;
    }

    // -------------------------------------------------------------------------
    // EventSequenceStep
    // -------------------------------------------------------------------------

    /// <summary>
    /// A single step inside an <see cref="EventSequenceData"/>.
    /// </summary>
    [Serializable]
    public class EventSequenceStep
    {
        /// <summary>Name of the event to fire.</summary>
        public string eventName;

        /// <summary>Optional string payload for this step.</summary>
        public string stringValue;

        /// <summary>Optional integer payload for this step.</summary>
        public int intValue;

        /// <summary>Optional float payload for this step.</summary>
        public float floatValue;

        /// <summary>Delay in seconds before this step fires. 0 = immediate.</summary>
        public float delayBefore;
    }

    // -------------------------------------------------------------------------
    // EventSequenceData
    // -------------------------------------------------------------------------

    /// <summary>
    /// An ordered list of <see cref="EventSequenceStep"/> items fired in sequence.
    /// Authored in JSON and stored in <c>Resources/EventSequences/</c>.
    /// </summary>
    [Serializable]
    public class EventSequenceData
    {
        /// <summary>Unique identifier used to trigger the sequence by name.</summary>
        public string id;

        /// <summary>Human-readable label shown in the Editor.</summary>
        public string label;

        /// <summary>Ordered steps to execute.</summary>
        public List<EventSequenceStep> steps;

        /// <summary>Raw JSON stored during deserialisation (non-serialised).</summary>
        [NonSerialized] public string rawJson;
    }

    // -------------------------------------------------------------------------
    // GameEvent
    // -------------------------------------------------------------------------

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
