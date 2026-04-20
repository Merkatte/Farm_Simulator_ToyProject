// 씨앗 종류별 성장 타이밍 데이터. 씨앗을 심을 때 FarmCell에 주입되며, 칸이 아닌 씨앗이 성장 속도를 결정한다.
using UnityEngine;

[CreateAssetMenu(fileName = "SeedConfig", menuName = "Project_FAD/SeedConfig")]
public class SeedConfig : ScriptableObject {
    [Header("Growth Timings (seconds)")]
    public float SeededToGrowingSeconds = 5f;
    public float GrowingToGrownSeconds = 10f;
}
