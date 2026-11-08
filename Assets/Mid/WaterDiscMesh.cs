using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterDiscMesh : MonoBehaviour
{
    public BowlMesh bowl;                       // 이 물이 담긴 그릇
    [Range(0f, 1f)] public float fill = 0.7f;   // 물 높이: 0 = 안쪽 바닥, 1 = 그릇 가장자리
    [Range(3, 64)] public int segments = 32;

    void OnEnable() { Build(); }
    void OnValidate() { Build(); }

    void Build()
    {
        if (bowl == null) return;

        // 1. 물 높이와 그 높이에서 그릇 안쪽 반지름 구하기
        float y = Mathf.Lerp(bowl.bottomThickness, bowl.height, fill);
        float r = bowl.InnerRadiusAtHeight(y) * 0.99f;   
        transform.localPosition = new Vector3(0f, y, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // 2. 정점: 가운데 점 + 원 위의 점 n개
        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uv = new Vector2[segments + 1];
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < segments; i++)
        {
            float angle = 360f / segments * i * Mathf.Deg2Rad;
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            vertices[i + 1] = new Vector3(c * r, 0f, s * r);
            uv[i + 1] = new Vector2(c * 0.5f + 0.5f, s * 0.5f + 0.5f);   // 원(-1~1)을 텍스처 범위(0~1)로
        }

        // 3. 삼각형: 위에서 볼 때 앞면
        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = next + 1;
            triangles[i * 3 + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "WaterDisc";
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}
