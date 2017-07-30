using UnityEngine;

namespace Helper
{
    [RequireComponent(typeof(Camera))]
    public class CameraPositionTracker: MonoBehaviour
    {
        private Transform _transform;
        private int _lastX;
        private int _lastZ;

        private void Awake()
        {
            _transform = transform;
        }

        private void LateUpdate()
        {
            //TODO add margin
            var x = Mathf.FloorToInt(_transform.position.x / Consts.UnitSize);
            var z = Mathf.FloorToInt(_transform.position.z / Consts.UnitSize);
            if (x == _lastX && z == _lastZ) return;
            _lastX = x;
            _lastZ = z;
            ServiceLocator.I.EventsHolder.RaiseCameraChunkPositionChangedEvent(x, z);
        }
    }
}
