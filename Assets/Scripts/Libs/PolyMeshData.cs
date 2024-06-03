using System.Linq;
using UnityEngine;

[System.Serializable]
public struct HeightRenderer
{
    public Material Material;
    [Range(0f, 1f)] public float LimitHeight;
}

[System.Serializable]
public struct PolyMeshRendererSettings
{
    public Material BaseMaterial;
    public Material TopMaterial;
    public HeightRenderer[] HeightsRenderer;
}

public class PolyMeshData
{
    public const int MinPolyLen = 3;
    private const float LineCurvePower = 0.5645f;

    private int m_PolyLen;
    private int m_Sampling;
    private float[] m_BaseSizes;
    private float[] m_TopSizes;
    private float m_FloretFactor;
    private float[] m_SummitsAngles;
    private float[] m_PolyHeights;
    private float m_Slope;
    private bool m_Curving;
    private float m_CutOff;
    private float[] m_MeshHeights;
    private float m_RendFactor;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[][] subMeshTriangles;

    public int PolyLen 
    { 
        get => (Floret ? 3 : 1) * m_PolyLen;
        set
        {
            m_PolyLen = Mathf.Clamp(value, MinPolyLen, int.MaxValue);
            BaseSizes = m_BaseSizes;
            TopSizes = m_TopSizes;
            SummitsAngles = m_SummitsAngles;
            PolyHeights = m_PolyHeights;
        }
    }

    public int Sampling 
    { 
        get => m_Sampling; 
        set => m_Sampling = Mathf.Clamp(value, 1, int.MaxValue); 
    }

    public float[] BaseSizes 
    { 
        get => m_BaseSizes; 
        set => m_BaseSizes = SetSizeArray(value, PolyLen, value != null && value.Length > 0 ? value[0] : 1f, 0f, float.MaxValue); 
    }

    public float[] TopSizes 
    { 
        get => m_TopSizes; 
        set => m_TopSizes = SetSizeArray(value, PolyLen, value != null && value.Length > 0 ? value[0] : 1f, 0f, float.MaxValue); 
    }

    public float FloretFactor 
    { 
        get => m_FloretFactor;
        set
        {
            m_FloretFactor = Mathf.Clamp(value, 0f, 1f);
            PolyLen = m_PolyLen;
        }
    }

    public float[] SummitsAngles
    {
        get => m_SummitsAngles;
        set => m_SummitsAngles = SetSizeArray(value, PolyLen);
    }

    public float[] PolyHeights 
    { 
        get => m_PolyHeights;
        set
        {
            m_PolyHeights = SetSizeArray(value, PolyLen, value != null && value.Length > 0 ? value[0] : 1f, 0f, float.MaxValue);
            CutOff = m_CutOff;
        }
    }

    public float Slope 
    { 
        get => m_Slope; 
        set => m_Slope = Mathf.Clamp(value, -1f, 1f); 
    }

    public bool Curving
    { 
        get => m_Curving; 
        set => m_Curving = value; 
    }

    public float CutOff 
    { 
        get => m_CutOff; 
        set => m_CutOff = Mathf.Clamp(value, Mathf.Min(0f, PolyHeights.Min()), Mathf.Max(0f, PolyHeights.Min())); 
    }

    public float[] MeshHeights 
    { 
        get => m_MeshHeights; 
        set => m_MeshHeights = value; 
    }

    public float RendFactor 
    { 
        get => m_RendFactor; 
        set => m_RendFactor = Mathf.Clamp(value, 0f, 1f); 
    }

    public bool Floret => FloretFactor != 0f;
    public int BaseLen => 1 + 2 * PolyLen;
    public int MiddleLen => 2 * PolyLen * Sampling;
    public int TopLen => 1 + Mathf.RoundToInt(Sampling * (Sampling + 1) * (PolyLen / 2f));
    public int VerticesLen => BaseLen + MiddleLen + TopLen;
    public float CenterHeight => Lerp3(PolyHeights.Min(), PolyHeights.Sum() / PolyLen, PolyHeights.Max(), Slope);

    private float[] SetSizeArray(float[] array, int size, float defaultValue = 0f, float minValue = float.MinValue, float maxValue = float.MaxValue)
    {
        float[] result = new float[size];
        for (int index = 0; index < size; index++)
        {
            result[index] = array != null && index < array.Length ? array[index] : defaultValue;
            result[index] = Mathf.Clamp(result[index], minValue, maxValue);
        }

        return result;
    }

