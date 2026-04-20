// 농경지 칸 하나의 상태 머신. 상태 전이, 시간 기반 자동 성장, 선택 하이라이트 시각 표현을 담당한다.
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState _state = FarmCellState.Untilled;
    private SeedConfig _plantedSeed;
    private float _growthTimer;
    private bool _isHighlighted;

    public FarmCellState State => _state;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        ApplyStateVisual();
    }

    // 성장 타이머를 진행시키고, 완료 시 다음 상태로 전이한다.
    private void Update() {
        if (_state != FarmCellState.Seeded && _state != FarmCellState.Growing) return;

        _growthTimer -= Time.deltaTime;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 경작 가능 여부를 반환한다.
    public bool CanTill()  => _config && _state == FarmCellState.Untilled;

    // 씨앗 심기 가능 여부를 반환한다.
    public bool CanPlant() => _config && _state == FarmCellState.Tilled;

    // 성장 가속 가능 여부를 반환한다.
    public bool CanBoost() => _config && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing);

    // 칸을 경작해 Tilled 상태로 전이한다.
    public void Till() {
        if (!CanTill()) return;
        SetState(FarmCellState.Tilled);
    }

    // 씨앗을 받아 심고 Seeded 상태로 전이한다. 성장 타이밍은 seed가 결정한다.
    public void PlantSeed(SeedConfig seed) {
        if (!CanPlant() || seed == null) return;
        _plantedSeed = seed;
        _growthTimer = seed.SeededToGrowingSeconds;
        SetState(FarmCellState.Seeded);
    }

    // 성장 타이머를 BoostReductionSeconds만큼 감소시키고, 완료 시 다음 상태로 전이한다.
    public void Boost() {
        if (!CanBoost()) return;
        _growthTimer -= _config.BoostReductionSeconds;
        while (_growthTimer <= 0f && (_state == FarmCellState.Seeded || _state == FarmCellState.Growing)) {
            AdvanceGrowth();
        }
    }

    // 하이라이트 플래그를 갱신하고 시각을 다시 적용한다.
    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    // 현재 성장 단계를 다음 단계로 전이한다. 초과된 시간을 다음 타이머에 이월한다.
    private void AdvanceGrowth() {
        float overflow = -_growthTimer;
        if (_state == FarmCellState.Seeded) {
            _growthTimer = Mathf.Max(0f, _plantedSeed.GrowingToGrownSeconds - overflow);
            SetState(FarmCellState.Growing);
        } else if (_state == FarmCellState.Growing) {
            _plantedSeed = null;
            SetState(FarmCellState.Grown);
        }
    }

    // 상태를 변경하고 시각을 갱신한다.
    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
    }

    // 현재 상태 색상과 하이라이트를 혼합해 SpriteRenderer에 적용한다.
    private void ApplyStateVisual() {
        if (_spriteRenderer == null || _config == null) return;
        Color baseColor = GetStateColor();
        _spriteRenderer.color = _isHighlighted ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
    }

    // 현재 상태에 대응하는 색상을 반환한다.
    private Color GetStateColor() {
        return _state switch {
            FarmCellState.Untilled => _config.UntilledColor,
            FarmCellState.Tilled   => _config.TilledColor,
            FarmCellState.Seeded   => _config.SeededColor,
            FarmCellState.Growing  => _config.GrowingColor,
            FarmCellState.Grown    => _config.GrownColor,
            _                      => Color.white
        };
    }
}
