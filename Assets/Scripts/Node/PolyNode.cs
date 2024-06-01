using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PolyNode : MonoBehaviour
{
    [field: SerializeField, Range(3, 45), Min(3)] public int PolyLen { get; set; } = 6;
    [field: SerializeField, Range(1, 35), Min(1)] public int Sampling { get; set; } = 7;
    [field: SerializeField, Min(0f)] public float[] BaseSizes { get; set; } = new float[6];
    [field: SerializeField, Min(0f)] public float[] TopSizes { get; set; } = new float[6];
    [field: SerializeField] public float[] SummitsAngles { get; set;} = new float[6];
    [field: SerializeField, Min(0f)] public float[] PolyHeights { get; set; } = new float[6];
    [field: SerializeField, Range(-1f, 1f)] public float Slope { get; set; } = 0f;
    [field: SerializeField] public bool Curving { get; set; } = false;
    [field: SerializeField] public float CutOff { get; set; } = 0.25f;
    [field: SerializeField] public PolyMeshRendererSettings RendererSettings { get; set; }
    [field: SerializeField, Range(0f, 1f)] public float RendFactor { get; set; } = 0f;

    private void OnValidate()
    {
        RendNode();
    }

    public void RendNode()
    {
        float[] summitsAngles = SummitsAngles.Select(angle => angle * Mathf.Deg2Rad).ToArray();

        CutOff = Mathf.Clamp(CutOff, Mathf.Min(0f, Mathf.Min(PolyHeights)), Mathf.Max(0f, Mathf.Min(PolyHeights)));

        float[] subMeshHeights = RendererSettings.HeightsRenderer.Select(rend => rend.LimitHeight * PolyHeights.Max()).ToArray();

        PolyMeshData meshData = new();

        meshData.SetSummits(PolyLen, Sampling, 0f, BaseSizes, TopSizes, summitsAngles);
        meshData.SetPolyHeights(PolyHeights, Slope, Curving, CutOff);
        meshData.SetMeshHeights(subMeshHeights, RendFactor);

        Mesh mesh = meshData.CreateMesh();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        if (TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider.sharedMesh = mesh;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials = new Material[]
        {
            RendererSettings.BaseMaterial, RendererSettings.TopMaterial
        }.Concat(RendererSettings.HeightsRenderer.Select(heightRenderer => heightRenderer.Material)).ToArray();
    }
}
