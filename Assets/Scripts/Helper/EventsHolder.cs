using System;

namespace Helper
{
    public class EventsHolder
    {
        public event Action<int, int> CameraPositionChangedEvent = delegate { };

        public void RaiseCameraChunkPositionChangedEvent(int x, int z)
        {
            CameraPositionChangedEvent(x, z);
        }
    }
}

