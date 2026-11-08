using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CylinderMesh : MonoBehaviour
{
    [Range(3, 64)] public int segments = 24;   // 원 위의 점 개수 n 
    public float radius = 0.15f;               // 반지름
    public float height = 1.2f;                // 높이

    void OnEnable() { Build(); }
    void OnValidate() { Build(); }             

    void Build()
    {
        int n = segments;

        // 정점 배치 
        // [0]           : 아래 뚜껑 중심
        // [1 ~ n]       : 아래 뚜껑 둘레
        // [n+1]         : 위 뚜껑 중심
        // [n+2 ~ 2n+1]  : 위 뚜껑 둘레
        // [2n+2 ~ 3n+1] : 옆면 아래 둘레
        // [3n+2 ~ 4n+1] : 옆면 위 둘레
        Vector3[] vertices = new Vector3[4 * n + 2];
        int bottomCenter = 0, bottomRing = 1;
        int topCenter = n + 1, topRing = n + 2;
        int sideBottom = 2 * n + 2, sideTop = 3 * n + 2;

        vertices[bottomCenter] = new Vector3(0f, 0f, 0f);
        vertices[topCenter] = new Vector3(0f, height, 0f);

        for (int i = 0; i < n; i++)
        {
            float angle = 360f / n * i * Mathf.Deg2Rad;      // 점 i의 각도 
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[bottomRing + i] = new Vector3(x, 0f, z);
            vertices[topRing + i]    = new Vector3(x, height, z);
            vertices[sideBottom + i] = new Vector3(x, 0f, z);
            vertices[sideTop + i]    = new Vector3(x, height, z);
        }

        // 삼각형: 뚜껑 n개씩 + 옆면 2n개
        int[] triangles = new int[n * 3 * 4];
        int t = 0;
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;   // 마지막 점은 첫 점과 이어짐

            // 아래 뚜껑 (아래에서 볼 때 앞면)
            triangles[t++] = bottomCenter;
            triangles[t++] = bottomRing + i;
            triangles[t++] = bottomRing + next;

            // 위 뚜껑 (위에서 볼 때 앞면) — 아래 뚜껑과 순서가 반대
            triangles[t++] = topCenter;
            triangles[t++] = topRing + next;
            triangles[t++] = topRing + i;

            // 옆면 사각형 = 삼각형 2개 (바깥에서 볼 때 앞면)
            triangles[t++] = sideBottom + i;
            triangles[t++] = sideTop + i;
            triangles[t++] = sideBottom + next;

            triangles[t++] = sideBottom + next;
            triangles[t++] = sideTop + i;
            triangles[t++] = sideTop + next;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Cylinder_" + n;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}