    private float Lerp3(float a, float b, float c, float t)
    {
        t = Mathf.Clamp(t, -1f, 1f);

        return t < 0f ? Mathf.Lerp(a, b, t + 1f) : Mathf.Lerp(b, c, t);
    }

    public PolyMeshData()
    {
        float[] baseSizes = new float[] { 1f, 1f, 1f };
        float[] topSizes = new float[] { 1f, 1f, 1f };
        float[] summitsAngles = new float[] { 2 * Mathf.PI / 3, 4 * Mathf.PI / 3, 2 * Mathf.PI };
        float[] polyHeights = new float[] { 1f, 1f, 1f };

        SetSummits(MinPolyLen, 1, 0f, baseSizes, topSizes, summitsAngles);
        SetPolyHeights(polyHeights, 0f, false, 0f);
        SetMeshHeights(new float[0], 0f);
    }

    public PolyMeshData(int polyLen, int sampling, float[] baseSizes, float[] topSizes, float floretFactor, float[] summitsAngles, 
        float[] polyHeights, float slope, bool curving, float cutOff, float[] subMeshsHeights, float rendFactor)
    {
        SetSummits(polyLen, sampling, floretFactor, baseSizes, topSizes, summitsAngles);
        SetPolyHeights(polyHeights, slope, curving, cutOff);
        SetMeshHeights(subMeshsHeights, rendFactor);
    }

    public void SetSummits(int polyLen, int sampling = 1, float floretFactor = 0f, 
        float[] baseSizes = null, float[] topSizes = null, float[] summitsAngles = null)
    {
        PolyLen = polyLen;
        Sampling = sampling;
        FloretFactor = floretFactor;

        BaseSizes = baseSizes;
        TopSizes = topSizes;
        SummitsAngles = summitsAngles;
    }

    public void SetPolyHeights(float[] polyHeights, float slope = 0f, bool curving = false, float cutOff = 0f)
    {
        PolyHeights = polyHeights;
        Slope = slope;
        Curving = curving;
        CutOff = cutOff;
    }

    public void SetMeshHeights(float[] meshHeights, float rendFactor = 0f)
    {
        MeshHeights = meshHeights;
        RendFactor = rendFactor;
    }

    public void SetToRegularPolygon(int polyLen, int sampling, float floretFactor, float baseSize, float topSize, float angleOffset)
    {
        PolyLen = polyLen;
        Sampling = sampling;
        FloretFactor = floretFactor;

        BaseSizes = new float[PolyLen].Select(_ => baseSize).ToArray();
        TopSizes = new float[PolyLen].Select(_ => topSize).ToArray();

        float polyAngle = 2f * Mathf.PI / PolyLen;
        SummitsAngles = new float[PolyLen].Select((_, polyIndex) => angleOffset + polyIndex * polyAngle).ToArray(); ;
    }

    public void UseSingleHeight(float height)
    {
        PolyHeights = new float[PolyLen].Select(_ => height).ToArray();
    }

    public Mesh CreateMesh()
    {
        SetVertices();
        SetTriangles();
        SetUVs();

        Mesh mesh = new()
        {
            vertices = vertices,
            subMeshCount = subMeshTriangles.Length,
            uv = uvs,
        };

        for (int index = 0; index < subMeshTriangles.Length; index++)
        {
            mesh.SetTriangles(subMeshTriangles[index], index);
        }

        mesh.RecalculateNormals();
        return mesh;
    }

