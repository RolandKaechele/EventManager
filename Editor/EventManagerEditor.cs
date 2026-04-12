#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EventManager.Runtime;

namespace EventManager.Editor
{
    /// <summary>
    /// Custom Inspector for <see cref="EventManager.Runtime.EventManager"/>.
    /// Shows loaded definitions, sequences, active channels, recent event history, and manual fire controls.
    /// </summary>
    [CustomEditor(typeof(Runtime.EventManager))]
    public class EventManagerEditor : UnityEditor.Editor
    {
        private string _fireEventName    = string.Empty;
        private string _fireStringValue  = string.Empty;
        private int    _fireIntValue;
        private string _fireSequenceId   = string.Empty;
        private bool   _showDefinitions  = false;
        private bool   _showSequences    = false;
        private bool   _showHistory      = true;
        private bool   _showChannels     = true;
        private Vector2 _historyScroll;
        private Vector2 _definitionsScroll;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var mgr = (Runtime.EventManager)target;

            EditorGUILayout.Space(8);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect events and channels.", MessageType.Info);
                return;
            }

            // ── Loaded Definitions ───────────────────────────────────────────────
            var definitions = mgr.GetAllDefinitions();
            _showDefinitions = EditorGUILayout.Foldout(_showDefinitions,
                $"Loaded Definitions ({definitions.Count})", true, EditorStyles.foldoutHeader);
            if (_showDefinitions)
            {
                if (definitions.Count == 0)
                {
                    EditorGUILayout.HelpBox("No definitions loaded. Place JSON files in Resources/Events/.", MessageType.None);
                }
                else
                {
                    _definitionsScroll = EditorGUILayout.BeginScrollView(_definitionsScroll, GUILayout.MaxHeight(140));
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID", EditorStyles.miniLabel, GUILayout.MinWidth(160));
                    EditorGUILayout.LabelField("Label", EditorStyles.miniLabel, GUILayout.MinWidth(120));
                    EditorGUILayout.EndHorizontal();
                    foreach (var kv in definitions)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(kv.Key,             GUILayout.MinWidth(160));
                        EditorGUILayout.LabelField(kv.Value.label ?? "", GUILayout.MinWidth(120));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.Space(4);

            // ── Loaded Sequences ─────────────────────────────────────────────────
            var sequences = mgr.GetAllSequences();
            _showSequences = EditorGUILayout.Foldout(_showSequences,
                $"Loaded Sequences ({sequences.Count})", true, EditorStyles.foldoutHeader);
            if (_showSequences)
            {
                if (sequences.Count == 0)
                {
                    EditorGUILayout.HelpBox("No sequences loaded. Place JSON files in Resources/EventSequences/.", MessageType.None);
                }
                else
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID", EditorStyles.miniLabel, GUILayout.MinWidth(160));
                    EditorGUILayout.LabelField("Steps", EditorStyles.miniLabel, GUILayout.Width(50));
                    EditorGUILayout.LabelField("Label", EditorStyles.miniLabel, GUILayout.MinWidth(100));
                    EditorGUILayout.EndHorizontal();
                    foreach (var kv in sequences)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(kv.Key,                                               GUILayout.MinWidth(160));
                        EditorGUILayout.LabelField((kv.Value.steps?.Count ?? 0).ToString(),               GUILayout.Width(50));
                        EditorGUILayout.LabelField(kv.Value.label ?? "",                                  GUILayout.MinWidth(100));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space(6);

            // ── Active Channels ──────────────────────────────────────────────────
            _showChannels = EditorGUILayout.Foldout(_showChannels, "Active Channels", true, EditorStyles.foldoutHeader);
            if (_showChannels)
            {
                var channels = mgr.GetActiveChannels();
                if (channels.Count == 0)
                {
                    EditorGUILayout.HelpBox("No active subscribers.", MessageType.None);
                }
                else
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Channel", EditorStyles.miniLabel, GUILayout.MinWidth(180));
                    EditorGUILayout.LabelField("Subscribers", EditorStyles.miniLabel, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();

                    foreach (var channel in channels)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(channel,                          GUILayout.MinWidth(180));
                        EditorGUILayout.LabelField(mgr.SubscriberCount(channel).ToString(), GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space(6);

            // ── Manual Fire ──────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Manual Fire", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            _fireEventName   = EditorGUILayout.TextField("Event Name",    _fireEventName);
            _fireStringValue = EditorGUILayout.TextField("String Value",  _fireStringValue);
            _fireIntValue    = EditorGUILayout.IntField("Int Value",      _fireIntValue);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_fireEventName);
            if (GUILayout.Button("Fire Event"))
            {
                var evt = new GameEvent(_fireEventName, _fireStringValue, _fireIntValue);
                mgr.Fire(evt);
            }
            GUI.enabled = true;
            if (GUILayout.Button("Clear History", GUILayout.Width(100)))
                mgr.ClearHistory();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            _fireSequenceId = EditorGUILayout.TextField("Sequence ID", _fireSequenceId);
            GUI.enabled = !string.IsNullOrEmpty(_fireSequenceId);
            if (GUILayout.Button("Fire Sequence"))
                mgr.FireSequence(_fireSequenceId);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // ── Event History ────────────────────────────────────────────────────
            var history = mgr.GetHistory();
            _showHistory = EditorGUILayout.Foldout(_showHistory,
                $"Recent Events ({history.Count})", true, EditorStyles.foldoutHeader);

            if (_showHistory)
            {
                if (history.Count == 0)
                {
                    EditorGUILayout.HelpBox("No events fired yet.", MessageType.None);
                }
                else
                {
                    _historyScroll = EditorGUILayout.BeginScrollView(_historyScroll,
                        GUILayout.MaxHeight(180));
                    EditorGUILayout.BeginVertical("box");

                    // Most recent at top
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        var e = history[i];
                        string detail = string.Empty;
                        if (!string.IsNullOrEmpty(e.stringValue)) detail += $"  str=\"{e.stringValue}\"";
                        if (e.intValue   != 0) detail += $"  int={e.intValue}";
                        if (e.floatValue != 0) detail += $"  float={e.floatValue}";
                        EditorGUILayout.LabelField($"{e.name}{detail}", EditorStyles.miniLabel);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                }
            }

            Repaint();
        }
    }
}
#endif
