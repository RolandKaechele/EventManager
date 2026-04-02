-- example_event_listener.lua
-- Demonstrates subscribing to events, firing events, and one-shot listeners.
-- In a full integration this runs via a Lua host bridge.

-- Subscribe to a persistent event
EventManager.On("map.loaded", function(evt)
    Debug.Log("Map loaded: " .. tostring(evt.stringValue))
end)

-- Subscribe to a chapter change event
EventManager.On("chapter.changed", function(evt)
    local prev = evt.stringValue
    local next = evt.intValue
    Debug.Log("Chapter changed from " .. prev .. " to " .. tostring(next))
end)

-- One-shot: react only to the very first cutscene that completes
EventManager.Once("cutscene.completed", function(evt)
    Debug.Log("First cutscene completed: " .. tostring(evt.stringValue))
end)

-- React to a custom game event fired from code or an EventTrigger component
EventManager.On("player.died", function(evt)
    Debug.Log("Player died at " .. tostring(evt.stringValue))
end)

-- Fire a custom event from Lua
EventManager.Fire("player.died", "sector_alpha")

-- Fire with integer payload
EventManager.Fire("score.changed", 250)
