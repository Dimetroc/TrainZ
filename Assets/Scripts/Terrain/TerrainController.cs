using System.Collections;
using System.Threading;
using Helper;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Terrain
{
    public class TerrainController: MonoBehaviour
    {
        private ChunkLod[][] _chunksLods;
        private TerrainChunkMeshData[][] _chunkMeshDatas;
        private TerrainChunk[][] _terrainChunks;
        private object _lock;

        private Int2 _lastCameraPos;
        private bool _cameraPositionChanged = false;
        private bool _needToUpdateVisualizers = false;
        private bool _isUpdateTaskActive = false;

        private void Awake()
        {
            _lock = new object();
            ServiceLocator.I.SetTerrainController(this);
        }

        private void Start()
        {
            StartCoroutine(BuilWorldMesh());
        }

        private void OnDestroy()
        {
            _isUpdateTaskActive = false;
        }

        IEnumerator BuilWorldMesh()
        {
            var w = ServiceLocator.I.TerrainData.GetMapSize() - 1;
            var u = (Consts.UnitDataSize - 1);
            var unitsNumber = Mathf.CeilToInt((float)w / u) + 1;
            var bounds = Mathf.FloorToInt((u * unitsNumber - w) / 2.0f);
            var middle = (unitsNumber * Consts.UnitSize) / 2.0f;

            _chunksLods = new ChunkLod[unitsNumber][];
            _terrainChunks = new TerrainChunk[unitsNumber][];
            _chunkMeshDatas = new TerrainChunkMeshData[unitsNumber][];

            for (int i = 0; i < unitsNumber; i++)
            {
                _chunksLods[i] = new ChunkLod[unitsNumber];
                _terrainChunks[i] = new TerrainChunk[unitsNumber];
                _chunkMeshDatas[i] = new TerrainChunkMeshData[unitsNumber];
                for (int j = 0; j < unitsNumber; j++)
                {
                    var data = new TerrainChunkData()
                    {
                        HeigthData = ServiceLocator.I.TerrainData.GetBaseData(i, j, Consts.UnitDataSize, bounds),
                        Origin = new Int2(i*(Consts.UnitDataSize - 1) - bounds, j*(Consts.UnitDataSize - 1) - bounds),
                        MaxHeight = Consts.MaxHight,
                        UnitSize = Consts.UnitSize,
                        Position = new Vector3(((i - 1)*Consts.UnitSize) - middle, 0, ((j - 1)*Consts.UnitSize) - middle)
                    };

                    CreateAndUpdateChunk(i, j, data);
                }
                yield return null;
            }

            ServiceLocator.I.EventsHolder.CameraPositionChangedEvent += OnCameraPositionChangedEvent;

            _isUpdateTaskActive = true;


            var t = new Thread(UpdateTask) { Priority = ThreadPriority.Normal };
            t.Start();
        }

        private void CreateAndUpdateChunk(int i, int j, TerrainChunkData data)
        {
            _terrainChunks[i][j] = new TerrainChunk(data, transform);
            _chunksLods[i][j] = new ChunkLod(data);
            _chunksLods[i][j].IsThereNewData(0, 0);
            _chunkMeshDatas[i][j] = _chunksLods[i][j].GetNewData();
            _terrainChunks[i][j].UpdateChunk(_chunkMeshDatas[i][j]);
        }

        private void Update()
        {
            UpdateVisualizers();
        }

        private void UpdateTask()
        {
            while (_isUpdateTaskActive)
            {
                UpdateMeshData();
                Thread.Sleep(0);
            }
        }

        private void OnCameraPositionChangedEvent(int cx, int cz)
        {
            lock (_lock)
            {
                _lastCameraPos = new Int2(cx, cz);
                _cameraPositionChanged = true;
            }
        }

        private void UpdateMeshData()
        {
            lock (_lock)
            {
                if(!_cameraPositionChanged || _needToUpdateVisualizers) return;
                var l0 = _chunksLods.Length;
                for (int i = 0; i < l0; i++)
                {
                    var l1 = _chunksLods[i].Length;
                    for (int j = 0; j < l1; j++)
                    {
                        if (_chunksLods[i][j].IsThereNewData(_lastCameraPos.X, _lastCameraPos.Z))
                        {
                            _chunkMeshDatas[i][j] = _chunksLods[i][j].GetNewData();
                            _chunkMeshDatas[i][j].NeedsSet = true;
                        }
                    }
                }
                _needToUpdateVisualizers = true;
                _cameraPositionChanged = false;
            }
        }

        private void UpdateVisualizers()
        {
            lock (_lock)
            {
                if (!_needToUpdateVisualizers) return;
                var l0 = _terrainChunks.Length;
                for (int i = 0; i < l0; i++)
                {
                    var l1 = _terrainChunks[i].Length;
                    for (int j = 0; j < l1; j++)
                    {
                        if (_chunkMeshDatas[i][j].NeedsSet)
                        {
                            _terrainChunks[i][j].UpdateChunk(_chunkMeshDatas[i][j]);
                            _chunkMeshDatas[i][j].NeedsSet = false;
                        }
                    }
                }
                _needToUpdateVisualizers = false;
            }
        }


    }
}
