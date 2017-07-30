using Terrain;

namespace Helper
{
    public interface IServiceLocator
    {

        TerrainController TerrainController { get; }
        TerrainData TerrainData { get; }
        EventsHolder EventsHolder { get; }

        void SetTerrainController(TerrainController terrainController);
        void SetTerrainData(TerrainData terrainData);
    }
}

