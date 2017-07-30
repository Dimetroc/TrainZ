using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using UnityEngine;

namespace Terrain
{
    public class TerrainChunkDataProcessor
    {
        private TerrainChunkData _chunkData;

        private float[,] HeigthData { get { return _chunkData.HeigthData; } }
        private Int2 Origin { get { return _chunkData.Origin; } }
        private float MaxHeight {get { return _chunkData.MaxHeight; } }
        private bool IsSeaBed { get { return _chunkData.IsSeaBed; } }
        private readonly int _maxDataLod;
        private readonly float _сhunkSize;

        public TerrainChunkDataProcessor(TerrainChunkData data)
        {
            _chunkData = data;
            _сhunkSize = Consts.UnitSize;
            _maxDataLod = (int)Mathf.Log(HeigthData.GetLength(0) - 1, 2.0f);
        }

        public TerrainChunkMeshData GetMeshData(int lodLevel, int[] neighbors)
        {
            if (IsSeaBed || lodLevel == Consts.LodsAmount) return GetSeaBottomData(lodLevel);


            if (lodLevel == _maxDataLod) return GetBaseDataLod(lodLevel, neighbors);
            if (lodLevel <= _maxDataLod) return GetInterpolatedDataLod(lodLevel, neighbors);
            return GetDataLod(Mathf.Clamp(lodLevel, 0, Consts.LodsAmount), neighbors);
        }

        private TerrainChunkMeshData GetSeaBottomData(int lodLevel)
        {
            var amount = 2;
            var verts = PickDataVertices(HeigthData.GetLength(0) - 1).ToArray();
            var data =  new TerrainChunkMeshData()
            {
                Vertices = verts,
                Colors32 = Assistant.CreateColorsFromVerticies(verts),
                Triangles = Assistant.CreateTriangles(amount),
                UVs = Assistant.CreateUvs(amount),
                Lod = lodLevel
            };
            PostProcessChunkMeshData(ref  data);
            return data;
        }

        private TerrainChunkMeshData GetBaseDataLod(int lodLevel, int[] neighbors)
        {
            var amount = (HeigthData.GetLength(0));
            var verts = Assistant.CheckNeighbors(PickDataVertices(1), lodLevel, neighbors).ToArray();
            var data = new TerrainChunkMeshData()
            {
                Vertices = verts,
                Colors32 = Assistant.CreateColorsFromVerticies(verts),
                Triangles = Assistant.CreateTriangles(amount),
                UVs = Assistant.CreateUvs(amount),
                Lod = lodLevel
            };
            PostProcessChunkMeshData(ref data);
            return data;
        }

        private TerrainChunkMeshData GetDataLod(int lodLevel, int[] neighbors)
        {
            var lod = Consts.LodsAmount - lodLevel;
            if (lod > _maxDataLod - 1) lod = _maxDataLod - 1;
            var length = HeigthData.GetLength(0);
            var step = (length - 1) / (int)Mathf.Pow(2, lod);
            var amount = (length - 1) / step + 1;
            var verts = Assistant.CheckNeighbors(PickDataVertices(step), lodLevel, neighbors).ToArray();
            var data = new TerrainChunkMeshData()
            {
                Vertices = verts,
                Colors32 =  Assistant.CreateColorsFromVerticies(verts),
                Triangles = Assistant.CreateTriangles(amount),
                UVs = Assistant.CreateUvs(amount),
                Lod = lodLevel
            };
            PostProcessChunkMeshData(ref data);
            return data;
        }

        private TerrainChunkMeshData GetInterpolatedDataLod(int lodLevel, int[] neighbors)
        {
            var lod = Consts.LodsAmount - lodLevel - _maxDataLod;
            var amount = (HeigthData.GetLength(0) - 1) * ((int)Mathf.Pow(2, lod)) + 1;
            var matrix = Assistant.CreateVerticesMatrix(amount, Consts.UnitSize);

            for (int i = 0; i < HeigthData.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < HeigthData.GetLength(1) - 1; j++)
                {
                    var interpolatedData = GetInterpolatedData(new Int2(i  + 1, j), (int)Mathf.Pow(2, lod) - 1);
                    for (int k = 0; k < interpolatedData.GetLength(0); k++)
                    {
                        for (int l = 0; l < interpolatedData.GetLength(1); l++)
                        {
                            matrix[i * (interpolatedData.GetLength(0) - 1) + k, j * (interpolatedData.GetLength(0) - 1) + l].y = interpolatedData[k, interpolatedData.GetLength(1) - 1 - l] * MaxHeight;
                        }
                    }
                }
            }
            var verts = Assistant.CheckNeighbors(matrix, lodLevel, neighbors).ToArray();
            var data = new TerrainChunkMeshData()
            {
                Vertices = verts,
                Colors32 = Assistant.CreateColorsFromVerticies(verts),
                Triangles = Assistant.CreateTriangles(amount),
                UVs = Assistant.CreateUvs(amount),
                Lod = lodLevel
            };
            PostProcessChunkMeshData(ref data);
            return data;
        }

        private float[,] GetInterpolatedData(Int2 position, int iterations)
        {
            var interpolator = new BicubicInterpolator(ServiceLocator.I.TerrainData.PickSeedData(Origin.X + position.X, Origin.Z + position.Z));
            var data = new float[2 + iterations, 2 + iterations];

            for (int i = 0; i < 2 + iterations; i++)
            {
                for (int j = 0; j < 2 + iterations; j++)
                {
                    data[i, j] = interpolator.GetValue((float)i / (1 + iterations), (float)j / (1 + iterations));
                }
            }
            return data;
        }

        private Vector3[,] PickDataVertices(int step)
        {
            var size = HeigthData.GetLength(0);
            if (size % 2 == 0) throw new ArgumentException();
            var matrix = Assistant.CreateVerticesMatrix(((size - 1) / step) + 1, _сhunkSize);

            for (int x = 0; x < size; x += step)
            {
                for (int z = 0; z < size; z += step)
                {
                    matrix[x / step, z / step].y = HeigthData[x, z] * MaxHeight;
                }
            }
            return matrix;
        }

        private void PostProcessChunkMeshData(ref TerrainChunkMeshData data)
        {
            if(data.Lod > _maxDataLod) return;

            var newVertices = new Vector3[data.Triangles.Length];
            var newUv = new Vector2[data.Triangles.Length];
            var newTriangles = new int[data.Triangles.Length];
            var newColors = new Color32[data.Triangles.Length];
            var newNormals = new Vector3[data.Triangles.Length];

            var trCount = 0;
            for (var i = 0; i < data.Triangles.Length; i++)
            {
                newVertices[i] = data.Vertices[data.Triangles[i]];
                newUv[i] = data.UVs[data.Triangles[i]];
                newTriangles[i] = i;

                if (trCount == 0)
                {
                    newNormals[i] = newNormals[i + 1] = newNormals[i + 2] = CalculateNormal(data.Vertices[data.Triangles[i]], data.Vertices[data.Triangles[i + 1]], data.Vertices[data.Triangles[i + 2]]);
                    newColors[i] = newColors[i + 1] = newColors[i + 2] = data.Colors32[data.Triangles[i]];
                }
                trCount ++;
                if (trCount == 3) trCount = 0;
            }

            data.Vertices = newVertices;
            data.UVs = newUv;
            data.Triangles = newTriangles;
            //data.Colors32 = newColors;
            data.Colors32 = Assistant.CreateColorsFromVerticies(data.Vertices, data.Normals);
            data.Normals = newNormals;
        }

        private Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross(p2 - p1, p3 - p1);
        }
    }
}
