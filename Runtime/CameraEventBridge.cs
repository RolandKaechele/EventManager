#if EVENTMANAGER_CAM
using UnityEngine;
using CameraManager.Runtime;

namespace EventManager.Runtime
{
    /// <summary>
    /// Optional bridge between EventManager and CameraManager.
    /// Enable define <c>EVENTMANAGER_CAM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when CameraManager raises its own events:
    /// <list type="bullet">
    ///   <item><c>"camera.changed"</c> — <see cref="GameEvent.stringValue"/> = new camera profile id</item>
    ///   <item><c>"camera.pushed"</c>  — <see cref="GameEvent.stringValue"/> = pushed profile id</item>
    ///   <item><c>"camera.popped"</c>  — <see cref="GameEvent.stringValue"/> = popped profile id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("EventManager/Camera Event Bridge")]
    [DisallowMultipleComponent]
    public class CameraEventBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on camera set.")]
        [SerializeField] private string changedEventName = "camera.changed";

        [Tooltip("Event name fired when a camera is pushed.")]
        [SerializeField] private string pushedEventName = "camera.pushed";

        [Tooltip("Event name fired when a camera is popped.")]
        [SerializeField] private string poppedEventName = "camera.popped";

        private EventManager _events;
        private CameraManager.Runtime.CameraManager _cam;

        private void Awake()
        {
            _events = GetComponent<EventManager>() ?? FindFirstObjectByType<EventManager>();
            _cam    = GetComponent<CameraManager.Runtime.CameraManager>()
                      ?? FindFirstObjectByType<CameraManager.Runtime.CameraManager>();

            if (_events == null) Debug.LogWarning("[CameraEventBridge] EventManager not found.");
            if (_cam    == null) Debug.LogWarning("[CameraEventBridge] CameraManager not found.");
        }

        private void OnEnable()
        {
            if (_cam != null)
            {
                _cam.OnCameraChanged += OnCameraChanged;
                _cam.OnCameraPushed  += OnCameraPushed;
                _cam.OnCameraPopped  += OnCameraPopped;
            }
        }

        private void OnDisable()
        {
            if (_cam != null)
            {
                _cam.OnCameraChanged -= OnCameraChanged;
                _cam.OnCameraPushed  -= OnCameraPushed;
                _cam.OnCameraPopped  -= OnCameraPopped;
            }
        }

        private void OnCameraChanged(string previousId, string newId) =>
            _events?.Fire(new GameEvent(changedEventName, newId));

        private void OnCameraPushed(string id) =>
            _events?.Fire(new GameEvent(pushedEventName, id));

        private void OnCameraPopped(string id) =>
            _events?.Fire(new GameEvent(poppedEventName, id));
    }
}
#else
namespace EventManager.Runtime
{
    /// <summary>No-op stub — enable define <c>EVENTMANAGER_CAM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("EventManager/Camera Event Bridge")]
    public class CameraEventBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[CameraEventBridge] Bridge disabled — add EVENTMANAGER_CAM to Scripting Define Symbols.");
    }
}
#endif
