using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S03_CustomPolygon : MonoBehaviour
{
    void Start()
    {
        // TODO 1: 원하는 다각형의 정점 좌표를 채우세요 (최소 4개)
        Vector3[] vertices = new Vector3[]
        {
              // 예: new Vector3(0f, 1f, 0f),
            new Vector3(0f, 1f, 0f), // 0
            new Vector3(0f, 2f, 0f), // 1
            new Vector3(1f, 3f, 0f), // 2
            new Vector3(2f, 3f, 0f), // 3
            new Vector3(3f, 2f, 0f), // 4
            new Vector3(3f, 1f, 0f), // 5
            new Vector3(2f, 0f, 0f), // 6
            new Vector3(1f, 0f, 0f), // 7

        };

        // TODO 2: 정점 3개씩 묶어 삼각형들을 구성하세요
        int[] triangles = new int[]
        {
              // 예: 0, 1, 2,
            2,1,0,
            2,0,7,
            2,7,6,
            2,6,5,
            2,5,4,
            2,4,3


        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}