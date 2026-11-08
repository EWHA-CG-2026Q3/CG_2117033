using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BowlMesh : MonoBehaviour
{
    [Range(3, 64)] public int segments = 32;   // 원 위의 점 개수 n
    [Range(1, 16)] public int rings = 6;       // 벽을 위아래로 몇 층으로 나눌지
    public float bottomRadius = 0.10f;         // 바닥 바깥 반지름
    public float topRadius = 0.14f;            // 위 바깥 반지름
    public float height = 0.07f;               // 그릇 높이
    public float wallThickness = 0.012f;       // 벽 두께
    public float bottomThickness = 0.01f;      // 바닥 두께
    [Range(0f, 1f)] public float curve = 0.5f; // 0이면 곧은 옆면, 1에 가까울수록 둥글게 부푼 옆면

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    void OnEnable() { Build(); }
    void OnValidate() { Build(); }

    // 높이 비율 t(0 = 바닥, 1 = 위)에서 바깥 벽의 반지름
    public float OuterRadiusAt(float t)
    {
        // 기본은 bottomRadius → topRadius로 곧게 넓어짐, curve만큼 옆으로 둥글게 부풂
        float straight = Mathf.Lerp(bottomRadius, topRadius, t);
        float bulge = Mathf.Sin(t * Mathf.PI) * curve * (topRadius - bottomRadius);
        return straight + bulge;
    }

    // 그릇 안쪽, 바닥에서 높이 y(로컬)인 곳의 안쪽 반지름 — 물 표면 크기를 맞출 때 씀
    public float InnerRadiusAtHeight(float y)
    {
        return OuterRadiusAt(Mathf.Clamp01(y / height)) - wallThickness;
    }

    void Build()
    {
        vertices.Clear(); uvs.Clear(); triangles.Clear();

        // 1. 바깥 벽: 바닥(0)부터 위(height)까지, 바깥에서 볼 때 앞면
        AddWall(0f, height, 0f, false);

        // 2. 안쪽 벽: 안쪽 바닥(bottomThickness)부터 위까지, 반지름은 벽 두께만큼 작게, 안에서 볼 때 앞면
        AddWall(bottomThickness, height, wallThickness, true);

        // 3. 위 테두리 고리: 바깥 벽 맨 위 원과 안쪽 벽 맨 위 원 사이를 막음 (위에서 볼 때 앞면)
        AddRim(height, OuterRadiusAt(1f) - wallThickness, OuterRadiusAt(1f));

        // 4. 바깥 바닥: 아래에서 볼 때 앞면 / 5. 안쪽 바닥: 위에서 볼 때 앞면
        AddDisc(0f, OuterRadiusAt(0f), false);
        AddDisc(bottomThickness, InnerRadiusAtHeight(bottomThickness), true);

        Mesh mesh = new Mesh();
        mesh.name = "Bowl";
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    // 벽 하나: (rings + 1)개의 원을 아래에서 위로 쌓고, 이웃한 두 원 사이를 사각형(삼각형 2개)으로 이음
    // inward = true면 안쪽 벽 → 삼각형 순서를 반대로
    void AddWall(float y0, float y1, float shrink, bool inward)
    {
        int start = vertices.Count;          // 이 벽의 첫 정점 번호
        int perRing = segments + 1;          // 한 원의 점 개수 (마지막 점 = 첫 점 자리)

        for (int j = 0; j <= rings; j++)
        {
            float y = Mathf.Lerp(y0, y1, (float)j / rings);   // 이 층의 높이
            float r = OuterRadiusAt(y / height) - shrink;     // 이 층 원의 반지름 

            for (int i = 0; i <= segments; i++)
            {
                float angle = 360f / segments * i * Mathf.Deg2Rad;   // S16 슬라이드 19
                vertices.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                uvs.Add(new Vector2((float)i / segments, (float)j / rings));
            }
        }

        for (int j = 0; j < rings; j++)
        {
            for (int i = 0; i < segments; i++)
            {
                int a = start + j * perRing + i;   // 아래층 i
                int b = a + 1;                     // 아래층 i+1
                int c = a + perRing;               // 위층 i
                int d = c + 1;                     // 위층 i+1

                if (!inward)
                {
                    // 바깥 벽
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(b); triangles.Add(c); triangles.Add(d);
                }
                else
                {
                    // 안쪽 벽: 순서를 뒤집어 안에서 볼 때 앞면
                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                    triangles.Add(b); triangles.Add(d); triangles.Add(c);
                }
            }
        }
    }

    // 위 테두리 고리: 같은 높이의 작은 원(안)과 큰 원(밖) 사이를 사각형으로 이음, 위에서 볼 때 앞면
    void AddRim(float y, float innerR, float outerR)
    {
        int start = vertices.Count;
        for (int i = 0; i < segments; i++)
        {
            float angle = 360f / segments * i * Mathf.Deg2Rad;
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            vertices.Add(new Vector3(c * innerR, y, s * innerR));   // 짝수 번호: 안쪽 점
            vertices.Add(new Vector3(c * outerR, y, s * outerR));   // 홀수 번호: 바깥 점
            uvs.Add(new Vector2(c * 0.5f + 0.5f, s * 0.5f + 0.5f) * (innerR / outerR));
            uvs.Add(new Vector2(c * 0.5f + 0.5f, s * 0.5f + 0.5f));
        }
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int inI = start + i * 2, outI = inI + 1;
            int inN = start + next * 2, outN = inN + 1;
            triangles.Add(inI); triangles.Add(inN); triangles.Add(outI);
            triangles.Add(outI); triangles.Add(inN); triangles.Add(outN);
        }
    }

    // 원판: 가운데 점 + 원 위의 이웃한 두 점
    // faceUp = true면 위에서 볼 때 앞면, false면 아래에서 볼 때 앞면 
    void AddDisc(float y, float r, bool faceUp)
    {
        int center = vertices.Count;
        vertices.Add(new Vector3(0f, y, 0f));
        uvs.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i < segments; i++)
        {
            float angle = 360f / segments * i * Mathf.Deg2Rad;
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            vertices.Add(new Vector3(c * r, y, s * r));
            uvs.Add(new Vector2(c * 0.5f + 0.5f, s * 0.5f + 0.5f));
        }
        for (int i = 0; i < segments; i++)
        {
            int cur = center + 1 + i;
            int next = center + 1 + (i + 1) % segments;
            if (faceUp) { triangles.Add(center); triangles.Add(next); triangles.Add(cur); }
            else        { triangles.Add(center); triangles.Add(cur); triangles.Add(next); }
        }
    }
}
