// AI 농부의 이동, 작업 기여속도, 행동 가중치, 확률 수정자 설정을 정의한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmerConfig", menuName = "Project_FAD/FarmerConfig")]
public class FarmerConfig : ScriptableObject {
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float InteractionRange = 0.1f;

    [Header("Work")]
    public float WorkContributionPerSecond = 1f;

    [Header("Action Base Weights")]
    public float LoiterBaseWeight    = 1f;
    public float TillBaseWeight      = 1f;
    public float PlantBaseWeight     = 1f;
    public float HelpGrowBaseWeight  = 1f;
    public float HarvestBaseWeight   = 2f;

    [Header("Loitering")]
    public float LoiterMinSeconds = 1f;
    public float LoiterMaxSeconds = 3f;

    [Header("Recency Penalty")]
    public float RecencyPenaltyFactor = 0.3f;
    public int   RecencyHistorySize   = 3;

    [Header("Context Multipliers")]
    public float AvailableTargetMultiplier = 2f;

    // Inspector 편집 시 즉시 최솟값을 보장한다.
    private void OnValidate() => Normalize();

    // 런타임에서도 호출 가능한 수치 보정 메서드. FarmerAI.Awake()에서 호출된다.
    public void Normalize() {
        MoveSpeed                 = Mathf.Max(0.01f, MoveSpeed);
        InteractionRange          = Mathf.Max(0.01f, InteractionRange);
        WorkContributionPerSecond = Mathf.Max(0.01f, WorkContributionPerSecond);
        LoiterMinSeconds          = Mathf.Max(0.01f, LoiterMinSeconds);
        LoiterMaxSeconds          = Mathf.Max(LoiterMinSeconds, LoiterMaxSeconds);
        RecencyHistorySize        = Mathf.Max(1, RecencyHistorySize);
        RecencyPenaltyFactor      = Mathf.Clamp01(RecencyPenaltyFactor);
        AvailableTargetMultiplier = Mathf.Max(1f, AvailableTargetMultiplier);
    }
}
