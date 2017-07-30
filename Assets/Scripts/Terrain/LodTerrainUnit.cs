using Helper;
using UnityEngine;

public class LodTerrainUnit : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Material _material;
    private int _myX;
    private int _myZ;
    private int _lodLevel = -1;
    private LodInterpolatedMeshBuilder _lodBuilder;
    private Int2 _origin;
    public bool SeaBed { get; private set; }
    private TerrainMeshBuilder _builder;
    
    public LodTerrainUnit Init(BuilderData data, Material material, TerrainMeshBuilder meshBuilder)
    {
        _myX = Mathf.FloorToInt(transform.position.x / Consts.UnitSize);
        _myZ = Mathf.FloorToInt(transform.position.z / Consts.UnitSize);
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _material = material;
        _meshRenderer.material = _material;
        _lodBuilder = new LodInterpolatedMeshBuilder(meshBuilder, data);
        meshBuilder.OnCamPositionChanged += CheckLod;
        _origin = data.Origin;
        SeaBed = data.SeaBed;
        CheckLod(0, 0);
        return this;
    }

    private void CheckLod(int cx, int cz)
    {
        var unitLod = GetUnitLod(_myX, _myZ, cx, cz);
        SetLodLevel(unitLod, GetNeighborsLodsStates(cx,cz, unitLod));
    }

    private void SetLodLevel(int lodLevel, bool[] neighbors)
    {
        if(_lodLevel == lodLevel) return;
        var meshData = _lodBuilder.GetMeshData(lodLevel, neighbors);
        _meshFilter.mesh = new Mesh()
        {
            vertices = meshData.Vertices,
            triangles = meshData.Triangles,
            uv = meshData.UVs
        };
        _meshFilter.mesh.RecalculateNormals();
        _lodLevel = lodLevel;
    }

    private int GetUnitLod(int unitX, int unitZ, int cx, int cz)
    {
        var x = Mathf.Abs(unitX - cx);
        var z = Mathf.Abs(unitZ - cz);
        return x > Consts.LodsAmount || z > Consts.LodsAmount ? Consts.LodsAmount : Mathf.Clamp((x > z ? x : z) - 1, 0, Consts.LodsAmount);
    }

    private bool[] GetNeighborsLodsStates(int cx, int cz, int unitLod)
    {
        return new bool[]
        {
            unitLod < GetUnitLod(_myX - 1, _myZ, cx, cz),
            unitLod < GetUnitLod(_myX, _myZ + 1, cx, cz),
            unitLod < GetUnitLod(_myX + 1, _myZ, cx, cz),
            unitLod < GetUnitLod(_myX, _myZ - 1, cx, cz),
        };
    }

}
