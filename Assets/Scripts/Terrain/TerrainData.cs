using System;
using Helper;
using UnityEngine;

namespace Terrain
{
    public class TerrainData: MonoBehaviour
    {
        [SerializeField] private Texture2D _heigthMap;
        [SerializeField] private Material _terrainMaterial;

        [SerializeField] private Color _seabed;
        [SerializeField] private float _seabedHeight;
        [SerializeField] private Color _shore;
        [SerializeField] private float _shoreHeight;
        [SerializeField] private Color _midlends;
        [SerializeField] private float _midlendsHeight;
        [SerializeField] private Color _highlands;
        [SerializeField] private float _highlandsHeight;
        [SerializeField] private Color _mountains;
        [SerializeField] private float _mountainsHeight;
        [SerializeField] private Color _mountainTops;
        [SerializeField] private float _mountainTopsHeight;


        private float[][] _data;

        private void Awake()
        {
            if(_heigthMap == null) throw new NullReferenceException("No height map!");
            if(_heigthMap.width != _heigthMap.height) throw new ArgumentException("Map sides are not equal!");
            ServiceLocator.I.SetTerrainData(this);
            ReadData();
        }

        public Color GetPointColor(Vector3 point)
        {
            var val = point.y/Consts.MaxHight;
            if (val < _seabedHeight)
            {
                return _seabed;
            }
            if (val < _shoreHeight)
            {
                return _shore;
            }
            if (val < _midlendsHeight)
            {
                return _midlends;
            }
            if (val < _highlandsHeight)
            {
                return _highlands;
            }
            if (val < _mountainsHeight)
            {
                return _mountains;
            }
            return _mountainTops;
        }

        private void ReadData()
        {
            _data = new float[_heigthMap.width][];

            for (int i = 0; i < _heigthMap.width; i++)
            {
                _data[i] = new float[_heigthMap.height];
                for (int j = 0; j < _heigthMap.height; j++)
                {
                    _data[i][j] = _heigthMap.GetPixel(i, j).grayscale;
                }
            }
            _heigthMap = null;
        }

        public int GetMapSize()
        {
            return _data.Length;
        }

        public float GetPixelValue(int i, int j)
        {
            if (i < 0 || j < 0) return 0.0f;
            if (i >= _data.Length || j >= _data[i].Length) return 0.0f;
            return _data[i][j];
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

        public float[,] GetBaseData(int iOffset, int jOffset, int length, int bounds)
        {
            var data = new float[length, length];
            for (int k = length - 1; k >= 0; k--)
            {
                for (int l = 0; l < length; l++)
                {
                    data[k, l] = GetPixelValue(iOffset * (length - 1) + k - bounds, jOffset * (length - 1) + l - bounds);
                }
            }
            return data;
        }

        public Material GetMainTerrainMaterial()
        {
            return _terrainMaterial;
        }
    }
}