    private void SetVertices()
    {
        vertices = new Vector3[VerticesLen];

        vertices[0] = Vector3.zero;

        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
        {
            float angle = SummitsAngles[polyIndex];

            bool isFloretSummit = polyIndex % 3 == 0;
            float factor = 1f - (Floret && isFloretSummit ? FloretFactor : 0f);

            if (!isFloretSummit)
            {
                angle += Mathf.Deg2Rad * (Floret ? polyIndex % 3 == 1 ? -0.85f : 0.85f : 0f);
            }

            Vector3 dir = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            vertices[BI(0, polyIndex)] = BaseSizes[polyIndex] * factor * dir;
            vertices[TI(Sampling, polyIndex)] = TopSizes[polyIndex] * factor * dir;
        }

        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
        {
            int nextPolyIndex = (polyIndex + 1) % PolyLen;

            for (int sample = 0; sample < Sampling; sample++)
            {
                float samplingRatio = (float)sample / Sampling;

                float height = Mathf.Lerp(PolyHeights[polyIndex], PolyHeights[nextPolyIndex], samplingRatio);
                float cutRatio = height != 0f ? (height - CutOff) / height : 1f;

                Vector3 polyVertex = Vector3.Lerp(vertices[BI(0, polyIndex)], vertices[TI(Sampling, polyIndex)], cutRatio);
                Vector3 nextPolyVertex = Vector3.Lerp(vertices[BI(0, nextPolyIndex)], vertices[TI(Sampling, nextPolyIndex)], cutRatio);
                Vector3 vertex = Vector3.Lerp(polyVertex, nextPolyVertex, samplingRatio);

                vertices[MI(0, polyIndex, sample)] = new Vector3(vertex.x, height - CutOff, vertex.z);
                vertices[MI(1, polyIndex, sample)] = new Vector3(vertex.x, height, vertex.z);
            }

            vertices[BI(1, polyIndex)] = vertices[MI(0, polyIndex)];
        }

        vertices[VerticesLen - 1] = new Vector3(0f, CenterHeight, 0f);

        for (int subSampling = Sampling; subSampling > 0; subSampling--)
        {
            float samplingRatio = (float)subSampling / Sampling;

            for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
            {
                int nextPolyIndex = (polyIndex + 1) % PolyLen;

                for (int sample = 0; sample < subSampling; sample++)
                {
                    float subSamplingRatio = (float)sample / subSampling;

                    float latHeight = samplingRatio * Mathf.Lerp(PolyHeights[polyIndex], PolyHeights[nextPolyIndex], subSamplingRatio);
                    float longHeight = (1 - samplingRatio) * CenterHeight;
                    float height = latHeight + longHeight;

                    Vector3 vertex = samplingRatio * Vector3.Lerp(vertices[TI(Sampling, polyIndex)], vertices[TI(Sampling, nextPolyIndex)], subSamplingRatio);

                    if (Curving)
                    {
                        float curvePower = Lerp3(0f, LineCurvePower, 2 * Mathf.PI, Slope);
                        float curveFactor = Mathf.Pow(1f - Mathf.Sin((1 - samplingRatio) * Mathf.PI / 2), curvePower);

                        height = Mathf.Lerp(CenterHeight, latHeight, curveFactor);
                    }

                    vertices[TI(subSampling, polyIndex, sample)] = new Vector3(vertex.x, height, vertex.z);
                }
            }
        }
        
    }

