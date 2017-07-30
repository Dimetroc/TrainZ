using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using UnityEngine;


public class LodInterpolatedMeshBuilder
{
    private readonly float[,] _heigthData;
    private Int2 _origin;
    private readonly float _unitSize;
    private readonly float _maxHeight;
    private readonly TerrainMeshBuilder _meshBuilder;
    private int _maxDataLod;
    public bool IsSeaBottom { get; private set; }

    public LodInterpolatedMeshBuilder(TerrainMeshBuilder meshBuilder, BuilderData data)
    {
        _meshBuilder = meshBuilder;
        _heigthData = data.HeigthData;
        _origin = data.Origin;
        _unitSize = data.UnitSize;
        _maxHeight = data.MaxHeight;
        _maxDataLod = (int)Mathf.Log(_heigthData.GetLength(0) - 1, 2.0f) + 1;
        IsSeaBottom = data.HeigthData.IsSeaBed();
    }

    /*
    public LodData GenerateLodDataFromHeigthMap(int level)
    {
        var size = _heigthData.GetLength(0);
        if (size%2 == 0) throw new ArgumentException();
        var lods = (int) Mathf.Log(size - 1, 2.0f) + 1;
        var meshData = new MeshData[lods];
        var isSeaBottom = true;
        var count = lods - 1;
        for (int step = size - 1; step >= 1; step /= 2)
        {
            var amount = (size - 1)/step + 1;
            var vertices = CalculateVertices(amount);

            for (int x = 0; x < size; x += step)
            {
                for (int z = 0; z < size; z += step)
                {
                    var height = _heigthData[x, z]* _maxHeight;
                    if (height > 10.0f) isSeaBottom = false;
                    //Debug.Log(x.ToString("D2") + "|" + z.ToString("D2") + "|" + step.ToString("D2") + "|" + vertices.Length.ToString("D3") + "|" + (((x / step) * ((size - 1) / step + 1)) + (z / step)).ToString("D3"));
                    vertices[((x/step)*((size - 1)/step + 1)) + (z/step)].y = height;
                }
            }
            meshData[count] = new MeshData()
            {
                Vertices = vertices,
                Triangles = CalulateTriangles(amount),
                UVs = CalculateUvs(amount)
            };

            count--;
        }

        return new LodData(meshData, isSeaBottom);
    }
    */

    public MeshData GetMeshData(int lodLevel, bool[] neighbors)
    {
        if (IsSeaBottom || lodLevel == Consts.LodsAmount) return GetSeaBottomData();
        if (lodLevel == 4) return GetDataLod(neighbors);
        if (lodLevel < 4) return GetInterpolatedDataLod(lodLevel, neighbors);
        return GetDataLod(Mathf.Clamp(lodLevel + 1,0, Consts.LodsAmount), neighbors);
    }

    //private MeshData GetInterpolatedDataLod(int lodLevel, bool[] neighbors)
    //{
    //    var lod = Consts.LodsAmount - lodLevel - _maxDataLod;
    //    var amount = (_heigthData.GetLength(0) - 1) * ((int)Mathf.Pow(2, lod)) + 1;
    //    var vertices = CalculateVertices(amount);
    //    for (int i = 0; i < _heigthData.GetLength(0) - 1; i++)
    //    {
    //        for (int j = 0; j < _heigthData.GetLength(1) - 1; j++)
    //        {
    //            var data = GetInterpolatedData(new Int2(i,j), (int)Mathf.Pow(2,lod) - 1);
    //            var lineLength = ((data.GetLength(0) - 1)*(_heigthData.GetLength(0) - 1) + 1);
    //            for (int k = data.GetLength(0) - 1; k >=0 ; k--)
    //            {
    //                for (int l = data.GetLength(1) - 1; l >= 0; l--)
    //                {
    //                    vertices[i* (data.GetLength(0) - 1) * lineLength + k * lineLength + j * (data.GetLength(0) - 1) + l].y = data[k, data.GetLength(1) - 1 - l]*Consts.MaxHight;
    //                }
    //            }
    //        }
    //    }
    //    return new MeshData()
    //    {
    //        Vertices = vertices,
    //        Triangles = CalulateTriangles(amount),
    //        UVs = CalculateUvs(amount)
    //    };
    //}

