using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Helper;

public class LodMeshBuilder
{
    private Dictionary<int, Vector3[]> _verticesDictionary;
    private Dictionary<int, int[]> _trianglesDictionary;
    private Dictionary<int, Vector2[]> _uvsDictionary; 

    public LodMeshBuilder()
    {
        _verticesDictionary = new Dictionary<int, Vector3[]>();
        _trianglesDictionary= new Dictionary<int, int[]>();
        _uvsDictionary = new Dictionary<int, Vector2[]>();
    }

    public LodData GenerateLodDataFromHeigthMap(float[,] heigthData)
    {
        var size = heigthData.GetLength(0);
        if (size%2 == 0) throw new ArgumentException();
        var lods = (int) Mathf.Log(size - 1, 2.0f) + 1;
        var meshData = new MeshData[lods];
        var isSeaBottom = true;
        var count = lods - 1;
        for (int step = size -1; step >= 1; step/=2)
        {
            var amount = (size - 1) / step + 1;
            var vertices = GetVertices(amount);

            for (int x = 0; x < size; x+=step)
            {
                for (int z = 0; z < size; z += step)
                {
                    var height = heigthData[x, z] * Consts.MaxHight;
                    if (height > 10.0f) isSeaBottom = false;
                    //Debug.Log(x.ToString("D2") + "|" + z.ToString("D2") + "|" + step.ToString("D2") + "|" + vertices.Length.ToString("D3") + "|" + (((x / step) * ((size - 1) / step + 1)) + (z / step)).ToString("D3"));
                    vertices[((x / step) * ((size - 1) / step + 1)) + (z / step)].y = height;
                }
            }
            meshData[count] = new MeshData()
            {
                Vertices = vertices,
                Triangles = GetTriangles(amount),
                UVs = GetUVs(amount)
            };

            count --;
        }
        
        return new LodData(meshData, isSeaBottom);
    }

    private Vector3[] GetVertices(int size)
    {
        Vector3[] vertices;
        if (!_verticesDictionary.TryGetValue(size, out vertices))
        {
            vertices = CalculateVertices(size);
            _verticesDictionary.Add(size, vertices);
        }
        return vertices;
    }

    private Vector3[] CalculateVertices(int size)
    {
        
        //var step = (Consts.UnitDataSize - 1)/(size - 1);
        var step = Consts.UnitSize/(size - 1);
        //var hSize = (Consts.UnitDataSize - 1)/2.0f;
        var hSize = Consts.UnitSize / 2.0f;
        var vertices = new Vector3[size * size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                //vertices[x*size + z] = new Vector3(x*Consts.UnitSize*step - hSize*Consts.UnitSize, 0.0f, z*Consts.UnitSize*step - hSize*Consts.UnitSize);
                vertices[x*size + z] = new Vector3(x*step - hSize, 0.0f, z*step - hSize);
            }
        }
        return vertices;
    }

    private int[] GetTriangles(int size)
    {
        int[] tr;
        if (!_trianglesDictionary.TryGetValue(size, out tr))
        {
            tr = CalulateTriangles(size);
            _trianglesDictionary.Add(size,tr);
        }
        return tr;
    }

    private int[] CalulateTriangles(int size)
    {
        var trianglesCount = (size - 1) * (size - 1) * 2;
        var triangles = new int[trianglesCount * 3];
        int q = 0;
        for (int x = 0; x < size - 1; x++)
        {
            for (int z = 0; z < size - 1; z++)
            {
                triangles[q] = x * size + z;
                triangles[q + 1] = x * size + z + 1;
                triangles[q + 2] = (x + 1) * size + z;

                triangles[q + 3] = x * size + z + 1;
                triangles[q + 4] = (x + 1) * size + z + 1;
                triangles[q + 5] = (x + 1) * size + z;
                q += 6;
            }
        }

        return triangles;
    }

    private Vector2[] GetUVs(int size)
    {
        Vector2[] uvs;
        if (!_uvsDictionary.TryGetValue(size, out uvs))
        {
            uvs = CalculateUvs(size);
            _uvsDictionary.Add(size, uvs);
        }
        return uvs;
    }

    private Vector2[] CalculateUvs(int size)
    {
        var uvs = new Vector2[size * size];
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                uvs[x * size + z] = new Vector2((float)x / (size - 1), (float)z / (size - 1));
            }
        }
        return uvs;
    }
}
