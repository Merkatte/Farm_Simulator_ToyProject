// 농경지 칸과 상호작용하는 공용 계약. AI 에이전트와 플레이어 모두 이 인터페이스를 통해 칸을 조작한다.
public interface IFarmCell
{
    // 현재 상태를 반환한다.
    FarmCellState State { get; }

    // 경작 가능 여부를 반환한다. (Untilled 상태일 때만 true)
    bool CanTill();

    // 씨앗 심기 가능 여부를 반환한다. (Tilled 상태일 때만 true)
    bool CanPlant();

    // 성장 가속 가능 여부를 반환한다. (Seeded 또는 Growing 상태일 때만 true)
    bool CanBoost();

    // 칸을 경작해 Tilled 상태로 전이한다.
    void Till();

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    void PlantSeed(SeedConfig seed);

    // 성장 타이머를 BoostReductionSeconds만큼 감소시킨다.
    void Boost();

    // 선택 하이라이트 표시 여부를 설정한다.
    void SetHighlight(bool highlight);
}