    private MeshData GetDataLod(bool[] neighbors)
    {
        var amount = (_heigthData.GetLength(0));
        var matrix = Assistant.CreateVerticesMatrix(amount, Consts.UnitSize);
        for (int i = 0; i < _heigthData.GetLength(0); i++)
        {
            for (int j = 0; j < _heigthData.GetLength(1); j++)
            {
                matrix[i, j].y = _heigthData[i, j]*Consts.MaxHight;
            }
        }

        return new MeshData()
        {
            Vertices = GetArrayFromMatrix(matrix, neighbors),
            Triangles = Assistant.CreateTriangles(amount),
            UVs = Assistant.CreateUvs(amount)
        };
    }

    private MeshData GetInterpolatedDataLod(int lodLevel, bool[] neighbors)
    {
        var lod = Consts.LodsAmount - lodLevel - _maxDataLod;
        var amount = (_heigthData.GetLength(0) - 1) * ((int)Mathf.Pow(2, lod)) + 1;

        var matrix = Assistant.CreateVerticesMatrix(amount,Consts.UnitSize);

        for (int i = 0; i < _heigthData.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < _heigthData.GetLength(1) - 1; j++)
            {
                var data = GetInterpolatedData(new Int2(i, j), (int)Mathf.Pow(2, lod) - 1);
                var lineLength = ((data.GetLength(0) - 1) * (_heigthData.GetLength(0) - 1) + 1);
                for (int k = 0; k < data.GetLength(0); k++)
                {
                    for (int l = 0; l < data.GetLength(1); l++)
                    {
                        //vertices[i * (data.GetLength(0) - 1) * lineLength + k * lineLength + j * (data.GetLength(0) - 1) + l].y = data[k, data.GetLength(1) - 1 - l] * Consts.MaxHight;
                        matrix[i*(data.GetLength(0) - 1) + k, j*(data.GetLength(0) - 1) + l].y = data[k, data.GetLength(1) - 1 - l] * Consts.MaxHight;
                    }
                }
            }
        }
        return new MeshData()
        {
            Vertices = GetArrayFromMatrix(matrix, neighbors),
            Triangles = Assistant.CreateTriangles(amount),
            UVs = Assistant.CreateUvs(amount)
        };
    }

    private float[,] GetInterpolatedData(Int2 position, int iterations)
    {
        //Todo fix "7" WTF?!
        var interpolator = new BicubicInterpolator(_meshBuilder.PickSeedData(_origin.X + position.X - 7, _origin.Z + position.Z));
        var data = new float[2 + iterations, 2 + iterations];

        for (int i = 0; i < 2 + iterations; i++)
        {
            for (int j = 0; j < 2 + iterations; j++)
            {
                data[i, j] = interpolator.GetValue((float)i / (float)(1 + iterations), (float)j / (float)(1 + iterations));
            }
        }

        return data;
    }

    //private MeshData GetDataLod(int lodLevel, bool[] neighbors)
    //{
    //    var lod = Consts.LodsAmount - lodLevel;
    //    if (lod == 0) return GetSeaBottomData();
    //    if (lod > _maxDataLod - 1) lod = _maxDataLod - 1;

    //    var size = _heigthData.GetLength(0);
    //    var step = (size - 1)/(int)Mathf.Pow(2, lod);


    //    var amount = (size - 1)/step + 1;
    //    var vertices = CalculateVertices(amount);

    //    for (int x = 0; x < size; x += step)
    //    {
    //        for (int z = 0; z < size; z += step)
    //        {
    //            var height = _heigthData[x, z]*_maxHeight;

    //            //Debug.Log(x.ToString("D2") + "|" + z.ToString("D2") + "|" + step.ToString("D2") + "|" + vertices.Length.ToString("D3") + "|" + (((x / step) * ((size - 1) / step + 1)) + (z / step)).ToString("D3"));
    //            vertices[((x/step)*((size - 1)/step + 1)) + (z/step)].y = height;
    //        }
    //    }
    //    return new MeshData()
    //    {
    //        Vertices = vertices,
    //        Triangles = CalulateTriangles(amount),
    //        UVs = CalculateUvs(amount)
    //    };
    //}

    private MeshData GetDataLod(int lodLevel, bool[] neighbors)
    {
        var lod = Consts.LodsAmount - lodLevel;
        if (lod > _maxDataLod - 1) lod = _maxDataLod - 1;
        

        var size = _heigthData.GetLength(0);
        var step = (size - 1) / (int)Mathf.Pow(2, lod);
        

        var amount = (size - 1) / step + 1;

        var matrix = Assistant.CreateVerticesMatrix(amount, Consts.UnitSize);

        for (int x = 0; x < size; x += step)
        {
            for (int z = 0; z < size; z += step)
            {
                var height = _heigthData[x, z] * _maxHeight;

                //Debug.Log(x.ToString("D2") + "|" + z.ToString("D2") + "|" + step.ToString("D2") + "|" + vertices.Length.ToString("D3") + "|" + (((x / step) * ((size - 1) / step + 1)) + (z / step)).ToString("D3"));
                //vertices[((x / step) * ((size - 1) / step + 1)) + (z / step)].y = height;
                matrix[x/step, z/step].y = height;
            }
        }
        return new MeshData()
        {
            Vertices = GetArrayFromMatrix(matrix, neighbors),
            Triangles = Assistant.CreateTriangles(amount),
            UVs = Assistant.CreateUvs(amount)
        };
    }

    private Vector3[] GetArrayFromMatrix(Vector3[,] matrix, bool[] neighbors)
    {
        var array = new Vector3[matrix.GetLength(0) * matrix.GetLength(1)];
        if (matrix.GetLength(0) > 2)
        {
            if (neighbors[0])
            {
                var i = 0;
                for (int j = 1; j < matrix.GetLength(1); j += 2)
                {
                    matrix[i, j] = (matrix[i, j - 1] + matrix[i, j + 1])/2.0f;
                }
            }

            if (neighbors[1])
            {
                var j = matrix.GetLength(1) - 1;
                for (int i = 1; i < matrix.GetLength(0); i += 2)
                {
                    matrix[i, j] = (matrix[i - 1, j] + matrix[i + 1, j])/2.0f;
                }
            }

            if (neighbors[2])
            {
                var i = matrix.GetLength(0) - 1;
                for (int j = 1; j < matrix.GetLength(0); j += 2)
                {
                    matrix[i, j] = (matrix[i, j - 1] + matrix[i, j + 1])/2.0f;
                }
            }

            if (neighbors[3])
            {
                var j = 0;
                for (int i = 1; i < matrix.GetLength(0); i += 2)
                {
                    matrix[i, j] = (matrix[i - 1, j] + matrix[i + 1, j])/2.0f;
                }
            }
        }
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                array[i*matrix.GetLength(0) + j] = matrix[i, j];
            }
        }

        return array;
    }
 
    private MeshData GetSeaBottomData()
    {
        var size = _heigthData.GetLength(0);
        if (size % 2 == 0) throw new ArgumentException();
        
        var step = size - 1;

        var vertices = Assistant.CreateVertices(2, Consts.UnitSize);

        for (int x = 0; x < size; x += step)
        {
            for (int z = 0; z < size; z += step)
            {
                vertices[((x/step)*((size - 1)/step + 1)) + (z/step)].y = _heigthData[x, z]*_maxHeight;
            }
        }
        return new MeshData()
        {
            Vertices = vertices,
            Triangles = Assistant.CreateTriangles(2),
            UVs = Assistant.CreateUvs(2)
        };
    }

    private Vector3[] RecalculateVertecies(Vector3[,] matrix, bool[] neighbors)
    {

        return null;
    }

}

