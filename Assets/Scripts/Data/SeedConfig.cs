// 씨앗 종류의 정체성 데이터. 작물 구분, 표시 이름, 수확량 등 메타데이터를 보관한다.
using UnityEngine;

[CreateAssetMenu(fileName = "SeedConfig", menuName = "Project_FAD/SeedConfig")]
public class SeedConfig : ScriptableObject {
    [Header("Identity")]
    public string DisplayName  = "Unknown Crop";
    public int    HarvestYield = 1;
    public Sprite Icon;
}
