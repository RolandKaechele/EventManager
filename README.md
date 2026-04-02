# EventManager

A standalone Unity package providing a global named-channel event bus. Any system can fire or subscribe to typed `GameEvent`s without holding a reference to the sender — fully decoupled. Optionally integrates with MapLoaderFramework and CutsceneManager.

## Features

- Fire named events from anywhere: `Fire("map.loaded")`, `Fire("player.died", "sector_alpha")`
- Subscribe with `On` / unsubscribe with `Off` — no delegate juggling
- One-shot subscriptions via `Once` — auto-removed after first call
- Typed `GameEvent` payload: `stringValue`, `intValue`, `floatValue`, `objectValue`
- Per-handler exception isolation — one bad handler never silences others
- Event history log with configurable capacity (Inspector-visible at runtime)
- `EventTrigger` component for scene pickups, zone entry/exit, lifecycle events — zero code
- **Optional** MapLoaderFramework bridge — fires `map.loaded` and `chapter.changed`
- **Optional** CutsceneManager bridge — fires `cutscene.started`, `cutscene.completed`, `cutscene.skipped`, and custom cutscene events
- **Optional** SaveManager bridge — fires `save.saved`, `save.loaded`, `save.deleted`, and `flag.changed` from SaveManager operations (activated via `EVENTMANAGER_SM`)
- **Optional** DialogueManager bridge — fires `dialogue.started`, `dialogue.completed`, and `dialogue.node` from DialogueManager (activated via `EVENTMANAGER_DM`)
- **Optional** InventoryManager bridge — fires `item.added`, `item.removed`, and `item.used` from InventoryManager (activated via `EVENTMANAGER_IM`)
- **Optional** LocalizationManager bridge — fires `language.changed` when the active language switches (activated via `EVENTMANAGER_LM`)


## Installation

### A — Unity Package Manager (Git URL)

```
https://github.com/rolandkaechele/com.rolandkaechele.eventmanager.git
```

### B — Local disk

Place the `EventManager/` folder anywhere under your project's `Assets/` directory.

### C — npm / postinstall

```bash
npm install
```

`postinstall.js` creates the required runtime folders and optionally copies example files.


## Folder Structure

```
EventManager/
├── Runtime/
│   ├── EventData.cs               # GameEvent data class
│   ├── EventManager.cs            # Global event bus (MonoBehaviour)
│   ├── EventTrigger.cs            # Scene trigger component
│   ├── MapLoaderEventBridge.cs    # Optional: MLF integration
│   ├── CutsceneEventBridge.cs     # Optional: CutsceneManager integration
│   ├── SaveEventBridge.cs         # Optional: SaveManager integration
│   ├── DialogueEventBridge.cs     # Optional: DialogueManager integration
│   ├── InventoryEventBridge.cs    # Optional: InventoryManager integration
│   └── LocalizationEventBridge.cs # Optional: LocalizationManager integration
├── Editor/
│   └── EventManagerEditor.cs      # Custom inspector
├── Examples/
│   └── Scripts/
│       └── example_event_listener.lua
├── package.json
├── postinstall.js
├── LICENSE
└── README.md
```


## Quick Start

### 1. Scene Setup

Add `EventManager` to a persistent manager GameObject.

### 2. Subscribe from Code

```csharp
var events = FindFirstObjectByType<EventManager.Runtime.EventManager>();

// Persistent subscription
events.On("player.died", evt => Debug.Log($"Player died at {evt.stringValue}"));

// One-shot — fires once then auto-removes
events.Once("chapter.changed", evt => Debug.Log($"Chapter {evt.intValue} started!"));
```

### 3. Fire from Code

```csharp
// Simple name-only event
events.Fire("game.started");

// With string payload
events.Fire("map.loaded", "station_alpha");

// With int payload
events.Fire("score.changed", 250);

// Full GameEvent with multiple fields
events.Fire(new GameEvent("item.picked_up", "key_reactor_room", 1));
```

### 4. Unsubscribe

```csharp
void OnDied(GameEvent evt) { /* ... */ }

events.On("player.died",  OnDied);
events.Off("player.died", OnDied);
```

