using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

public class SimpleMeshBuilder : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _size = 16;
    [SerializeField] private float _scale = 1.0f;

    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector3[] _normals;
    private Vector2[] _uvs;

    private Mesh _finalMesh;
    private void Start()
    {
        StartCoroutine(BuildMesh());
    }

    private IEnumerator BuildMesh()
    {
        var meshfilter = gameObject.AddComponent<MeshFilter>();
        var meshResnderer = gameObject.AddComponent<MeshRenderer>();

        var halfSize = _size / 2;

        _vertices = new Vector3[_size * _size];
        yield return new WaitForSeconds(1.0f);
        for (int x = 0; x < _size; x++)
        {
            for (int z = 0; z < _size; z++)
            {
                _vertices[x * _size + z] = new Vector3(halfSize * _scale - x * _scale, 0, z * _scale - halfSize * _scale);
                Debug.Log(new Vector3(halfSize * _scale - x * _scale, 0, z * _scale - halfSize * _scale));
                yield return new WaitForSeconds(0.1f);
            }
        }

        var trianglesCount = (_size - 1) * (_size - 1) * 2;
        _triangles = new int[trianglesCount * 3];
        int q = 0;
        for (int x = 0; x < _size - 1; x++)
        {
            for (int z = 0; z < _size - 1; z++)
            {
                _triangles[q] = x * _size + z;                
                _triangles[q + 1] = (x + 1) * _size + z;
                _triangles[q + 2] = x * _size + z + 1;

                _triangles[q + 3] = x * _size + z + 1;
                _triangles[q + 4] = (x + 1) * _size + z;
                _triangles[q + 5] = (x + 1) * _size + z + 1;

                q += 6;
            }
        }
        _normals = new Vector3[_size * _size];
        for (int i = 0; i < _normals.Length; i++)
        {
            _normals[i] = Vector3.up;
        }

        _uvs = new Vector2[_size * _size];
        q = 0;
        for (int x = 0; x < _size; x++)
        {
            for (int z = 0; z < _size; z++)
            {
                _uvs[q] = new Vector2((float)x / (_size - 1), (float)z / (_size - 1));
                //_uvs[q + 1] = new Vector2((x + 1) / (float)_size, 0.0f);
                //_uvs[q + 2] = new Vector2(0.0f, (z + 1) / (float)_size);
                //_uvs[q + 3] = new Vector2((x + 1) / (float)_size, (z + 1) / (float)_size);

                q++;
            }
        }
        _finalMesh = new Mesh { name = "runtime_mesh", vertices = _vertices, triangles = _triangles, uv = _uvs };
        //_finalMesh.RecalculateNormals();
        //_normals = _finalMesh.normals;
        _finalMesh.normals = _normals;
        meshfilter.mesh = _finalMesh;
        meshResnderer.material = _material;
    }

    private void OnDrawGizmos()
    {
        if(_vertices == null) return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Gizmos.DrawSphere(_vertices[i], 0.1f);
        }
        if(_normals == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Gizmos.DrawRay(_vertices[i], _normals[i]);
        }
    }
}
