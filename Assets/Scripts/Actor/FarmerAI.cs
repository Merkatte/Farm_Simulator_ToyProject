// AI 농부 에이전트. 가중치 기반 확률로 행동을 선택하고 밭 탐색 → 경작/파종/부스트 → 수확 → 창고 반납 사이클을 자율 수행한다.
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FarmerAI : MonoBehaviour {
    [SerializeField] private FarmerConfig _config;

    private FarmerState _state = FarmerState.Idle;
    private IFarmCell _targetCell;
    private IWarehouse _warehouse;
    private float _workTimer;
    private float _boostTimer;
    private float _loiterTimer;
    private int _heldCrop;
    private System.Action _pendingWorkAction;

    private readonly Queue<FarmerState> _recentActions = new();

    // 컴포넌트를 초기화하고 _config 할당 및 수치 유효성을 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }
        _config.Normalize();
    }

    // 창고 및 농경지 관리자 참조를 캐시하고 유효성을 검증한다.
    private void Start() {
        _warehouse = WarehouseManager.Instance;
        if (_warehouse == null) {
            Debug.LogError("[FarmerAI] WarehouseManager를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        if (FarmlandManager.Instance == null) {
            Debug.LogError("[FarmerAI] FarmlandManager를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    // 매 프레임 현재 상태에 맞는 처리를 수행한다.
    private void Update() {
        switch (_state) {
            case FarmerState.Idle:               HandleIdle();             break;
            case FarmerState.Loitering:          HandleLoitering();        break;
            case FarmerState.MovingToCell:       HandleMoveToCell();       break;
            case FarmerState.Tilling:
            case FarmerState.Planting:
            case FarmerState.Harvesting:         HandleWork();             break;
            case FarmerState.Boosting:           HandleBoosting();         break;
            case FarmerState.MovingToWarehouse:  HandleMoveToWarehouse();  break;
            case FarmerState.Depositing:         HandleDepositing();       break;
        }
    }

    // 가중치 기반 확률로 다음 행동을 선택하고 해당 상태로 전이한다.
    private void HandleIdle() {
        var cells = FarmlandManager.Instance.AllCells;
        var candidates = BuildActionCandidates(cells);

        IFarmCell[] targets = new IFarmCell[candidates.Count];
        float[] weights     = new float[candidates.Count];
        float totalWeight   = 0f;

        for (int i = 0; i < candidates.Count; i++) {
            targets[i]  = candidates[i].FindTarget();
            float w     = candidates[i].ComputeWeight(targets[i] != null || candidates[i].Kind == FarmerState.Loitering);
            weights[i]  = Mathf.Max(0f, w);
            totalWeight += weights[i];
        }

        int chosen = 0;
        if (totalWeight > 0f) {
            float roll = UnityEngine.Random.value * totalWeight;
            float acc  = 0f;
            for (int i = 0; i < weights.Length; i++) {
                acc += weights[i];
                if (roll <= acc) { chosen = i; break; }
            }
        }

        RecordAction(candidates[chosen].Kind);
        candidates[chosen].Execute(targets[chosen]);
    }

    // _loiterTimer가 0이 되면 Idle로 복귀한다.
    private void HandleLoitering() {
        _loiterTimer -= Time.deltaTime;
        if (_loiterTimer <= 0f) TransitionTo(FarmerState.Idle);
    }

    // 목표 밭 칸으로 이동한다. 도달 시 칸 상태에 맞는 작업으로 전이한다.
    private void HandleMoveToCell() {
        if (_targetCell == null) { TransitionTo(FarmerState.Idle); return; }
        if (!FarmlandManager.Instance.TryGetCellPosition(_targetCell, out var cellPos)) {
            TransitionTo(FarmerState.Idle);
            return;
        }
        if (!MoveTowards(cellPos)) return;

        if      (_targetCell.CanHarvest()) TransitionTo(FarmerState.Harvesting);
        else if (_targetCell.CanPlant())   TransitionTo(FarmerState.Planting);
        else if (_targetCell.CanTill())    TransitionTo(FarmerState.Tilling);
        else if (_targetCell.CanBoost())   TransitionTo(FarmerState.Boosting);
        else                               TransitionTo(FarmerState.Idle);
    }

    // _workTimer가 0이 되면 _pendingWorkAction을 실행한다.
    private void HandleWork() {
        _workTimer -= Time.deltaTime;
        if (_workTimer <= 0f) _pendingWorkAction?.Invoke();
    }

    // BoostIntervalSeconds 간격으로 Boost를 호출한다. Grown이 되면 Harvesting으로 전이한다.
    private void HandleBoosting() {
        if (!IsTargetValid())          { TransitionTo(FarmerState.Idle);       return; }
        if (_targetCell.CanHarvest())  { TransitionTo(FarmerState.Harvesting); return; }
        if (!_targetCell.CanBoost())   { TransitionTo(FarmerState.Idle);       return; }

        _boostTimer -= Time.deltaTime;
        if (_boostTimer <= 0f) {
            _targetCell.Boost();
            _boostTimer = _config.BoostIntervalSeconds;
        }
    }

    // 창고를 향해 이동한다. 도달 시 Depositing으로 전이한다.
    private void HandleMoveToWarehouse() {
        if (MoveTowards(_warehouse.Position)) TransitionTo(FarmerState.Depositing);
    }

    // 수확물을 창고에 보관하고 Idle로 전이해 다음 사이클을 시작한다.
    private void HandleDepositing() {
        _warehouse.Deposit(_heldCrop);
        _heldCrop = 0;
        TransitionTo(FarmerState.Idle);
    }

    // 칸을 경작하고 Idle로 전이한다.
    private void OnTillDone() {
        if (!IsTargetValid() || !_targetCell.CanTill()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.Till();
        TransitionTo(FarmerState.Idle);
    }

    // 씨앗을 심고 Idle로 전이한다.
    private void OnPlantDone() {
        if (!IsTargetValid() || !_targetCell.CanPlant()) { TransitionTo(FarmerState.Idle); return; }
        var seed = FarmlandManager.Instance.DefaultSeed;
        if (seed == null) {
            Debug.LogWarning("[FarmerAI] DefaultSeed가 null입니다. FarmlandManager Inspector를 확인하세요.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        _targetCell.PlantSeed(seed);
        TransitionTo(FarmerState.Idle);
    }

    // 수확하고 수확물을 들고 창고로 이동한다.
    private void OnHarvestDone() {
        if (!IsTargetValid() || !_targetCell.CanHarvest()) { TransitionTo(FarmerState.Idle); return; }
        _heldCrop = _targetCell.Harvest();
        if (_heldCrop <= 0) {
            Debug.LogWarning("[FarmerAI] Harvest() 반환값이 0입니다. Idle로 복귀합니다.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        TransitionTo(FarmerState.MovingToWarehouse);
    }

    // _targetCell이 null이 아니고 소멸되지 않았으며 FarmlandManager에 등록된 경우 true를 반환한다.
    private bool IsTargetValid() {
        if (_targetCell == null) return false;
        var obj = _targetCell as UnityEngine.Object;
        if (obj != null && !obj) return false;
        return FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _);
    }

    // 지정 위치로 MoveTowards 이동한다. InteractionRange 이내 도달 시 true를 반환한다.
    private bool MoveTowards(Vector3 target) {
        target.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, target, _config.MoveSpeed * Time.deltaTime);
        return Vector3.Distance(transform.position, target) <= _config.InteractionRange;
    }

    // 지정 상태의 첫 번째 칸을 반환한다.
    private IFarmCell FindByState(IReadOnlyList<IFarmCell> cells, FarmCellState state) {
        foreach (var cell in cells)
            if (cell != null && cell.State == state) return cell;
        return null;
    }

    // Seeded 또는 Growing 상태인 첫 번째 칸을 반환한다.
    private IFarmCell FindBoostable(IReadOnlyList<IFarmCell> cells) {
        foreach (var cell in cells)
            if (cell != null && (cell.State == FarmCellState.Seeded || cell.State == FarmCellState.Growing)) return cell;
        return null;
    }

    // 지정 상태로 전이하고 타이머와 대기 액션을 재설정한다.
    private void TransitionTo(FarmerState next) {
        Debug.Log($"[FarmerAI] {_state} → {next}");
        _state     = next;
        _boostTimer = 0f;
        _loiterTimer = next == FarmerState.Loitering
            ? UnityEngine.Random.Range(_config.LoiterMinSeconds, _config.LoiterMaxSeconds)
            : 0f;
        _workTimer = next switch {
            FarmerState.Tilling    => _config.TillingSeconds,
            FarmerState.Planting   => _config.PlantingSeconds,
            FarmerState.Harvesting => _config.HarvestingSeconds,
            _                      => 0f
        };
        _pendingWorkAction = next switch {
            FarmerState.Tilling    => OnTillDone,
            FarmerState.Planting   => OnPlantDone,
            FarmerState.Harvesting => OnHarvestDone,
            _                      => null
        };
    }

    // 실행 가능한 행동 후보 리스트를 구성한다. 새 행동은 여기에만 추가한다.
    private List<ActionCandidate> BuildActionCandidates(IReadOnlyList<IFarmCell> cells) {
        return new List<ActionCandidate> {
            new ActionCandidate {
                Kind        = FarmerState.Loitering,
                FindTarget  = () => null,
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Loitering, _config.LoiterBaseWeight, hasTarget),
                Execute     = _ => TransitionTo(FarmerState.Loitering)
            },
            new ActionCandidate {
                Kind        = FarmerState.Tilling,
                FindTarget  = () => FindByState(cells, FarmCellState.Untilled),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Tilling, _config.TillBaseWeight, hasTarget),
                Execute     = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind        = FarmerState.Planting,
                FindTarget  = () => FindByState(cells, FarmCellState.Tilled),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Planting, _config.PlantBaseWeight, hasTarget),
                Execute     = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind        = FarmerState.Boosting,
                FindTarget  = () => FindBoostable(cells),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Boosting, _config.BoostBaseWeight, hasTarget),
                Execute     = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind        = FarmerState.Harvesting,
                FindTarget  = () => FindByState(cells, FarmCellState.Grown),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Harvesting, _config.HarvestBaseWeight, hasTarget),
                Execute     = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
        };
    }

    // 기본값 × 최근행동패널티 × 환경수정자 × 컨텍스트수정자로 최종 가중치를 계산한다. 타겟이 없는 비-Loiter 행동은 0을 반환한다.
    private float ComputeWeight(FarmerState kind, float baseWeight, bool hasTarget) {
        if (kind != FarmerState.Loitering && !hasTarget) return 0f;
        float recency     = GetRecencyPenalty(kind);
        float environment = GetEnvironmentMultiplier(kind);
        float context     = hasTarget ? _config.AvailableTargetMultiplier : 1f;
        return baseWeight * recency * environment * context;
    }

    // 최근 수행한 행동일수록 낮은 배율을 반환한다.
    private float GetRecencyPenalty(FarmerState kind) {
        int count = 0;
        foreach (var past in _recentActions)
            if (past == kind) count++;
        return Mathf.Pow(_config.RecencyPenaltyFactor, count);
    }

    // 향후 Hunger/PlayNeed 도입 시 여기서 상태별 배율을 반환한다.
    private float GetEnvironmentMultiplier(FarmerState kind) => 1f;

    // 선택된 행동을 최근 이력 큐에 기록하고 초과분을 제거한다.
    private void RecordAction(FarmerState kind) {
        _recentActions.Enqueue(kind);
        while (_recentActions.Count > _config.RecencyHistorySize)
            _recentActions.Dequeue();
    }

    // 행동 후보 하나를 표현한다. 새 행동은 BuildActionCandidates에 이 타입으로 추가한다.
    private class ActionCandidate {
        public FarmerState Kind;
        public Func<IFarmCell> FindTarget;
        public Func<bool, float> ComputeWeight;
        public Action<IFarmCell> Execute;
    }
}
