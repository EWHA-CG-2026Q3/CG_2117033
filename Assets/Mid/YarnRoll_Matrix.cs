using UnityEngine;

// 실뭉치가 원을 그리며 바닥을 굴러가기 
//   Rorbit  : Y축으로 돌아서 원을 그림              (= Ball_Orbit)
//   Toffset : 원의 중심에서 반지름만큼 떨어뜨림       (= Ball_Offset)
//   Rspin   : 실뭉치 자신이 굴러가는 회전              (= Ball_Sphere의 자전)
// 굴러간 거리만큼 돌아야 미끄러지지 않음 → 자전 각도 = 굴러간 거리 ÷ 실뭉치 반지름
// YarnBallMesh가 붙은 오브젝트에 함께 붙임. 그 오브젝트의 Transform = 원의 중심 (바닥 위)
[ExecuteAlways]
[RequireComponent(typeof(YarnBallMesh))]
public class YarnRoll_Matrix : MonoBehaviour
{
    public float orbitRadius = 0.7f;    // 그리는 원의 반지름 (캣타워 둘레를 돌게 하려면 받침보다 크게)
    public float orbitSeconds = 10f;    // 원을 한 바퀴 도는 데 걸리는 시간 (음수면 반대 방향)

    YarnBallMesh yarn;

    void LateUpdate()
    {
        if (yarn == null) yarn = GetComponent<YarnBallMesh>();
        if (yarn.BaseVertices == null) return;

        float radius = yarn.radius;
        float time = Application.isPlaying ? Time.time : 0f;

        // 1. 원 위에서 지금 각도와, 지금까지 굴러간 거리
        float orbitAngle = 360f / orbitSeconds * time;                     // 도 단위
        float distance = orbitRadius * orbitAngle * Mathf.Deg2Rad;         // 호의 길이 = 반지름 × 각도(라디안)

        // 2. 굴러간 거리만큼의 자전 각도 (실뭉치 반지름 기준)
        //    원 위의 진행 방향이 −Z라서 X축 기준 음수 방향으로 돎
        float spinAngle = -distance / radius * Mathf.Rad2Deg;

        // 3. 행렬 세 개를 하나로 합침 
        Matrix4x4 Rorbit = Matrix4x4.Rotate(Quaternion.Euler(0f, orbitAngle, 0f));        // 원을 그리며 돎
        Matrix4x4 Toffset = Matrix4x4.Translate(new Vector3(orbitRadius, radius, 0f));    // 중심에서 떨어뜨리고 바닥 위로
        Matrix4x4 Rspin = Matrix4x4.Rotate(Quaternion.Euler(spinAngle, 0f, 0f));          // 제자리에서 굴러가는 회전
        Matrix4x4 M = Rorbit * Toffset * Rspin;
      

        // 4. 꼭짓점마다 한 번 곱함 (법선은 방향이라 MultiplyVector: 이동의 영향을 받지 않음, w = 0)
        Vector3[] baseV = yarn.BaseVertices;
        Vector3[] baseN = yarn.BaseNormals;
        Vector3[] verts = new Vector3[baseV.Length];
        Vector3[] norms = new Vector3[baseN.Length];
        for (int i = 0; i < baseV.Length; i++)
        {
            verts[i] = M.MultiplyPoint(baseV[i]);
            norms[i] = M.MultiplyVector(baseN[i]).normalized;
        }
        yarn.SetVertices(verts, norms);
    }
}
