#if EVENTMANAGER_LM
using UnityEngine;
using LocalizationManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and LocalizationManager.
    /// Enable define <c>EVENTMANAGER_LM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named event when the active language changes:
    /// <list type="bullet">
    /// <item><c>"language.changed"</c> — <see cref="GameEvent.stringValue"/> = new language code</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Localization Event Bridge")]
    [DisallowMultipleComponent]
    public class LocalizationEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when the active language changes.")]
        [SerializeField] private string languageChangedEventName = "language.changed";

        private EventManager _events;
        private LocalizationManager.Runtime.LocalizationManager _localization;

        private void Awake()
        {
            _events       = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _localization = GetComponent<LocalizationManager.Runtime.LocalizationManager>()
                            ?? FindFirstObjectByType<LocalizationManager.Runtime.LocalizationManager>();

            if (_events       == null) Debug.LogWarning("[LocalizationEventBridge] EventManager not found.");
            if (_localization == null) Debug.LogWarning("[LocalizationEventBridge] LocalizationManager not found.");
        }

        private void OnEnable()
        {
            if (_localization != null) _localization.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            if (_localization != null) _localization.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(string languageCode) =>
            _events?.Fire(new GameEvent(languageChangedEventName, languageCode));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_LM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Localization Event Bridge")]
    public class LocalizationEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[LocalizationEventBridge] LocalizationManager integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_LM to enable it.");
    }
}
#endif
