// 농경지 칸과 상호작용하는 공용 계약. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 칸을 조작한다.
public interface IFarmCell {
    // 현재 상태를 반환한다.
    FarmCellState State { get; }

    // 현재 작업 진척량을 반환한다.
    float WorkProgress { get; }

    // 현재 상태의 최대 작업량을 반환한다.
    float MaxWork { get; }

    // 현재까지의 수확 횟수를 반환한다.
    int HarvestCount { get; }

    // 경작 가능 여부를 반환한다. (Untilled 상태일 때만 true)
    bool CanTill();

    // 씨앗 심기 가능 여부를 반환한다. (Tilled 상태 + 전용 씨앗 할당 시 true)
    bool CanPlant();

    // 성장 보조 가능 여부를 반환한다. (Growing 상태일 때만 true)
    bool CanHelpGrow();

    // 수확 가능 여부를 반환한다. (Grown 상태일 때만 true)
    bool CanHarvest();

    // 이 칸에 농부를 등록한다. 항상 성공한다.
    void RegisterFarmer();

    // 이 칸에서 농부를 해제한다.
    void UnregisterFarmer();

    // 원시 작업 기여량을 전달한다. 셀 내부에서 효율 곡선을 적용해 진척량에 반영한다.
    void ContributeWork(float amount);

    // 수확물을 수취한다. 최초 호출자만 true와 yield를 반환하며, 이후 호출은 false를 반환한다.
    bool TryConsumeHarvest(out int yield);

    // 선택 하이라이트 표시 여부를 설정한다.
    void SetHighlight(bool highlight);
}
