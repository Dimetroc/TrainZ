using System;
using Helper;
using UnityEngine;

namespace Terrain
{
    public class ChunkLod
    {
        private int _currentLod = -1;
        private int[] _currentNeighbors;
        private readonly int _chunkX;
        private readonly int _chunkZ;
        private readonly TerrainChunkDataProcessor _chunkDataProcessor;

        public ChunkLod(TerrainChunkData data)
        {
            _chunkDataProcessor = new TerrainChunkDataProcessor(data);
            _chunkX = Mathf.FloorToInt(data.Position.x / Consts.UnitSize);
            _chunkZ = Mathf.FloorToInt(data.Position.z / Consts.UnitSize);
            _currentNeighbors = new int [] {-1,-1,-1,-1};
        }

        public bool IsThereNewData(int cx, int cz)
        {
            var newLod = Assistant.GetUnitLod(_chunkX, _chunkZ, cx, cz);
            var newNeighbors = Assistant.GetNeighborsLods(_chunkX, _chunkZ, cx, cz);
            if (!NeedUpdate(newLod, newNeighbors)) return false;
            _currentLod = newLod;
            _currentNeighbors = newNeighbors;
            return true;
        }

        public TerrainChunkMeshData GetNewData()
        {
            return _chunkDataProcessor.GetMeshData(_currentLod, _currentNeighbors);
        }

        private bool NeedUpdate(int lod, int[] neighbors)
        {
            return lod != _currentLod || neighbors[0] != _currentNeighbors[0] || neighbors[1] != _currentNeighbors[1] || neighbors[2] != _currentNeighbors[2] || neighbors[3] != _currentNeighbors[3];
        }


    }
}
