using Helper;
using UnityEngine;

namespace Terrain
{
    public class TerrainVisualizer:MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private int _lod;

        private void Awake()
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        private void Start()
        {
            _meshRenderer.material = ServiceLocator.I.TerrainData.GetMainTerrainMaterial();
        }

        public void SetData(TerrainChunkMeshData meshData)
        {
            _meshFilter.mesh = new Mesh()
            {
                vertices = meshData.Vertices,
                uv = meshData.UVs,
                triangles = meshData.Triangles,
                colors32 = meshData.Colors32,
                normals =  meshData.Normals,
            };
            if(meshData.Normals == null || meshData.Normals.Length == 0) _meshFilter.mesh.RecalculateNormals();
            _lod = meshData.Lod;
        }
    }
}