### 5. EventTrigger Component

Add `EventTrigger` to any scene object to fire an event without code:

| Field | Description |
| ----- | ----------- |
| `Event Name` | Event to fire |
| `String Payload` | Optional string value |
| `Int Payload` | Optional integer value |
| `Trigger Mode` | `OnStart`, `OnEnable`, `OnDisable`, `OnDestroy`, `OnTriggerEnter`, `OnTriggerExit`, `OnInteract` |
| `Fire Once` | Only fire once per scene lifetime |
| `Trigger Tag` | Collider tag filter (default: `"Player"`) |

Call `eventTrigger.Interact()` from code to fire an `OnInteract`-mode trigger.


## GameEvent Fields

| Field | Type | Description |
| ----- | ---- | ----------- |
| `name` | string | Channel name — used for routing |
| `stringValue` | string | Map id, item id, sequence id, etc. |
| `intValue` | int | Chapter number, quantity, score delta, etc. |
| `floatValue` | float | Damage, time, distance, etc. |
| `objectValue` | object | Any reference — not serialized, runtime only |


## MapLoaderFramework Integration

Enable `EVENTMANAGER_MLF` in Player Settings › Scripting Define Symbols.

Add `MapLoaderEventBridge` to the same GameObject as `EventManager` and `MapLoaderFramework`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"map.loaded"` | `OnMapLoaded` | `stringValue` = map id |
| `"chapter.changed"` | `OnChapterChanged` | `stringValue` = previous chapter, `intValue` = new chapter |

### Inspector Fields

| Field | Default | Description |
| ----- | ---- | ----------- |
| `Map Loaded Event Name` | `"map.loaded"` | Configurable event name |
| `Chapter Changed Event Name` | `"chapter.changed"` | Configurable event name |


## CutsceneManager Integration

Enable `EVENTMANAGER_CSM` in Player Settings › Scripting Define Symbols.

Add `CutsceneEventBridge` to the same GameObject as `EventManager` and `CutsceneManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"cutscene.started"` | `OnSequenceStarted` | `stringValue` = sequence id |
| `"cutscene.completed"` | `OnSequenceCompleted` | `stringValue` = sequence id |
| `"cutscene.skipped"` | `OnSequenceSkipped` | `stringValue` = sequence id |
| *custom key* | `OnCustomEvent` | `stringValue` = sequence id (when `forwardCustomEvents` is true, the cutscene's custom event key becomes the event name) |

### Inspector Fields

| Field | Default | Description |
| ----- | ---- | ----------- |
| `Cutscene Started Event Name` | `"cutscene.started"` | Configurable |
| `Cutscene Completed Event Name` | `"cutscene.completed"` | Configurable |
| `Cutscene Skipped Event Name` | `"cutscene.skipped"` | Configurable |
| `Forward Custom Events` | `true` | Re-broadcast CutsceneManager custom events using their key as the event name |


## SaveManager Integration

Enable `EVENTMANAGER_SM` in Player Settings › Scripting Define Symbols.

Add `SaveEventBridge` to the same GameObject as `EventManager` and `SaveManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"save.saved"` | `OnSaved` | `intValue` = slot index |
| `"save.loaded"` | `OnLoaded` | `intValue` = slot index |
| `"save.deleted"` | `OnDeleted` | `intValue` = slot index |
| `"flag.changed"` | `OnFlagChanged` | `stringValue` = flag name, `intValue` = 1 (set) or 0 (unset) |

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `Saved Event Name` | `"save.saved"` | Configurable event name |
| `Loaded Event Name` | `"save.loaded"` | Configurable event name |
| `Deleted Event Name` | `"save.deleted"` | Configurable event name |
| `Flag Changed Event Name` | `"flag.changed"` | Configurable event name |


## DialogueManager Integration

Enable `EVENTMANAGER_DM` in Player Settings › Scripting Define Symbols.

Add `DialogueEventBridge` to the same GameObject as `EventManager` and `DialogueManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"dialogue.started"` | `OnDialogueStarted` | `stringValue` = sequence id |
| `"dialogue.completed"` | `OnDialogueCompleted` | `stringValue` = sequence id |
| `"dialogue.node"` | `OnNodeShown` | `stringValue` = sequence id, `objectValue` = node id |

Set `nodeEventName` to an empty string in the Inspector to disable per-node events.

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `Started Event Name` | `"dialogue.started"` | Configurable event name |
| `Completed Event Name` | `"dialogue.completed"` | Configurable event name |
| `Node Event Name` | `"dialogue.node"` | Configurable; leave empty to disable node events |


## InventoryManager Integration

Enable `EVENTMANAGER_IM` in Player Settings › Scripting Define Symbols.

Add `InventoryEventBridge` to the same GameObject as `EventManager` and `InventoryManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"item.added"` | `OnItemAdded` | `stringValue` = item id, `intValue` = quantity |
| `"item.removed"` | `OnItemRemoved` | `stringValue` = item id, `intValue` = quantity |
| `"item.used"` | `OnItemUsed` | `stringValue` = item id |

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `Added Event Name` | `"item.added"` | Configurable event name |
| `Removed Event Name` | `"item.removed"` | Configurable event name |
| `Used Event Name` | `"item.used"` | Configurable event name |


## LocalizationManager Integration

Enable `EVENTMANAGER_LM` in Player Settings › Scripting Define Symbols.

Add `LocalizationEventBridge` to the same GameObject as `EventManager` and `LocalizationManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"language.changed"` | `OnLanguageChanged` | `stringValue` = new language code |

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `Language Changed Event Name` | `"language.changed"` | Configurable event name |


## Runtime API

### EventManager

| Member | Description |
| ------ | ----------- |
| `Fire(GameEvent evt)` | Fire a fully constructed event |
| `Fire(string name)` | Fire a name-only event |
| `Fire(string name, string str)` | Fire with string payload |
| `Fire(string name, int intVal)` | Fire with int payload |
| `Fire(string name, float floatVal)` | Fire with float payload |
| `On(string name, Action<GameEvent>)` | Persistent subscription |
| `Off(string name, Action<GameEvent>)` | Unsubscribe |
| `Once(string name, Action<GameEvent>)` | One-shot subscription |
| `ClearChannel(string name)` | Remove all subscribers for one channel |
| `ClearAll()` | Remove all subscribers everywhere |
| `SubscriberCount(string name) → int` | Number of persistent subscribers |
| `GetHistory() → IReadOnlyList<GameEvent>` | Recent event log |
| `GetActiveChannels()` | All channels with at least one subscriber |
| `ClearHistory()` | Empty the history log |


## Integration Defines Summary

| Define | Effect |
| ------ | ------ |
| `EVENTMANAGER_MLF` | EventManager fires `map.loaded` / `chapter.changed` via MapLoaderFramework |
| `EVENTMANAGER_CSM` | EventManager forwards CutsceneManager sequence events |
| `EVENTMANAGER_SM` | EventManager fires save/load/delete/flag events from SaveManager |
| `EVENTMANAGER_DM` | EventManager fires dialogue lifecycle events from DialogueManager |
| `EVENTMANAGER_IM` | EventManager fires item added/removed/used events from InventoryManager |
| `EVENTMANAGER_LM` | EventManager fires `language.changed` from LocalizationManager |


## Examples

See `Examples/Scripts/example_event_listener.lua` for subscribing, firing, and one-shot patterns.


## Dependencies

| Dependency | Role |
| ---------- | ---- |
| Unity 2022.3+ | Required |
| MapLoaderFramework | Optional — enable `EVENTMANAGER_MLF` |
| CutsceneManager | Optional — enable `EVENTMANAGER_CSM` |
| SaveManager | Optional — enable `EVENTMANAGER_SM` |
| DialogueManager | Optional — enable `EVENTMANAGER_DM` |
| InventoryManager | Optional — enable `EVENTMANAGER_IM` |
| LocalizationManager | Optional — enable `EVENTMANAGER_LM` |


## Repository

`https://github.com/RolandKaechele/EventManager`


## License

MIT — see [LICENSE](LICENSE)
