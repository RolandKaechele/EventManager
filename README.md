# EventManager

A standalone Unity package providing a global named-channel event bus. Any system can fire or subscribe to typed `GameEvent`s without holding a reference to the sender — fully decoupled. Supports JSON-authored event definitions and timed event sequences. Optionally integrates with MapLoaderFramework, CutsceneManager, and more.

## Features

- Fire named events from anywhere: `Fire("map.loaded")`, `Fire("player.died", "sector_alpha")`
- Subscribe with `On` / unsubscribe with `Off` — no delegate juggling
- One-shot subscriptions via `Once` — auto-removed after first call
- Typed `GameEvent` payload: `stringValue`, `intValue`, `floatValue`, `objectValue`
- Per-handler exception isolation — one bad handler never silences others
- Event history log with configurable capacity (Inspector-visible at runtime)
- **JSON event definitions** — author named channels with default payloads, labels, descriptions and tags in `Resources/Events/*.json`
- **JSON event sequences** — chain timed event firings in `Resources/EventSequences/*.json`; fire sequences from code or `EventTrigger`
- Hot-reload from `persistentDataPath/Events/` and `persistentDataPath/EventSequences/` for mods / DLC
- `EventTrigger` component for scene pickups, zone entry/exit, lifecycle events — zero code; supports sequence mode
- **Optional** MapLoaderFramework bridge — fires `map.loaded` and `chapter.changed`
- **Optional** CutsceneManager bridge — fires `cutscene.started`, `cutscene.completed`, `cutscene.skipped`, and custom cutscene events
- **Optional** SaveManager bridge — fires `save.saved`, `save.loaded`, `save.deleted`, and `flag.changed` from SaveManager operations (activated via `EVENTMANAGER_SM`)
- **Optional** DialogueManager bridge — fires `dialogue.started`, `dialogue.completed`, and `dialogue.node` from DialogueManager (activated via `EVENTMANAGER_DM`)
- **Optional** InventoryManager bridge — fires `item.added`, `item.removed`, and `item.used` from InventoryManager (activated via `EVENTMANAGER_IM`)
- **Optional** MiniGameManager bridge — fires `minigame.started`, `minigame.completed`, and `minigame.aborted` from MiniGameManager (activated via `EVENTMANAGER_MGM`)
- **Optional** DlcManager bridge — fires `dlc.unlocked` and `dlc.revoked` from DlcManager (activated via `EVENTMANAGER_DLC`)
- **Optional** LocalizationManager bridge — fires `language.changed` when the active language switches (activated via `EVENTMANAGER_LM`)
- **Optional** StateManager bridge — fires `state.changed`, `state.pushed`, `state.popped` whenever StateManager transitions (activated via `EVENTMANAGER_STM`)
- **Optional** AnimationManager bridge — fires `animation.started`, `animation.stopped`, `animation.completed` from AnimationManager (activated via `EVENTMANAGER_ANM`)
- **Optional** UiManager bridge — fires `ui.panel.shown`, `ui.panel.hidden` from UiManager (activated via `EVENTMANAGER_UIM`)
- **Optional** InputManager bridge — fires `input.profileChanged`, `input.blocked`, `input.unblocked` from InputManager (activated via `EVENTMANAGER_INP`)
- **Optional** CameraManager bridge — fires `camera.changed`, `camera.pushed`, `camera.popped` from CameraManager (activated via `EVENTMANAGER_CAM`)


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
│   ├── EventData.cs               # GameEvent, EventDefinitionData, EventSequenceData
│   ├── EventManager.cs            # Global event bus (MonoBehaviour) + JSON loader
│   ├── EventTrigger.cs            # Scene trigger component (event + sequence mode)
│   ├── MapLoaderEventBridge.cs    # Optional: MLF integration
│   ├── CutsceneEventBridge.cs     # Optional: CutsceneManager integration
│   ├── SaveEventBridge.cs         # Optional: SaveManager integration
│   ├── DialogueEventBridge.cs     # Optional: DialogueManager integration
│   ├── InventoryEventBridge.cs    # Optional: InventoryManager integration
│   ├── MiniGameEventBridge.cs     # Optional: MiniGameManager integration
│   └── DlcEventBridge.cs          # Optional: DlcManager integration
│   └── LocalizationEventBridge.cs # Optional: LocalizationManager integration
│   └── StateEventBridge.cs        # Optional: StateManager integration
│   └── AnimationEventBridge.cs    # Optional: AnimationManager integration
│   └── UiEventBridge.cs           # Optional: UiManager integration
│   └── InputEventBridge.cs        # Optional: InputManager integration
│   └── CameraEventBridge.cs       # Optional: CameraManager integration
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

