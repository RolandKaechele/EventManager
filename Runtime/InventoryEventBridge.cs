#if EVENTMANAGER_IM
using UnityEngine;
using InventoryManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and InventoryManager.
    /// Enable define <c>EVENTMANAGER_IM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when InventoryManager raises its own events:
    /// <list type="bullet">
    /// <item><c>"item.added"</c>   — <see cref="GameEvent.stringValue"/> = item id,
    ///       <see cref="GameEvent.intValue"/> = quantity added</item>
    /// <item><c>"item.removed"</c> — <see cref="GameEvent.stringValue"/> = item id,
    ///       <see cref="GameEvent.intValue"/> = quantity removed</item>
    /// <item><c>"item.used"</c>    — <see cref="GameEvent.stringValue"/> = item id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Inventory Event Bridge")]
    [DisallowMultipleComponent]
    public class InventoryEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when items are added to inventory.")]
        [SerializeField] private string addedEventName   = "item.added";

        [Tooltip("Event name fired when items are removed from inventory.")]
        [SerializeField] private string removedEventName = "item.removed";

        [Tooltip("Event name fired when an item is used.")]
        [SerializeField] private string usedEventName    = "item.used";

        private EventManager _events;
        private InventoryManager.Runtime.InventoryManager _inventory;

        private void Awake()
        {
            _events    = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _inventory = GetComponent<InventoryManager.Runtime.InventoryManager>()
                         ?? FindFirstObjectByType<InventoryManager.Runtime.InventoryManager>();

            if (_events    == null) Debug.LogWarning("[InventoryEventBridge] EventManager not found.");
            if (_inventory == null) Debug.LogWarning("[InventoryEventBridge] InventoryManager not found.");
        }

        private void OnEnable()
        {
            if (_inventory != null)
            {
                _inventory.OnItemAdded   += OnItemAdded;
                _inventory.OnItemRemoved += OnItemRemoved;
                _inventory.OnItemUsed    += OnItemUsed;
            }
        }

        private void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.OnItemAdded   -= OnItemAdded;
                _inventory.OnItemRemoved -= OnItemRemoved;
                _inventory.OnItemUsed    -= OnItemUsed;
            }
        }

        private void OnItemAdded(string itemId, int qty) =>
            _events?.Fire(new GameEvent(addedEventName, itemId, qty));

        private void OnItemRemoved(string itemId, int qty) =>
            _events?.Fire(new GameEvent(removedEventName, itemId, qty));

        private void OnItemUsed(string itemId) =>
            _events?.Fire(new GameEvent(usedEventName, itemId));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub. Enable EVENTMANAGER_IM in Player Settings to activate the bridge.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Inventory Event Bridge")]
    public class InventoryEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InventoryEventBridge] InventoryManager integration is disabled. " +
                                  "Add the scripting define EVENTMANAGER_IM to enable it.");
    }
}
#endif
