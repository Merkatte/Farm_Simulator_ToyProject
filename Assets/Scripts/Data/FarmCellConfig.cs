// 농경지 칸의 작업 수치, 효율 곡선, 수확 설정, 상태별 색상 데이터.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmCellConfig", menuName = "Project_FAD/FarmCellConfig")]
public class FarmCellConfig : ScriptableObject {
    [Header("Work Requirements")]
    public float TillingMaxWork    = 30f;
    public float PlantingMaxWork   = 20f;
    public float SeededMaxWork     = 10f;
    public float GrowingMaxWork    = 60f;
    public float HarvestingMaxWork = 15f;

    [Header("Growth")]
    public float NaturalGrowthRate = 1f;

    [Header("Farmer Efficiency")]
    public AnimationCurve FarmerEfficiencyCurve = AnimationCurve.Linear(0f, 0f, 5f, 2.7f);
    public int MaxEffectiveFarmers = 5;

    [Header("Harvest")]
    public int MaxHarvestCount = 5;
    public int HarvestYield    = 1;

    [Header("State Colors")]
    public Color UntilledColor = new Color(0.55f, 0.40f, 0.22f);
    public Color TilledColor   = new Color(0.35f, 0.22f, 0.10f);
    public Color SeededColor   = new Color(0.70f, 0.65f, 0.30f);
    public Color GrowingColor  = new Color(0.40f, 0.75f, 0.30f);
    public Color GrownColor    = new Color(0.10f, 0.55f, 0.10f);

    private void OnValidate() => Normalize();

    public void Normalize() {
        TillingMaxWork    = Mathf.Max(0.01f, TillingMaxWork);
        PlantingMaxWork   = Mathf.Max(0.01f, PlantingMaxWork);
        SeededMaxWork     = Mathf.Max(0.01f, SeededMaxWork);
        GrowingMaxWork    = Mathf.Max(0.01f, GrowingMaxWork);
        HarvestingMaxWork = Mathf.Max(0.01f, HarvestingMaxWork);
        NaturalGrowthRate = Mathf.Max(0f, NaturalGrowthRate);
        MaxEffectiveFarmers = Mathf.Max(1, MaxEffectiveFarmers);
        MaxHarvestCount   = Mathf.Max(1, MaxHarvestCount);
        HarvestYield      = Mathf.Max(1, HarvestYield);
        if (FarmerEfficiencyCurve == null || FarmerEfficiencyCurve.length == 0)
            FarmerEfficiencyCurve = AnimationCurve.Linear(0f, 0f, 5f, 2.7f);
    }
}
