#if EVENTMANAGER_CHR
using UnityEngine;
using CharacterManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and CharacterManager.
    /// Enable define <c>EVENTMANAGER_CHR</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires named game events on the <see cref="EventManager"/> bus when CharacterManager raises
    /// its lifecycle events:
    /// <list type="bullet">
    /// <item><c>"character.unlocked"</c> – <see cref="GameEvent.stringValue"/> = character id</item>
    /// <item><c>"character.changed"</c>  – <see cref="GameEvent.stringValue"/> = new active character id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Character Event Bridge")]
    [DisallowMultipleComponent]
    public class CharacterEventBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [SerializeField] private string unlockedEventName = "character.unlocked";
        [SerializeField] private string changedEventName  = "character.changed";

        // ─── References ───────────────────────────────────────────────────────
        private EventManager _events;
        private CharacterManager.Runtime.CharacterManager _mgr;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _mgr    = GetComponent<CharacterManager.Runtime.CharacterManager>()
                      ?? FindFirstObjectByType<CharacterManager.Runtime.CharacterManager>();

            if (_events == null) Debug.LogWarning("[CharacterEventBridge] EventManager not found.");
            if (_mgr    == null) Debug.LogWarning("[CharacterEventBridge] CharacterManager not found.");
        }

        private void OnEnable()
        {
            if (_mgr != null)
            {
                _mgr.OnCharacterUnlocked      += OnUnlocked;
                _mgr.OnActiveCharacterChanged += OnChanged;
            }
        }

        private void OnDisable()
        {
            if (_mgr != null)
            {
                _mgr.OnCharacterUnlocked      -= OnUnlocked;
                _mgr.OnActiveCharacterChanged -= OnChanged;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnUnlocked(string id) =>
            _events?.Fire(new GameEvent(unlockedEventName, id));

        private void OnChanged(string id) =>
            _events?.Fire(new GameEvent(changedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_CHR in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Character Event Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class CharacterEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
