#if EVENTMANAGER_UIM
using UnityEngine;
using UiManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and UiManager.
    /// Enable define <c>EVENTMANAGER_UIM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when UiManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"ui.panel.shown"</c>  — <see cref="GameEvent.stringValue"/> = panel id</item>
    ///   <item><c>"ui.panel.hidden"</c> — <see cref="GameEvent.stringValue"/> = panel id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/UI Event Bridge")]
    [DisallowMultipleComponent]
    public class UiEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a UI panel is shown.")]
        [SerializeField] private string shownEventName  = "ui.panel.shown";

        [Tooltip("Event name fired when a UI panel is hidden.")]
        [SerializeField] private string hiddenEventName = "ui.panel.hidden";

        private EventManager _events;
        private UiManager.Runtime.UiManager _ui;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _ui     = GetComponent<UiManager.Runtime.UiManager>()
                      ?? FindFirstObjectByType<UiManager.Runtime.UiManager>();

            if (_events == null) Debug.LogWarning("[UiEventBridge] EventManager not found.");
            if (_ui     == null) Debug.LogWarning("[UiEventBridge] UiManager not found.");
        }

        private void OnEnable()
        {
            if (_ui != null)
            {
                _ui.OnPanelShown  += OnPanelShown;
                _ui.OnPanelHidden += OnPanelHidden;
            }
        }

        private void OnDisable()
        {
            if (_ui != null)
            {
                _ui.OnPanelShown  -= OnPanelShown;
                _ui.OnPanelHidden -= OnPanelHidden;
            }
        }

        private void OnPanelShown(string id)  => _events?.Fire(new GameEvent(shownEventName,  id));
        private void OnPanelHidden(string id) => _events?.Fire(new GameEvent(hiddenEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — enable define <c>EVENTMANAGER_UIM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("EventManager/UI Event Bridge")]
    public class UiEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiEventBridge] Bridge disabled — add EVENTMANAGER_UIM to Scripting Define Symbols.");
    }
}
#endif
