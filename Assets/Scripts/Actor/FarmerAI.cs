// AI 농부 에이전트의 생명주기 및 의사결정. 가중치 기반 확률로 다음 행동을 선택한다.
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public partial class FarmerAI : MonoBehaviour {
    [SerializeField] private FarmerConfig _config;

    private FarmerState _state = FarmerState.Idle;
    private IFarmCell   _targetCell;
    private IWarehouse  _warehouse;
    private float _loiterTimer;
    private int   _heldCrop;

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

    // 비활성화 시 등록된 셀에서 안전하게 해제한다.
    private void OnDisable() {
        if (IsWorkingState(_state)) SafeUnregisterTarget();
    }

    // 매 프레임 현재 상태에 맞는 처리를 수행한다.
    private void Update() {
        switch (_state) {
            case FarmerState.Idle:               HandleIdle();            break;
            case FarmerState.Loitering:          HandleLoitering();       break;
            case FarmerState.MovingToCell:       HandleMoveToCell();      break;
            case FarmerState.Tilling:
            case FarmerState.Planting:
            case FarmerState.HelpingGrow:
            case FarmerState.Harvesting:         HandleWorking();         break;
            case FarmerState.MovingToWarehouse:  HandleMoveToWarehouse(); break;
            case FarmerState.Depositing:         HandleDepositing();      break;
        }
    }

    // 가중치 기반 확률로 다음 행동을 선택하고 해당 상태로 전이한다.
    private void HandleIdle() {
        var cells      = FarmlandManager.Instance.AllCells;
        var candidates = BuildActionCandidates(cells);

        IFarmCell[] targets = new IFarmCell[candidates.Count];
        float[]     weights = new float[candidates.Count];
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

    // 지정 상태로 전이하고, Working 상태 진입·이탈 시 셀 등록/해제를 처리한다.
    private void TransitionTo(FarmerState next) {
        Debug.Log($"[FarmerAI] {_state} → {next}");
        if (IsWorkingState(_state)) SafeUnregisterTarget();
        _state       = next;
        _loiterTimer = next == FarmerState.Loitering
            ? UnityEngine.Random.Range(_config.LoiterMinSeconds, _config.LoiterMaxSeconds)
            : 0f;
        if (IsWorkingState(next) && _targetCell != null) _targetCell.RegisterFarmer();
    }

    // 파괴된 Unity 오브젝트에 UnregisterFarmer가 호출되지 않도록 유효성을 확인한 뒤 해제한다.
    private void SafeUnregisterTarget() {
        if (_targetCell == null) return;
        var obj = _targetCell as UnityEngine.Object;
        if (obj == null) return;  // C# null 또는 Unity가 파괴한 오브젝트 모두 차단
        _targetCell.UnregisterFarmer();
    }

    // 실행 가능한 행동 후보 리스트를 구성한다. 새 행동은 여기에만 추가한다.
    private List<ActionCandidate> BuildActionCandidates(IReadOnlyList<IFarmCell> cells) {
        return new List<ActionCandidate> {
            new ActionCandidate {
                Kind          = FarmerState.Loitering,
                FindTarget    = () => null,
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Loitering, _config.LoiterBaseWeight, hasTarget),
                Execute       = _ => TransitionTo(FarmerState.Loitering)
            },
            new ActionCandidate {
                Kind          = FarmerState.Tilling,
                FindTarget    = () => FindFirstWhere(cells, c => c.CanTill()),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Tilling, _config.TillBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.Planting,
                FindTarget    = () => FindFirstWhere(cells, c => c.CanPlant()),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Planting, _config.PlantBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.HelpingGrow,
                FindTarget    = () => FindFirstWhere(cells, c => c.CanHelpGrow()),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.HelpingGrow, _config.HelpGrowBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.Harvesting,
                FindTarget    = () => FindFirstWhere(cells, c => c.CanHarvest()),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Harvesting, _config.HarvestBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
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

    // Working 상태 여부를 반환한다. 셀 등록/해제 판단에 사용한다.
    private static bool IsWorkingState(FarmerState s) =>
        s == FarmerState.Tilling    || s == FarmerState.Planting ||
        s == FarmerState.HelpingGrow || s == FarmerState.Harvesting;

    // 행동 후보 하나를 표현한다. 새 행동은 BuildActionCandidates에 이 타입으로 추가한다.
    private class ActionCandidate {
        public FarmerState Kind;
        public Func<IFarmCell> FindTarget;
        public Func<bool, float> ComputeWeight;
        public Action<IFarmCell> Execute;
    }
}
