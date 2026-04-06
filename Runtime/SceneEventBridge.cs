#if EVENTMANAGER_SCM
using UnityEngine;
using SceneManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and SceneManager.
    /// Enable define <c>EVENTMANAGER_SCM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when SceneManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"scene.loading"</c>  — <see cref="GameEvent.stringValue"/> = destination scene id</item>
    ///   <item><c>"scene.loaded"</c>   — <see cref="GameEvent.stringValue"/> = scene id</item>
    ///   <item><c>"scene.unloaded"</c> — <see cref="GameEvent.stringValue"/> = scene id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Scene Event Bridge")]
    [DisallowMultipleComponent]
    public class SceneEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a scene transition begins.")]
        [SerializeField] private string loadingEventName  = "scene.loading";

        [Tooltip("Event name fired when a scene has finished loading.")]
        [SerializeField] private string loadedEventName   = "scene.loaded";

        [Tooltip("Event name fired when a scene has been unloaded.")]
        [SerializeField] private string unloadedEventName = "scene.unloaded";

        private EventManager              _events;
        private SceneManager.Runtime.SceneManager _scenes;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _scenes = GetComponent<SceneManager.Runtime.SceneManager>()
                      ?? FindFirstObjectByType<SceneManager.Runtime.SceneManager>();

            if (_events == null) Debug.LogWarning("[SceneEventBridge] EventManager not found.");
            if (_scenes == null) Debug.LogWarning("[SceneEventBridge] SceneManager not found.");
        }

        private void OnEnable()
        {
            if (_scenes != null)
            {
                _scenes.OnSceneLoading  += OnSceneLoading;
                _scenes.OnSceneLoaded   += OnSceneLoaded;
                _scenes.OnSceneUnloaded += OnSceneUnloaded;
            }
        }

        private void OnDisable()
        {
            if (_scenes != null)
            {
                _scenes.OnSceneLoading  -= OnSceneLoading;
                _scenes.OnSceneLoaded   -= OnSceneLoaded;
                _scenes.OnSceneUnloaded -= OnSceneUnloaded;
            }
        }

        private void OnSceneLoading(SceneTransitionData data)
            => _events?.Fire(new GameEvent(loadingEventName, data.toSceneId));

        private void OnSceneLoaded(string sceneId)
            => _events?.Fire(new GameEvent(loadedEventName, sceneId));

        private void OnSceneUnloaded(string sceneId)
            => _events?.Fire(new GameEvent(unloadedEventName, sceneId));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — define <c>EVENTMANAGER_SCM</c> to activate this bridge.</summary>
    public class SceneEventBridge : UnityEngine.MonoBehaviour { }
}
#endif
