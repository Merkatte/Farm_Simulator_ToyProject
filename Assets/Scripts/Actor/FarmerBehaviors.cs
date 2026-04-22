// FarmerAI의 상태별 행동 실행 로직. 이동, 작업 기여, 수확, 창고 반납 등 Handle 메서드를 담당한다.
using System.Collections.Generic;
using UnityEngine;

public partial class FarmerAI {
    // _loiterTimer가 0이 되면 Idle로 복귀한다.
    private void HandleLoitering() {
        _loiterTimer -= Time.deltaTime;
        if (_loiterTimer <= 0f) TransitionTo(FarmerState.Idle);
    }

    // 목표 밭 칸으로 이동한다. 도달 시 칸 상태에 맞는 작업으로 전이한다.
    private void HandleMoveToCell() {
        if (!IsTargetValid()) { TransitionTo(FarmerState.Idle); return; }
        if (!FarmlandManager.Instance.TryGetCellPosition(_targetCell, out var cellPos)) {
            TransitionTo(FarmerState.Idle);
            return;
        }
        if (!MoveTowards(cellPos)) return;

        if      (_targetCell.CanHarvest())   TransitionTo(FarmerState.Harvesting);
        else if (_targetCell.CanPlant())     TransitionTo(FarmerState.Planting);
        else if (_targetCell.CanTill())      TransitionTo(FarmerState.Tilling);
        else if (_targetCell.CanHelpGrow())  TransitionTo(FarmerState.HelpingGrow);
        else                                 TransitionTo(FarmerState.Idle);
    }

    // 대상 셀에 작업 기여량을 전달하고, 작업 완료(상태 변화) 감지 시 다음 행동으로 전이한다.
    private void HandleWorking() {
        if (!IsTargetValid()) { TransitionTo(FarmerState.Idle); return; }

        FarmCellState expectedState = _state switch {
            FarmerState.Tilling      => FarmCellState.Untilled,
            FarmerState.Planting     => FarmCellState.Tilled,
            FarmerState.HelpingGrow  => FarmCellState.Growing,
            FarmerState.Harvesting   => FarmCellState.Grown,
            _                        => FarmCellState.Untilled
        };

        if (_targetCell.State != expectedState) {
            if (_state == FarmerState.Harvesting && _targetCell.TryConsumeHarvest(out int yield)) {
                _heldCrop = yield;
                TransitionTo(FarmerState.MovingToWarehouse);
                return;
            }
            TransitionTo(FarmerState.Idle);
            return;
        }

        _targetCell.ContributeWork(_config.WorkContributionPerSecond * Time.deltaTime);
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

    // _targetCell이 null이 아니고 소멸되지 않았으며 FarmlandManager에 등록된 경우 true를 반환한다.
    private bool IsTargetValid() {
        if (_targetCell == null) return false;
        var obj = _targetCell as UnityEngine.Object;
        if (obj == null) return false;  // C# null 또는 Unity가 파괴한 오브젝트 모두 차단
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

    // 조건 술어를 만족하는 첫 번째 칸을 반환한다.
    private static IFarmCell FindFirstWhere(IReadOnlyList<IFarmCell> cells, System.Func<IFarmCell, bool> predicate) {
        foreach (var cell in cells)
            if (cell != null && predicate(cell)) return cell;
        return null;
    }
}