Add `EventTrigger` to any scene object to fire an event or sequence without code:

| Field | Description |
| ----- | ----------- |
| `Event Name` | Event to fire (leave empty when using Sequence mode) |
| `Sequence Id` | Sequence to fire instead of a single event |
| `String Payload` | Optional string value (ignored in Sequence mode) |
| `Int Payload` | Optional integer value (ignored in Sequence mode) |
| `Trigger Mode` | `OnStart`, `OnEnable`, `OnDisable`, `OnDestroy`, `OnTriggerEnter`, `OnTriggerExit`, `OnInteract` |
| `Fire Once` | Only fire once per scene lifetime |
| `Trigger Tag` | Collider tag filter (default: `"Player"`) |

Call `eventTrigger.Interact()` from code to fire an `OnInteract`-mode trigger.

### 6. JSON Event Definitions

Place one definition per `.json` file in `Assets/Resources/Events/`:

```json
{
  "id": "player.died",
  "label": "Player Died",
  "description": "Fired when the player loses all HP.",
  "defaultStringValue": "",
  "defaultIntValue": 0,
  "defaultFloatValue": 0,
  "tags": ["player", "combat"]
}
```

Fire using defaults defined in JSON:

```csharp
events.FireWithDefaults("player.died");
```

### 7. JSON Event Sequences

Place one sequence per `.json` file in `Assets/Resources/EventSequences/`:

```json
{
  "id": "chapter.start",
  "label": "Chapter Start",
  "steps": [
    { "eventName": "screen.fade.in",  "delayBefore": 0.0 },
    { "eventName": "music.start",     "stringValue": "chapter_theme", "delayBefore": 0.5 },
    { "eventName": "chapter.started", "intValue": 1, "delayBefore": 1.0 }
  ]
}
```

Fire a sequence from code:

```csharp
events.FireSequence("chapter.start");
events.StopSequence();
```

Or set `Sequence Id` on an `EventTrigger` component to fire it from the scene without code.


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


## MiniGameManager Integration

Enable `EVENTMANAGER_MGM` in Player Settings › Scripting Define Symbols.

Add `MiniGameEventBridge` to the same GameObject as `EventManager` and `MiniGameManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"minigame.started"` | `OnMiniGameStarted` | `stringValue` = mini-game id |
| `"minigame.completed"` | `OnMiniGameCompleted` | `stringValue` = mini-game id, `intValue` = score, `floatValue` = timestamp |
| `"minigame.aborted"` | `OnMiniGameAborted` | `stringValue` = mini-game id |

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `Started Event Name` | `"minigame.started"` | Configurable event name |
| `Completed Event Name` | `"minigame.completed"` | Configurable event name |
| `Aborted Event Name` | `"minigame.aborted"` | Configurable event name |


## DlcManager Integration

Enable `EVENTMANAGER_DLC` in Player Settings › Scripting Define Symbols.

Add `DlcEventBridge` to the same GameObject as `EventManager` and `DlcManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"dlc.unlocked"` | `OnPackUnlocked` | `stringValue` = pack id |
| `"dlc.revoked"` | `OnPackRevoked` | `stringValue` = pack id |


