using System;
using UnityEngine;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using Helper;

public class TerrainMeshBuilder : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private int _lastX;
    [SerializeField] private int _lastZ;
    [SerializeField] private Texture2D _heigthMap;
    [SerializeField] private Material _material;

    private int interpolations = 2;

    [SerializeField] private SpriteRenderer _renderer;

    private LodMeshBuilder _lodMeshBuilder;

    public event Action<int, int> OnCamPositionChanged = delegate { };

    void Start()
    {
        _lastX = Mathf.FloorToInt(_cameraTransform.position.x/Consts.UnitSize);
        _lastZ = Mathf.FloorToInt(_cameraTransform.position.z/ Consts.UnitSize);
        _lodMeshBuilder = new LodMeshBuilder();
        //StartCoroutine(BuilWorldMesh());
        StartCoroutine(BuilWorldMesh());
    }

    IEnumerator BuilWorldMesh()
    {
        var w = _heigthMap.width - 1;
        var u = Consts.UnitDataSize - 1;
        var unitsNumber = Mathf.CeilToInt((float)w/u) + 1;
        Debug.Log(unitsNumber);
        var shift = Mathf.FloorToInt((u* unitsNumber - w)/2.0f);
        var amountOfPoints = unitsNumber*(Consts.UnitDataSize - 1) + 1;
        var middle = (unitsNumber * Consts.UnitSize) /2.0f;
        var shift2 = amountOfPoints - shift - _heigthMap.width;
        var sX = 0;
        var sY = 0;

        for (int i =  0; i < unitsNumber; i++)
        {
            sX = 0;
            for (int j = 0; j < unitsNumber; j++)
            {
                var data = new float[Consts.UnitDataSize, Consts.UnitDataSize];
                var firstPoint = true;
                var start = new Int2();

                for (int k = Consts.UnitDataSize - 1; k >= 0; k--)
                {
                    for (int l = 0; l < Consts.UnitDataSize; l++)
                    {

                        var x = i * Consts.UnitDataSize + k + sY;
                        var y = j * Consts.UnitDataSize + l + sX;

                        if (firstPoint)
                        {
                            start.X = x - shift;
                            start.Z = y - shift;
                            firstPoint = false;
                        }

                        if (x > shift && x < amountOfPoints - shift2 && y > shift && y < amountOfPoints - shift2)
                        {
                            data[k, l] = _heigthMap.GetPixel(x - shift, y - shift).grayscale;
                        }
                        else
                        {
                            data[k, l] = 0.0f;
                        }
                    }
                }
                sX --;
                CreateNewChank(i, j, start, middle, data);
            }
            sY --;
            yield return null;
        }
    }

    private IEnumerator BuildWorldMeshInterpolated()
    {
        var size = (_heigthMap.width - 1)*(interpolations + 1) + 1; 
        Debug.Log(size);
        Texture2D texture = new Texture2D(size,size);
        //Texture2D texture = new Texture2D(_heigthMap.width, _heigthMap.width);
        for (int y = _heigthMap.height - 1; y >= 0 ; y--)
        {
            for (int x = 0; x < _heigthMap.width; x++)
            {
                var seed = PickSeedData(x, y);
                var data = InterpolateSeedData(seed, interpolations);
                SetDataToTheTexture(x,y, data, ref texture);
                //texture.SetPixel(x,y,new Color(data[1,1], data[1, 1], data[1, 1],1));
                yield return null;
            }

        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        var sprite = Sprite.Create(texture,new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100);
        _renderer.sprite = sprite;
    }

    private void SetDataToTheTexture(int x, int y, float[,] data, ref Texture2D texture)
    {
        var size = data.GetLength(0);
        for (int i = size - 1; i > 0; i--)
        {
            for (int j = size - 1; j > 0; j--)
            {
                //if(x * size - size - j < 0 || y * size + i > texture.height) continue;
                //texture.SetPixel(x*size - size - j, y*size + i , new Color(data[j, i], data[j, i], data[j, i], 1));

                texture.SetPixel(x*(size - 1) - (size - 1) + j, y * (size - 1) + (size - 1) - i, new Color(data[j, i], data[j, i], data[j, i], 1));
            }
        }
    }

    public float[,] PickSeedData(int x, int y)
    {
        var data = new float[4, 4];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                data[i, j] = GetPixelValue(x - 2 + i, y + 2 - j);
            }
        }

        return data;
    }

    private float[,] InterpolateSeedData(float[,] seed, int iterations)
    {
        var interpolator = new BicubicInterpolator(seed);
        var data = new float[2 + iterations, 2 + iterations];

        for (int i = 0; i < 2 + iterations; i++)
        {
            for (int j = 0; j < 2 + iterations; j++)
            {
                data[i, j] = interpolator.GetValue((float) i/(float) (1 + iterations), (float) j/(float) (1 + iterations));
            }
        }

        return data;
    }

    private float GetPixelValue(int x, int y)
    {
        if (x < 0 || y < 0) return 0.0f;
        if (x >= _heigthMap.width || y >= _heigthMap.height) return 0.0f;
        var data = _heigthMap.GetPixel(x, y).grayscale;
        return data;
    }

    private void CreateNewChank(int i, int j, Int2 origin, float middle, float[,] heightData)
    {
        var go = new GameObject("Chunk_" + i + "_" + j);
        go.transform.position = new Vector3(((i - 1)*Consts.UnitSize) - middle, 0, ((j - 1)*Consts.UnitSize) - middle);
        //var lodData = _lodMeshBuilder.GenerateLodDataFromHeigthMap(DataResample.DoubleResample(data));
        var data = new BuilderData()
        {
            HeigthData = heightData,
            Origin = origin,
            MaxHeight = Consts.MaxHight,
            UnitSize = Consts.UnitSize
        };
        go.AddComponent<LodTerrainUnit>().Init(data, _material, this);
    }


}

