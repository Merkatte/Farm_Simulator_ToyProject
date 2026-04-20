// 농경지 칸 고유의 설정 데이터. 씨앗과 무관한 Boost 수치와 상태별 색상을 보관한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmCellConfig", menuName = "Project_FAD/FarmCellConfig")]
public class FarmCellConfig : ScriptableObject {
    [Header("Cell Settings")]
    public float BoostReductionSeconds = 2f;

    [Header("State Colors")]
    public Color UntilledColor = new Color(0.55f, 0.40f, 0.22f);
    public Color TilledColor   = new Color(0.35f, 0.22f, 0.10f);
    public Color SeededColor   = new Color(0.70f, 0.65f, 0.30f);
    public Color GrowingColor  = new Color(0.40f, 0.75f, 0.30f);
    public Color GrownColor    = new Color(0.10f, 0.55f, 0.10f);
}
