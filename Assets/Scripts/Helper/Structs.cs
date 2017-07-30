using UnityEngine;
using System.Collections;

namespace Helper
{
    public struct MeshData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector2[] UVs;
    }

    public struct LodData
    {
        public MeshData[] Levels;
        public bool IsSeaBottom;

        public LodData(MeshData[] levels, bool isSeaBottom)
        {
            Levels = levels;
            IsSeaBottom = isSeaBottom;
        }
    }

    public struct Int2
    {
        public int X;
        public int Z;

        public Int2(int x, int z)
        {
            X = x;
            Z = z;
        }
    }

    public struct BuilderData
    {
        public float[,] HeigthData;
        public Int2 Origin;
        public float UnitSize;
        public float MaxHeight;
        public bool SeaBed;
    }

    public struct TerrainChunkData
    {
        public float[,] HeigthData;
        public Int2 Origin;
        public float UnitSize;
        public float MaxHeight;
        public bool IsSeaBed;
        public Vector3 Position;
    }

    public struct TerrainChunkMeshData
    {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[] Triangles;
        public Vector2[] UVs;
        public Color32[] Colors32;
        public int Lod;
        public bool NeedsSet;
    }
}