/*using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

public class TerrainMeshBuilder : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private int _lastX;
    [SerializeField] private int _lastZ;
    [SerializeField] private Texture2D _heigthMap;
    [SerializeField] private Material _material;

    private int interpolations = 2;

    [SerializeField] private SpriteRenderer _renderer;

    private LodMeshBuilder _lodMeshBuilder;

    private Dictionary<Int2, LodTerrainUnit> _lodTerrainUnits; 

    public event Action<int, int> OnCamPositionChanged = delegate { };

    void Start()
    {
        _lodTerrainUnits = new Dictionary<Int2, LodTerrainUnit>();
        _lastX = Mathf.FloorToInt(_cameraTransform.position.x/Consts.UnitSize);
        _lastZ = Mathf.FloorToInt(_cameraTransform.position.z/ Consts.UnitSize);
        _lodMeshBuilder = new LodMeshBuilder();
        //StartCoroutine(BuilWorldMesh());
        StartCoroutine(BuilWorldMesh());
    }

    IEnumerator BuilWorldMesh()
    {
        var w = _heigthMap.width - 1;
        var u = Consts.UnitDataSize - 1;
        var unitsNumber = Mathf.CeilToInt((float)w/u) + 1;
        Debug.Log(unitsNumber);
        var shift = Mathf.FloorToInt((u* unitsNumber - w)/2.0f);
        var amountOfPoints = unitsNumber*(Consts.UnitDataSize - 1) + 1;
        var middle = (unitsNumber * Consts.UnitSize) /2.0f;
        var shift2 = amountOfPoints - shift - _heigthMap.width;
        var sX = 0;
        var sY = 0;

        for (int i =  0; i < unitsNumber; i++)
        {
            sX = 0;
            for (int j = 0; j < unitsNumber; j++)
            {
                var data = new float[Consts.UnitDataSize, Consts.UnitDataSize];
                var seaBed = true;
                for (int k = Consts.UnitDataSize - 1; k >= 0; k--)
                {
                    for (int l = 0; l < Consts.UnitDataSize; l++)
                    {

                        var x = i * Consts.UnitDataSize + k + sY;
                        var y = j * Consts.UnitDataSize + l + sX;

                        if (x > shift && x < amountOfPoints - shift2 && y > shift && y < amountOfPoints - shift2)
                        {
                            var value = _heigthMap.GetPixel(x - shift, y - shift).grayscale;
                            if (value > 0.0001f) seaBed = false;
                            data[k, l] = value;
                        }
                        else
                        {
                            data[k, l] = 0.0f;
                        }
                    }
                }
                sX --;
                CreateNewChank(i, j, middle, data, seaBed);
            }
            sY --;
            yield return null;
        }
    }

    private IEnumerator BuildWorldMeshInterpolated()
    {
        var size = (_heigthMap.width - 1)*(interpolations + 1) + 1; 
        Debug.Log(size);
        Texture2D texture = new Texture2D(size,size);
        //Texture2D texture = new Texture2D(_heigthMap.width, _heigthMap.width);
        for (int y = _heigthMap.height - 1; y >= 0 ; y--)
        {
            for (int x = 0; x < _heigthMap.width; x++)
            {
                var seed = PickSeedData(x, y);
                var data = InterpolateSeedData(seed, interpolations);
                SetDataToTheTexture(x,y, data, ref texture);
                //texture.SetPixel(x,y,new Color(data[1,1], data[1, 1], data[1, 1],1));
                yield return null;
            }

        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        var sprite = Sprite.Create(texture,new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100);
        _renderer.sprite = sprite;
    }

    private void SetDataToTheTexture(int x, int y, float[,] data, ref Texture2D texture)
    {
        var size = data.GetLength(0);
        for (int i = size - 1; i > 0; i--)
        {
            for (int j = size - 1; j > 0; j--)
            {
                //if(x * size - size - j < 0 || y * size + i > texture.height) continue;
                //texture.SetPixel(x*size - size - j, y*size + i , new Color(data[j, i], data[j, i], data[j, i], 1));

                texture.SetPixel(x*(size - 1) - (size - 1) + j, y * (size - 1) + (size - 1) - i, new Color(data[j, i], data[j, i], data[j, i], 1));
            }
        }
    }

    public float[,] PickSeedData(int x, int y)
    {
        var data = new float[4, 4];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                data[i, j] = GetPixelValue(x - 2 + i, y + 2 - j);
            }
        }

        return data;
    }

    private float[,] InterpolateSeedData(float[,] seed, int iterations)
    {
        var interpolator = new BicubicInterpolator(seed);
        var data = new float[2 + iterations, 2 + iterations];

        for (int i = 0; i < 2 + iterations; i++)
        {
            for (int j = 0; j < 2 + iterations; j++)
            {
                data[i, j] = interpolator.GetValue((float) i/(float) (1 + iterations), (float) j/(float) (1 + iterations));
            }
        }

        return data;
    }

    private float GetPixelValue(int x, int y)
    {
        if (x < 0 || y < 0) return 0.0f;
        if (x >= _heigthMap.width || y >= _heigthMap.height) return 0.0f;
        var data = _heigthMap.GetPixel(x, y).grayscale;
        return data;
    }

    private void CreateNewChank(int i, int j, float middle, float[,] heightData, bool isSeaBed)
    {
        var go = new GameObject("Chunk_" + i + "_" + j);
        go.transform.position = new Vector3(((i - 1)*Consts.UnitSize) - middle, 0, ((j - 1)*Consts.UnitSize) - middle);
        //var lodData = _lodMeshBuilder.GenerateLodDataFromHeigthMap(DataResample.DoubleResample(data));
        var data = new BuilderData()
        {
            HeigthData = heightData,
            Origin = new Int2(i,j),
            MaxHeight = Consts.MaxHight,
            UnitSize = Consts.UnitSize,
            SeaBed = isSeaBed
        };
        _lodTerrainUnits.Add(new Int2(i,j),  go.AddComponent<LodTerrainUnit>().Init(data, _material, this));
    }

    public LodTerrainUnit GetUnit(Int2 position)
    {
        LodTerrainUnit unit = null;
        _lodTerrainUnits.TryGetValue(position, out unit);
        return unit;
    }

    private void Update()
    {
        var x = Mathf.FloorToInt(_cameraTransform.position.x / Consts.UnitSize);
        var z = Mathf.FloorToInt(_cameraTransform.position.z / Consts.UnitSize);
        if (x != _lastX || z != _lastZ)
        {
            _lastX = x;
            _lastZ = z;
            OnCamPositionChanged(x, z);
        }
    }
}
*/
