using Terrain;

namespace Helper
{
    public class ServiceLocatorInstance : IServiceLocator
    {
        public TerrainController TerrainController { get; private set; }
        public TerrainData TerrainData { get; private set; }
        public EventsHolder EventsHolder { get; private set; }

        public ServiceLocatorInstance()
        {
            EventsHolder = new EventsHolder();
        }

        public void SetTerrainController(TerrainController terrainController)
        {
            TerrainController = terrainController;
        }

        public void SetTerrainData(TerrainData terrainData)
        {
            TerrainData = terrainData;
        }
    }
}

