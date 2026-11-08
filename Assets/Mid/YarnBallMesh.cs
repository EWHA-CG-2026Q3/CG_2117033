using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class YarnBallMesh : MonoBehaviour
{
    public float radius = 0.08f;               // 실뭉치 반지름
    [Range(3, 64)] public int slices = 32;     // 한 층의 원 위의 점 개수 
    [Range(2, 64)] public int stacks = 24;     // 위에서 아래로 층 개수 
    public float grooveDepth = 0.06f;          // 실 홈 깊이 (반지름의 비율, 0이면 매끈한 구)
    public float strands = 10f;                // 실 가닥 수

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] baseNormals;

    public Vector3[] BaseVertices => baseVertices;
    public Vector3[] BaseNormals => baseNormals;

    void OnEnable() { Build(); }
    void OnValidate() { Build(); }

    void Build()
    {
        // 정점: (stacks + 1)층 × 층마다 (slices + 1)개 (마지막 점은 첫 점과 같은 자리)
        baseVertices = new Vector3[(stacks + 1) * (slices + 1)];
        Vector2[] uv = new Vector2[baseVertices.Length];

        for (int j = 0; j <= stacks; j++)
        {
            float theta = 180f / stacks * j * Mathf.Deg2Rad;   // 위에서 잰 각도: 0(북극) → π(남극)
            float y = Mathf.Cos(theta);                       // 이 층의 높이
            float ringRadius = Mathf.Sin(theta);              // 이 층 원의 반지름 (적도에서 가장 큼)

            for (int i = 0; i <= slices; i++)
            {
                float phi = 360f / slices * i * Mathf.Deg2Rad; // 원 위의 각도 

                // 실이 비스듬히 감긴 홈: 각도에 따라 반지름을 살짝 줄였다 늘렸다
                float r = radius * (1f - grooveDepth * (0.5f + 0.5f * Mathf.Sin(strands * (phi + 2f * theta))));

                int k = j * (slices + 1) + i;
                baseVertices[k] = new Vector3(Mathf.Cos(phi) * ringRadius, y, Mathf.Sin(phi) * ringRadius) * r;
                uv[k] = new Vector2((float)i / slices, 1f - (float)j / stacks);
            }
        }

        // 삼각형: 위아래로 이웃한 두 층 사이의 사각형마다 삼각형 2개
        int[] triangles = new int[stacks * slices * 6];
        int t = 0;
        for (int j = 0; j < stacks; j++)
        {
            for (int i = 0; i < slices; i++)
            {
                int a = j * (slices + 1) + i;   // 위층 i
                int b = a + 1;                  // 위층 i+1
                int c = a + (slices + 1);       // 아래층 i
                int d = c + 1;                  // 아래층 i+1

                triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
                triangles[t++] = b; triangles[t++] = d; triangles[t++] = c;
            }
        }

        mesh = new Mesh();
        mesh.name = "YarnBall";
        mesh.vertices = baseVertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        baseNormals = mesh.normals;
        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    
    public void SetVertices(Vector3[] newVertices, Vector3[] newNormals)
    {
        if (mesh == null) return;
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.RecalculateBounds();
    }
}