## StateManager Integration

Enable `EVENTMANAGER_STM` in Player Settings › Scripting Define Symbols.

Add `StateEventBridge` to the same GameObject as `EventManager` and `StateManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"state.changed"` | `OnStateChanged` | `stringValue` = new state name |
| `"state.pushed"` | `OnStatePushed` | `stringValue` = pushed state name |
| `"state.popped"` | `OnStatePopped` | `stringValue` = popped state name |


## AnimationManager Integration

Enable `EVENTMANAGER_ANM` in Player Settings › Scripting Define Symbols.

Add `AnimationEventBridge` to the same GameObject as `EventManager` and `AnimationManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"animation.started"` | `OnAnimationStarted` | `stringValue` = animation id |
| `"animation.stopped"` | `OnAnimationStopped` | `stringValue` = animation id |
| `"animation.completed"` | `OnAnimationCompleted` | `stringValue` = animation id |


## UiManager Integration

Enable `EVENTMANAGER_UIM` in Player Settings › Scripting Define Symbols.

Add `UiEventBridge` to the same GameObject as `EventManager` and `UiManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"ui.panel.shown"` | `OnPanelShown` | `stringValue` = panel id |
| `"ui.panel.hidden"` | `OnPanelHidden` | `stringValue` = panel id |


## InputManager Integration

Enable `EVENTMANAGER_INP` in Player Settings › Scripting Define Symbols.

Add `InputEventBridge` to the same GameObject as `EventManager` and `InputManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"input.profileChanged"` | `OnProfileChanged` | `stringValue` = new profile id |
| `"input.blocked"` | `OnInputBlocked` | *(no payload)* |
| `"input.unblocked"` | `OnInputUnblocked` | *(no payload)* |


## CameraManager Integration

Enable `EVENTMANAGER_CAM` in Player Settings › Scripting Define Symbols.

Add `CameraEventBridge` to the same GameObject as `EventManager` and `CameraManager`.

| Event Fired | Trigger | Payload |
| ----------- | ------- | ------- |
| `"camera.changed"` | `OnCameraChanged` | `stringValue` = new camera id |
| `"camera.pushed"` | `OnCameraPushed` | `stringValue` = pushed camera id |
| `"camera.popped"` | `OnCameraPopped` | `stringValue` = popped camera id |


## Runtime API

### EventManager

#### Event Bus

| Member | Description |
| ------ | ----------- |
| `Fire(GameEvent evt)` | Fire a fully constructed event |
| `Fire(string name)` | Fire a name-only event |
| `Fire(string name, string str)` | Fire with string payload |
| `Fire(string name, int intVal)` | Fire with int payload |
| `Fire(string name, float floatVal)` | Fire with float payload |
| `FireWithDefaults(string name)` | Fire using default payloads from the loaded definition |
| `On(string name, Action<GameEvent>)` | Persistent subscription |
| `Off(string name, Action<GameEvent>)` | Unsubscribe |
| `Once(string name, Action<GameEvent>)` | One-shot subscription |
| `ClearChannel(string name)` | Remove all subscribers for one channel |
| `ClearAll()` | Remove all subscribers everywhere |
| `SubscriberCount(string name) → int` | Number of persistent subscribers |
| `GetHistory() → IReadOnlyList<GameEvent>` | Recent event log |
| `GetActiveChannels()` | All channels with at least one subscriber |
| `ClearHistory()` | Empty the history log |

#### JSON Loading

| Member | Description |
| ------ | ----------- |
| `LoadAllDefinitions()` | Reload definitions from `Resources/Events/` and `persistentDataPath/Events/` |
| `LoadAllSequences()` | Reload sequences from `Resources/EventSequences/` and `persistentDataPath/EventSequences/` |
| `GetDefinition(string id) → EventDefinitionData` | Look up a loaded definition by id |
| `GetAllDefinitions()` | All loaded event definitions |
| `GetSequence(string id) → EventSequenceData` | Look up a loaded sequence by id |
| `GetAllSequences()` | All loaded event sequences |