    private void SetUVs()
    {
        uvs = new Vector2[VerticesLen];

        uvs[0] = new Vector2(0.5f, 0.5f);
        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex += 2)
        {
            int nextPolyIndex = (polyIndex + 1) % PolyLen;

            uvs[BI(0, polyIndex)] = new Vector2(0f, 0f);
            uvs[BI(0, nextPolyIndex)] = new Vector2(1f, 0f);
            uvs[BI(1, polyIndex)] = new Vector2(0f, 1f);
            uvs[BI(1, nextPolyIndex)] = new Vector2(1f, 1f);
        }

        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex += 2)
        {
            int nextPolyIndex = (polyIndex + 1) % PolyLen;

            for (int sample = 0; sample < Sampling; sample++)
            {
                float samplingRatio = (float)sample / Sampling;
                float nextSamplingRatio = (float)((sample + 1) % Sampling) / Sampling;

                uvs[MI(0, polyIndex, sample)] = new Vector2(samplingRatio, samplingRatio);
                uvs[MI(0, nextPolyIndex, sample)] = new Vector2(nextSamplingRatio, samplingRatio);
                uvs[MI(1, polyIndex, sample)] = new Vector2(samplingRatio, nextSamplingRatio);
                uvs[MI(1, nextPolyIndex, sample)] = new Vector2(nextSamplingRatio, nextSamplingRatio);
            }
        }

        float maxSize = TopSizes.Max();
        for (int index = BaseLen + MiddleLen; index < VerticesLen; index++)
        {
            uvs[index] = new Vector2(vertices[index].x / maxSize, vertices[index].z / maxSize);
        }
    }

    private void SetTriangles()
    {
        subMeshTriangles = new int[][] { new int[] { }, new int[] { }, };

        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
        {
            int nextPolyIndex = (polyIndex + 1) % PolyLen;
            subMeshTriangles[0] = subMeshTriangles[0].Concat(new int[] { 0, BI(0, polyIndex), BI(0, nextPolyIndex) }).ToArray();
            subMeshTriangles[0] = subMeshTriangles[0].Concat(new int[]
            {
                BI(1, polyIndex), BI(0, nextPolyIndex), BI(0, polyIndex),
                BI(0, nextPolyIndex), BI(1, polyIndex), BI(1, nextPolyIndex),
            }).ToArray();
        }

        for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
        {
            int nextPolyIndex = (polyIndex + 1) % PolyLen;
            for (int sample = 0; sample < Sampling; sample++)
            {
                int index = sample + 1 < Sampling ? polyIndex : nextPolyIndex;
                int nextSample = (sample + 1) % Sampling;

                subMeshTriangles[1] = subMeshTriangles[1].Concat(new int[]
                {
                    MI(0, polyIndex, sample), MI(1, polyIndex, sample), MI(1, index, nextSample),
                    MI(1, index, nextSample), MI(0, index, nextSample), MI(0, polyIndex, sample),

                }).ToArray();
            }
        }
        
        for (int index = 0; index < MeshHeights.Length; index++)
        {
            subMeshTriangles = subMeshTriangles.Append(new int[0]).ToArray();
        }

        for (int subSampling = Sampling; subSampling > 0; subSampling--)
        {
            for (int polyIndex = 0; polyIndex < PolyLen; polyIndex++)
            {
                int nextPolyIndex = (polyIndex + 1) % PolyLen;

                for (int sample = 0; sample < subSampling; sample++)
                {
                    int nextSample = (sample + 1) % subSampling;

                    int vertexIndex1 = TI(subSampling, polyIndex, sample);
                    int vertexIndex2 = TI(subSampling - 1, sample < subSampling - 1 ? polyIndex : nextPolyIndex, sample < subSampling - 1 ? sample : nextSample);
                    int vertexIndex3 = TI(subSampling, sample + 1 < subSampling ? polyIndex : nextPolyIndex, nextSample);

                    int subMeshIndex = GetSubMeshIndex(vertexIndex1, vertexIndex2, vertexIndex3);

                    subMeshTriangles[subMeshIndex] = subMeshTriangles[subMeshIndex].Concat(new int[] { vertexIndex1, vertexIndex2, vertexIndex3 }).ToArray();

                    if (sample < subSampling - 1)
                    {
                        int nextSubSample = (sample + 1) % (subSampling - 1);

                        vertexIndex1 = TI(subSampling - 1, polyIndex, sample);
                        vertexIndex2 = TI(subSampling - 1, sample + 1 < subSampling - 1 ? polyIndex : nextPolyIndex, nextSubSample);
                        vertexIndex3 = TI(subSampling, polyIndex, nextSample);

                        subMeshIndex = GetSubMeshIndex(vertexIndex1, vertexIndex2, vertexIndex3);

                        subMeshTriangles[subMeshIndex] = subMeshTriangles[subMeshIndex].Concat(new int[] { vertexIndex1, vertexIndex2, vertexIndex3 }).ToArray();
                    }
                }
            }
        }
    }

    private void ReSetUVs()
    {
        float maxSize = TopSizes.Max();

        uvs = new Vector2[VerticesLen];
        for (int index = 0; index < vertices.Length; index++)
        {
            uvs[index] = new Vector2(vertices[index].x / maxSize, vertices[index].z / maxSize);
        }
    }

    private int BI(int layer, int polyIndex) => 1 + layer * PolyLen + polyIndex;
    private int MI(int layer, int polyIndex, int sample = 0) => BaseLen + Sampling * (layer * PolyLen + polyIndex) + sample; 
    private int TI(int subSampling, int polyIndex, int sample = 0)
    {
        int number = Mathf.RoundToInt(PolyLen / 2f * (Sampling * (Sampling + 1) - subSampling * (subSampling + 1)));

        return BaseLen + MiddleLen + number + (polyIndex * subSampling) + sample;
    }

    private int GetSubMeshIndex(int vertexIndex1, int vertexIndex2, int vertexIndex3)
    {
        if (MeshHeights.Length == 0) { return 1; }

        float minHeight = Mathf.Min(vertices[vertexIndex1].y, vertices[vertexIndex2].y, vertices[vertexIndex3].y);
        float middleHeight = (vertices[vertexIndex1].y + vertices[vertexIndex2].y + vertices[vertexIndex3].y) / 3f;
        float maxHeight = Mathf.Max(vertices[vertexIndex1].y, vertices[vertexIndex2].y, vertices[vertexIndex3].y);

        float factor = 2f * (RendFactor - 0.5f);
        float height = Lerp3(minHeight, middleHeight, maxHeight, factor);

        for (int index = 0; index < MeshHeights.Length; index++)
        {
            if (height <= MeshHeights[index]) { return index + 2; }
        }

        return MeshHeights.Length + 1;
    }
}
