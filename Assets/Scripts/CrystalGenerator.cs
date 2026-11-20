using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CrystalGenerator : MonoBehaviour
{
    [Header("Shape")]
    [Range(3, 12)] public int sides = 6;
    public float radius = 1f;
    public float height = 2f;
    [Range(0f, 0.5f)] public float noise = 0.15f;
    public int seed = 12345;

    [Header("Top point")]
    public bool pointedTop = true;
    public float topHeight = 0.8f;

    private Mesh _mesh;

    void OnEnable()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
            Generate();
    }

    void OnValidate()
    {
        if (GetComponent<MeshFilter>().sharedMesh != null)
            Generate();
    }

    [ContextMenu("Generate Crystal")]
    public void Generate()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh { name = "ProceduralCrystal" };
        }
        else
        {
            _mesh.Clear();
        }

        var rand = new System.Random(seed);

        Vector3[] bottomRing = new Vector3[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            float r = radius + (float)(rand.NextDouble() * 2 - 1) * noise;
            bottomRing[i] = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
        }

        Vector3[] topRing = new Vector3[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            float r = radius * 0.7f + (float)(rand.NextDouble() * 2 - 1) * noise * 0.5f;
            topRing[i] = new Vector3(Mathf.Cos(angle) * r, height, Mathf.Sin(angle) * r);
        }

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        vertices.Add(Vector3.zero); // 0
        for (int i = 0; i < sides; i++) vertices.Add(bottomRing[i]);
        for (int i = 0; i < sides; i++)
        {
            int a = 0;
            int b = 1 + i;
            int c = 1 + (i + 1) % sides;
            triangles.Add(a); triangles.Add(b); triangles.Add(c);
        }

        int bottomStart = vertices.Count;
        for (int i = 0; i < sides; i++) vertices.Add(bottomRing[i]);
        int topStart = vertices.Count;
        for (int i = 0; i < sides; i++) vertices.Add(topRing[i]);

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            int b0 = bottomStart + i;
            int b1 = bottomStart + next;
            int t0 = topStart + i;
            int t1 = topStart + next;

            triangles.Add(b0); triangles.Add(t0); triangles.Add(t1);
            triangles.Add(b0); triangles.Add(t1); triangles.Add(b1);
        }

        if (pointedTop)
        {
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, height + topHeight, 0f));
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                triangles.Add(topCenter);
                triangles.Add(topStart + next);
                triangles.Add(topStart + i);
            }
        }
        else
        {
            int topCenterIdx = vertices.Count;
            vertices.Add(new Vector3(0f, height, 0f));
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                triangles.Add(topCenterIdx);
                triangles.Add(topStart + next);
                triangles.Add(topStart + i);
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = _mesh;

        var renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material")
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            mat.color = new Color(0.3f, 0.8f, 1f, 0.6f);

            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetFloat("_Metallic", 0.2f);

            renderer.sharedMaterial = mat;
        }
    }
}