#### Sequences

| Member | Description |
| ------ | ----------- |
| `FireSequence(string id)` | Fire a named sequence by id |
| `FireSequence(EventSequenceData)` | Fire a sequence instance directly |
| `StopSequence()` | Stop the currently running sequence |
| `OnSequenceStarted` | Event fired when a sequence begins (sequence id) |
| `OnSequenceCompleted` | Event fired when a sequence finishes (sequence id) |

### EventDefinitionData (JSON fields)

| Field | Type | Description |
| ----- | ---- | ----------- |
| `id` | string | Unique channel name |
| `label` | string | Human-readable label |
| `description` | string | When/why this event fires |
| `defaultStringValue` | string | Default string payload for `FireWithDefaults` |
| `defaultIntValue` | int | Default int payload |
| `defaultFloatValue` | float | Default float payload |
| `tags` | string[] | Optional grouping tags |

### EventSequenceData (JSON fields)

| Field | Type | Description |
| ----- | ---- | ----------- |
| `id` | string | Unique sequence identifier |
| `label` | string | Human-readable label |
| `steps` | EventSequenceStep[] | Ordered list of steps |

### EventSequenceStep (JSON fields)

| Field | Type | Description |
| ----- | ---- | ----------- |
| `eventName` | string | Event to fire |
| `stringValue` | string | String payload |
| `intValue` | int | Integer payload |
| `floatValue` | float | Float payload |
| `delayBefore` | float | Seconds to wait before firing this step |


## Integration Defines Summary

| Define | Effect |
| ------ | ------ |
| `EVENTMANAGER_MLF` | EventManager fires `map.loaded` / `chapter.changed` via MapLoaderFramework |
| `EVENTMANAGER_CSM` | EventManager forwards CutsceneManager sequence events |
| `EVENTMANAGER_SM` | EventManager fires save/load/delete/flag events from SaveManager |
| `EVENTMANAGER_DM` | EventManager fires dialogue lifecycle events from DialogueManager |
| `EVENTMANAGER_IM` | EventManager fires item added/removed/used events from InventoryManager |
| `EVENTMANAGER_LM` | EventManager fires `language.changed` from LocalizationManager |
| `EVENTMANAGER_MGM` | EventManager fires mini-game started/completed/aborted events from MiniGameManager |
| `EVENTMANAGER_DLC` | EventManager fires DLC pack unlocked/revoked events from DlcManager |
| `EVENTMANAGER_STM` | EventManager fires `state.changed/pushed/popped` from StateManager |
| `EVENTMANAGER_ANM` | EventManager fires `animation.started/stopped/completed` from AnimationManager |
| `EVENTMANAGER_UIM` | EventManager fires `ui.panel.shown/hidden` from UiManager |
| `EVENTMANAGER_INP` | EventManager fires `input.profileChanged/blocked/unblocked` from InputManager |
| `EVENTMANAGER_CAM` | EventManager fires `camera.changed/pushed/popped` from CameraManager |


## JSON File Locations

| Content | Bundled path | Hot-reload / mod path |
| ------- | ------------ | --------------------- |
| Event definitions | `Assets/Resources/Events/*.json` | `persistentDataPath/Events/*.json` |
| Event sequences | `Assets/Resources/EventSequences/*.json` | `persistentDataPath/EventSequences/*.json` |

Toggle hot-reload with the **Load From Persistent Data Path** checkbox on the `EventManager` component.


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
| MiniGameManager | Optional — enable `EVENTMANAGER_MGM` |
| DlcManager | Optional — enable `EVENTMANAGER_DLC` |


## Repository

`https://github.com/RolandKaechele/EventManager`


## License

MIT — see [LICENSE](LICENSE)
