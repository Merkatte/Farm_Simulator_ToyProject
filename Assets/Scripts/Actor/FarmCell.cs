// 농경지 칸 하나의 상태 머신. WorkProgress 기반 자동 전이, 다중 농부 협력, 다중 수확을 처리한다.
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmCell : MonoBehaviour, IFarmCell {
    [SerializeField] private FarmCellConfig _config;
    [SerializeField] private SeedConfig _dedicatedSeed;

    private SpriteRenderer _spriteRenderer;
    private FarmCellState  _state = FarmCellState.Untilled;
    private float _workProgress;
    private int   _harvestCount;
    private int   _lastHarvestYield;
    private bool  _harvestClaimed;
    private int   _activeFarmerCount;
    private bool  _isHighlighted;

    public FarmCellState State        => _state;
    public float         WorkProgress => _workProgress;
    public float         MaxWork      => GetMaxWorkForState(_state);
    public int           HarvestCount => _harvestCount;

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_config == null) {
            Debug.LogError($"[FarmCell] {name}: FarmCellConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        _config.Normalize();
        ApplyStateVisual();
    }

    // 매 프레임 자연 성장률을 누적하고, MaxWork 도달 시 자동 전이한다.
    private void Update() {
        if (_state == FarmCellState.Seeded || _state == FarmCellState.Growing) {
            _workProgress += _config.NaturalGrowthRate * Time.deltaTime;
            if (_workProgress >= MaxWork) AutoTransition();
        }
    }

    // 경작 가능 여부를 반환한다.
    public bool CanTill()     => _config != null && _state == FarmCellState.Untilled;

    // 씨앗 심기 가능 여부를 반환한다.
    public bool CanPlant()    => _config != null && _state == FarmCellState.Tilled && _dedicatedSeed != null;

    // 성장 보조 가능 여부를 반환한다.
    public bool CanHelpGrow() => _config != null && _state == FarmCellState.Growing;

    // 수확 가능 여부를 반환한다.
    public bool CanHarvest()  => _config != null && _state == FarmCellState.Grown;

    // 농부를 이 칸에 등록한다.
    public void RegisterFarmer() {
        _activeFarmerCount++;
        Debug.Log($"[FarmCell] {name}: 농부 등록 (활성: {_activeFarmerCount})");
    }

    // 농부를 이 칸에서 해제한다.
    public void UnregisterFarmer() {
        _activeFarmerCount = Mathf.Max(0, _activeFarmerCount - 1);
        Debug.Log($"[FarmCell] {name}: 농부 해제 (활성: {_activeFarmerCount})");
    }

    // 원시 작업 기여량을 받아 효율 곡선을 적용 후 진척량에 반영한다. Seeded 상태이거나 농부가 없으면 무시한다.
    public void ContributeWork(float amount) {
        if (_activeFarmerCount <= 0) return;
        if (_state == FarmCellState.Seeded) return;
        float effective  = Mathf.Min(_activeFarmerCount, _config.MaxEffectiveFarmers);
        float efficiency = _config.FarmerEfficiencyCurve.Evaluate(effective);
        _workProgress += amount * efficiency / _activeFarmerCount;
        if (_workProgress >= MaxWork) AutoTransition();
    }

    // 수확물을 수취한다. 최초 호출자만 true와 yield를 반환한다.
    public bool TryConsumeHarvest(out int yield) {
        if (_harvestClaimed || _lastHarvestYield <= 0) { yield = 0; return false; }
        _harvestClaimed = true;
        yield = _lastHarvestYield;
        return true;
    }

    // 하이라이트 플래그를 갱신하고 시각을 다시 적용한다.
    public void SetHighlight(bool highlight) {
        _isHighlighted = highlight;
        ApplyStateVisual();
    }

    // WorkProgress가 MaxWork에 도달하면 다음 상태로 자동 전이한다.
    private void AutoTransition() {
        _workProgress = 0f;
        switch (_state) {
            case FarmCellState.Untilled:
                SetState(FarmCellState.Tilled);
                break;
            case FarmCellState.Tilled:
                SetState(FarmCellState.Seeded);
                break;
            case FarmCellState.Seeded:
                SetState(FarmCellState.Growing);
                break;
            case FarmCellState.Growing:
                SetState(FarmCellState.Grown);
                break;
            case FarmCellState.Grown:
                _lastHarvestYield = _config.HarvestYield;
                _harvestClaimed   = false;
                _harvestCount++;
                Debug.Log($"[FarmCell] {name}: 수확 {_harvestCount}/{_config.MaxHarvestCount}회");
                if (_harvestCount >= _config.MaxHarvestCount) {
                    _harvestCount = 0;
                    SetState(FarmCellState.Untilled);
                } else {
                    SetState(FarmCellState.Growing);
                }
                break;
        }
    }

    // 현재 상태에 대응하는 최대 작업량을 반환한다.
    private float GetMaxWorkForState(FarmCellState state) {
        if (_config == null) return 1f;
        return state switch {
            FarmCellState.Untilled => _config.TillingMaxWork,
            FarmCellState.Tilled   => _config.PlantingMaxWork,
            FarmCellState.Seeded   => _config.SeededMaxWork,
            FarmCellState.Growing  => _config.GrowingMaxWork,
            FarmCellState.Grown    => _config.HarvestingMaxWork,
            _                      => 1f
        };
    }

    // 상태를 변경하고 시각을 갱신한다.
    private void SetState(FarmCellState newState) {
        _state = newState;
        ApplyStateVisual();
        Debug.Log($"[FarmCell] {name}: → {newState}");
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
