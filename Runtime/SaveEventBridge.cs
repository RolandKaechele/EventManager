#if EVENTMANAGER_SM
using UnityEngine;
using SaveManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and SaveManager.
    /// Enable define <c>EVENTMANAGER_SM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when SaveManager raises its own events:
    /// <list type="bullet">
    /// <item><c>"save.saved"</c>   — <see cref="GameEvent.intValue"/> = slot index</item>
    /// <item><c>"save.loaded"</c>  — <see cref="GameEvent.intValue"/> = slot index</item>
    /// <item><c>"save.deleted"</c> — <see cref="GameEvent.intValue"/> = slot index</item>
    /// <item><c>"flag.changed"</c> — <see cref="GameEvent.stringValue"/> = flag name,
    ///       <see cref="GameEvent.intValue"/> = 1 if set, 0 if unset</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Save Event Bridge")]
    [DisallowMultipleComponent]
    public class SaveEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a slot is saved.")]
        [SerializeField] private string savedEventName   = "save.saved";

        [Tooltip("Event name fired when a slot is loaded.")]
        [SerializeField] private string loadedEventName  = "save.loaded";

        [Tooltip("Event name fired when a slot is deleted.")]
        [SerializeField] private string deletedEventName = "save.deleted";

        [Tooltip("Event name fired when a game flag changes.")]
        [SerializeField] private string flagChangedEventName = "flag.changed";

        private EventManager _events;
        private SaveManager.Runtime.SaveManager _save;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _save   = GetComponent<SaveManager.Runtime.SaveManager>()
                      ?? FindFirstObjectByType<SaveManager.Runtime.SaveManager>();

            if (_events == null) Debug.LogWarning("[SaveEventBridge] EventManager not found.");
            if (_save   == null) Debug.LogWarning("[SaveEventBridge] SaveManager not found.");
        }

        private void OnEnable()
        {
            if (_save != null)
            {
                _save.OnSaved       += OnSaved;
                _save.OnLoaded      += OnLoaded;
                _save.OnDeleted     += OnDeleted;
                _save.OnFlagChanged += OnFlagChanged;
            }
        }

        private void OnDisable()
        {
            if (_save != null)
            {
                _save.OnSaved       -= OnSaved;
                _save.OnLoaded      -= OnLoaded;
                _save.OnDeleted     -= OnDeleted;
                _save.OnFlagChanged -= OnFlagChanged;
            }
        }

        private void OnSaved(int slot)   => _events?.Fire(new GameEvent(savedEventName,   slot));
        private void OnLoaded(int slot)  => _events?.Fire(new GameEvent(loadedEventName,  slot));
        private void OnDeleted(int slot) => _events?.Fire(new GameEvent(deletedEventName, slot));

        private void OnFlagChanged(string flag, bool isSet) =>
            _events?.Fire(new GameEvent(flagChangedEventName, flag, isSet ? 1 : 0));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_SM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Save Event Bridge")]
    public class SaveEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[SaveEventBridge] SaveManager integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_SM to enable it.");
    }
}
#endif
