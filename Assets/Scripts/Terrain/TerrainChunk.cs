using Helper;
using UnityEngine;

namespace Terrain
{
    public class TerrainChunk
    {
        private readonly TerrainVisualizer _terrainVisualizer;

        public TerrainChunk(TerrainChunkData data, Transform parent)
        {
            data.IsSeaBed = data.HeigthData.IsSeaBed();
            _terrainVisualizer = (new GameObject(string.Format("TC_{0}_{1}", data.Origin.X, data.Origin.Z))).AddComponent<TerrainVisualizer>();
            _terrainVisualizer.transform.SetParent(parent);
            _terrainVisualizer.transform.position = data.Position;
        }

        public void UpdateChunk(TerrainChunkMeshData data)
        {
            _terrainVisualizer.SetData(data);
        }

    }
}
