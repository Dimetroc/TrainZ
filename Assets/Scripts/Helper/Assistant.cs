using Helper;
using UnityEngine;


public static class Assistant
{
    #region DataProcessing

    public static Vector3[] ToArray(this Vector3[,] matrix)
    {
        var array = new Vector3[matrix.GetLength(0) * matrix.GetLength(1)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                array[i * matrix.GetLength(0) + j] = matrix[i, j];
            }
        }

        return array;
    }

    public static bool IsSeaBed(this float[,] data)
    {
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                if (data[i, j] > 0.001f) return false;
            }
        }
        return true;
    }

    #endregion

    #region Mesh

    public static Vector3[] CreateVertices(int length, float size)
    {
        var step = size/(length - 1);
        var hSize = size/2.0f;
        var vertices = new Vector3[length*length];

        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                vertices[x*length + z] = new Vector3(x*step - hSize, 0.0f, z*step - hSize);
            }
        }
        return vertices;
    }

    public static Vector3[,] CreateVerticesMatrix(int length, float size)
    {
        var step = size/(length - 1);
        var hSize = size/2.0f;
        var vertices = new Vector3[length, length];

        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                vertices[x, z] = new Vector3(x*step - hSize, 0.0f, z*step - hSize);
            }
        }
        return vertices;
    }

    public static int[] CreateTriangles(int length)
    {
        var trianglesCount = (length - 1)*(length - 1)*2;
        var triangles = new int[trianglesCount*3];
        int q = 0;
        for (int x = 0; x < length - 1; x++)
        {
            for (int z = 0; z < length - 1; z++)
            {
                triangles[q] = x*length + z;
                triangles[q + 1] = x*length + z + 1;
                triangles[q + 2] = (x + 1)*length + z;

                triangles[q + 3] = x*length + z + 1;
                triangles[q + 4] = (x + 1)*length + z + 1;
                triangles[q + 5] = (x + 1)*length + z;
                q += 6;
            }
        }

        return triangles;
    }

    public static Vector2[] CreateUvs(int length)
    {
        var uvs = new Vector2[length*length];
        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                uvs[x*length + z] = new Vector2((float) x/(length - 1), (float) z/(length - 1));
            }
        }
        return uvs;
    }

    public static Color32[] CreateColorsFromVerticies(Vector3[] verticies)
    {
        var length = verticies.Length;
        var colors = new Color32[length];
        for (int i = 0; i < length; i++)
        {
            colors[i] = ServiceLocator.I.TerrainData.GetPointColor(verticies[i]);
        }

        return colors;
    }

    public static Color32[] CreateColorsFromVerticies(Vector3[] verticies, Vector3[] normals)
    {
        var length = verticies.Length;
        var colors = new Color32[length];
        for (int i = 0; i < length; i+= 3)
        {
            colors[i] = colors[i + 1] = colors[i + 2] = ServiceLocator.I.TerrainData.GetPointColor(verticies[i]) * (normals != null ? Vector3.Dot(normals[i], Vector3.up) : 1);
        }

        return colors;
    }

    #endregion

    #region Lod

    public static int GetUnitLod(int unitX, int unitZ, int cx, int cz)
    {
        var x = Mathf.Abs(unitX - cx);
        var z = Mathf.Abs(unitZ - cz);
        return Mathf.Clamp((x < z ? z : x), 0, Consts.LodsAmount);
    }

    public static int[] GetNeighborsLods(int unitX, int unitZ, int cx, int cz)
    {
        return new int[]
        {
            GetUnitLod(unitX, unitZ + 1, cx, cz),
            GetUnitLod(unitX + 1, unitZ, cx, cz),
            GetUnitLod(unitX, unitZ - 1, cx, cz),
            GetUnitLod(unitX - 1, unitZ, cx, cz),  
                    
        };
    }

    public static Vector3[,] CheckNeighbors(Vector3[,] matrix, int lod, int[] neighbors)
    {
        if (matrix.GetLength(0) <= 2) return matrix;
        if (lod < neighbors[0])
        {
            var j = matrix.GetLength(1) - 1;
            for (int i = 1; i < matrix.GetLength(0); i += 2)
            {
                matrix[i, j] = (matrix[i - 1, j] + matrix[i + 1, j]) / 2.0f;
            }
        }

        if (lod < neighbors[1])
        {
            var i = matrix.GetLength(0) - 1; 
            for (int j = 1; j < matrix.GetLength(0); j += 2)
            {
                matrix[i, j] = (matrix[i, j - 1] + matrix[i, j + 1]) / 2.0f;
            }
        }

        if (lod < neighbors[2])
        {
            var j = 0;
            for (int i = 1; i < matrix.GetLength(0); i += 2)
            {
                matrix[i, j] = (matrix[i - 1, j] + matrix[i + 1, j]) / 2.0f;
            }
        }

        if (lod < neighbors[3])
        {
            var i = 0;
            for (int j = 1; j < matrix.GetLength(0); j += 2)
            {
                matrix[i, j] = (matrix[i, j - 1] + matrix[i, j + 1]) / 2.0f;
            }
        }
        return matrix;
    }

    #endregion

}

