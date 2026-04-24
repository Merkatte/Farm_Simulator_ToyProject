# Codex 리뷰 로그 — 2026-04-22

Claude가 Codex에게 보낸 프롬프트와 Codex의 응답을 회차별로 기록합니다.

## 1회차 리뷰 — 2026-04-22 00:43:28 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
FarmerAI 행동 결정 시스템을 Weighted Utility AI 패턴으로 리팩토링. ActionCandidate 내부 클래스, 최근행동 패널티 큐, 환경/컨텍스트 수정자, Loitering(멍때리기) 상태 도입. 확장성: 새 행동은 BuildActionCandidates에 한 줄 추가로 가능.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
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
        if (_targetCell.CanHarvest()) { TransitionTo(FarmerState.Harvesting); return; }
        if (!_targetCell.CanBoost())  { TransitionTo(FarmerState.Idle);       return; }

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
        if (!_targetCell.CanTill()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.Till();
        TransitionTo(FarmerState.Idle);
    }

    // 씨앗을 심고 Idle로 전이한다.
    private void OnPlantDone() {
        if (!_targetCell.CanPlant()) { TransitionTo(FarmerState.Idle); return; }
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
        if (!_targetCell.CanHarvest()) { TransitionTo(FarmerState.Idle); return; }
        _heldCrop = _targetCell.Harvest();
        if (_heldCrop <= 0) {
            Debug.LogWarning("[FarmerAI] Harvest() 반환값이 0입니다. Idle로 복귀합니다.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        TransitionTo(FarmerState.MovingToWarehouse);
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

    // 기본값 × 최근행동패널티 × 환경수정자 × 컨텍스트수정자로 최종 가중치를 계산한다.
    private float ComputeWeight(FarmerState kind, float baseWeight, bool hasTarget) {
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

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
// AI 농부의 이동, 작업 지속시간, 행동 가중치, 확률 수정자 설정을 정의한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmerConfig", menuName = "Project_FAD/FarmerConfig")]
public class FarmerConfig : ScriptableObject {
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float InteractionRange = 0.1f;

    [Header("Work Duration")]
    public float TillingSeconds = 1f;
    public float PlantingSeconds = 0.8f;
    public float HarvestingSeconds = 1.2f;
    public float BoostIntervalSeconds = 0.5f;

    [Header("Action Base Weights")]
    public float LoiterBaseWeight  = 1f;
    public float TillBaseWeight    = 1f;
    public float PlantBaseWeight   = 1f;
    public float BoostBaseWeight   = 1f;
    public float HarvestBaseWeight = 2f;

    [Header("Loitering")]
    public float LoiterMinSeconds = 1f;
    public float LoiterMaxSeconds = 3f;

    [Header("Recency Penalty")]
    public float RecencyPenaltyFactor = 0.3f;
    public int   RecencyHistorySize   = 3;

    [Header("Context Multipliers")]
    public float AvailableTargetMultiplier = 2f;
}

```

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    Boosting,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

---

## 2회차 리뷰 — 2026-04-22 00:45:44 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 2번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
FarmerAI 행동 결정 시스템을 Weighted Utility AI 패턴으로 리팩토링. ActionCandidate 내부 클래스, 최근행동 패널티 큐, 환경/컨텍스트 수정자, Loitering(멍때리기) 상태 도입. 확장성: 새 행동은 BuildActionCandidates에 한 줄 추가로 가능.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
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
        if (_targetCell == null)       { TransitionTo(FarmerState.Idle);       return; }
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
        if (_targetCell == null || !_targetCell.CanTill()) { TransitionTo(FarmerState.Idle); return; }
        _targetCell.Till();
        TransitionTo(FarmerState.Idle);
    }

    // 씨앗을 심고 Idle로 전이한다.
    private void OnPlantDone() {
        if (_targetCell == null || !_targetCell.CanPlant()) { TransitionTo(FarmerState.Idle); return; }
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
        if (_targetCell == null || !_targetCell.CanHarvest()) { TransitionTo(FarmerState.Idle); return; }
        _heldCrop = _targetCell.Harvest();
        if (_heldCrop <= 0) {
            Debug.LogWarning("[FarmerAI] Harvest() 반환값이 0입니다. Idle로 복귀합니다.", this);
            TransitionTo(FarmerState.Idle);
            return;
        }
        TransitionTo(FarmerState.MovingToWarehouse);
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

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
// AI 농부의 이동, 작업 지속시간, 행동 가중치, 확률 수정자 설정을 정의한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmerConfig", menuName = "Project_FAD/FarmerConfig")]
public class FarmerConfig : ScriptableObject {
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float InteractionRange = 0.1f;

    [Header("Work Duration")]
    public float TillingSeconds = 1f;
    public float PlantingSeconds = 0.8f;
    public float HarvestingSeconds = 1.2f;
    public float BoostIntervalSeconds = 0.5f;

    [Header("Action Base Weights")]
    public float LoiterBaseWeight  = 1f;
    public float TillBaseWeight    = 1f;
    public float PlantBaseWeight   = 1f;
    public float BoostBaseWeight   = 1f;
    public float HarvestBaseWeight = 2f;

    [Header("Loitering")]
    public float LoiterMinSeconds = 1f;
    public float LoiterMaxSeconds = 3f;

    [Header("Recency Penalty")]
    public float RecencyPenaltyFactor = 0.3f;
    public int   RecencyHistorySize   = 3;

    [Header("Context Multipliers")]
    public float AvailableTargetMultiplier = 2f;

    // Inspector에서 값이 변경될 때 최솟값을 보장한다.
    private void OnValidate() {
        MoveSpeed               = Mathf.Max(0.01f, MoveSpeed);
        InteractionRange        = Mathf.Max(0.01f, InteractionRange);
        TillingSeconds          = Mathf.Max(0.01f, TillingSeconds);
        PlantingSeconds         = Mathf.Max(0.01f, PlantingSeconds);
        HarvestingSeconds       = Mathf.Max(0.01f, HarvestingSeconds);
        BoostIntervalSeconds    = Mathf.Max(0.01f, BoostIntervalSeconds);
        LoiterMinSeconds        = Mathf.Max(0.01f, LoiterMinSeconds);
        LoiterMaxSeconds        = Mathf.Max(LoiterMinSeconds, LoiterMaxSeconds);
        RecencyHistorySize      = Mathf.Max(1, RecencyHistorySize);
        RecencyPenaltyFactor    = Mathf.Clamp01(RecencyPenaltyFactor);
        AvailableTargetMultiplier = Mathf.Max(1f, AvailableTargetMultiplier);
    }
}

```

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    Boosting,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

```

### Codex 응답

**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

---

## 3회차 리뷰 — 2026-04-22 00:48:22 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 3번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
FarmerAI 행동 결정 시스템을 Weighted Utility AI 패턴으로 리팩토링. ActionCandidate 내부 클래스, 최근행동 패널티 큐, 환경/컨텍스트 수정자, Loitering(멍때리기) 상태 도입. 확장성: 새 행동은 BuildActionCandidates에 한 줄 추가로 가능.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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

    // 컴포넌트를 초기화하고 _config 할당 여부를 검증한다.
    private void Awake() {
        if (_config == null) {
            Debug.LogError("[FarmerAI] FarmerConfig이 할당되지 않았습니다.", this);
            enabled = false;
        }
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

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
// AI 농부의 이동, 작업 지속시간, 행동 가중치, 확률 수정자 설정을 정의한다.
using UnityEngine;

[CreateAssetMenu(fileName = "FarmerConfig", menuName = "Project_FAD/FarmerConfig")]
public class FarmerConfig : ScriptableObject {
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float InteractionRange = 0.1f;

    [Header("Work Duration")]
    public float TillingSeconds = 1f;
    public float PlantingSeconds = 0.8f;
    public float HarvestingSeconds = 1.2f;
    public float BoostIntervalSeconds = 0.5f;

    [Header("Action Base Weights")]
    public float LoiterBaseWeight  = 1f;
    public float TillBaseWeight    = 1f;
    public float PlantBaseWeight   = 1f;
    public float BoostBaseWeight   = 1f;
    public float HarvestBaseWeight = 2f;

    [Header("Loitering")]
    public float LoiterMinSeconds = 1f;
    public float LoiterMaxSeconds = 3f;

    [Header("Recency Penalty")]
    public float RecencyPenaltyFactor = 0.3f;
    public int   RecencyHistorySize   = 3;

    [Header("Context Multipliers")]
    public float AvailableTargetMultiplier = 2f;

    // Inspector에서 값이 변경될 때 최솟값을 보장한다.
    private void OnValidate() {
        MoveSpeed               = Mathf.Max(0.01f, MoveSpeed);
        InteractionRange        = Mathf.Max(0.01f, InteractionRange);
        TillingSeconds          = Mathf.Max(0.01f, TillingSeconds);
        PlantingSeconds         = Mathf.Max(0.01f, PlantingSeconds);
        HarvestingSeconds       = Mathf.Max(0.01f, HarvestingSeconds);
        BoostIntervalSeconds    = Mathf.Max(0.01f, BoostIntervalSeconds);
        LoiterMinSeconds        = Mathf.Max(0.01f, LoiterMinSeconds);
        LoiterMaxSeconds        = Mathf.Max(LoiterMinSeconds, LoiterMaxSeconds);
        RecencyHistorySize      = Mathf.Max(1, RecencyHistorySize);
        RecencyPenaltyFactor    = Mathf.Clamp01(RecencyPenaltyFactor);
        AvailableTargetMultiplier = Mathf.Max(1f, AvailableTargetMultiplier);
    }
}

```

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    Boosting,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

---

## 1회차 리뷰 — 2026-04-22 16:27:57 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
FarmerAI.cs를 partial class로 분리. 행동 실행 로직(Handle*, On*Done, IsTargetValid, MoveTowards, FindBy*)을 FarmerBehaviors.cs로 이동. 생명주기+AI의사결정은 FarmerAI.cs에 유지. 동작 변경 없는 순수 파일 분할 리팩터링.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 3회차 리뷰 (2026-04-22 00:48:22)
**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
// AI 농부 에이전트의 생명주기 및 의사결정. 가중치 기반 확률로 다음 행동을 선택한다.
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public partial class FarmerAI : MonoBehaviour {
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

```

### 파일: Assets/Scripts/Actor/FarmerBehaviors.cs
```
// FarmerAI의 상태별 행동 실행 로직. 이동, 작업, 수확, 창고 반납 등 Handle 메서드를 담당한다.
using System.Collections.Generic;
using UnityEngine;

public partial class FarmerAI : MonoBehaviour {
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
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:5)에서 분리된 partial 파일도 `public partial class FarmerAI : MonoBehaviour`로 선언되어 있습니다냥. Unity는 `MonoBehaviour` 스크립트의 파일명과 클래스명 매칭에 민감하므로 `FarmerBehaviors.cs`가 `FarmerAI`라는 public `MonoBehaviour`를 담고 있으면 스크립트 임포트 경고나 “클래스 이름과 파일 이름 불일치” 문제가 날 수 있습니다냥. 보조 partial 파일은 `public partial class FarmerAI { ... }`처럼 base class를 생략하고, 실제 `MonoBehaviour` 상속 표기는 `FarmerAI.cs` 한 곳에만 두는 편이 안전합니다냥.

2. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:13)의 `HandleMoveToCell()`은 `_targetCell == null`과 `TryGetCellPosition()`만 확인하고, [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:92)의 `IsTargetValid()`를 사용하지 않습니다냥. `_targetCell`이 Unity 객체로 파괴된 상태라면 인터페이스 참조의 null 비교는 통과할 수 있고, `FarmlandManager.TryGetCellPosition()` 내부에서 파괴된 `_cells[i].transform` 접근이 `MissingReferenceException`으로 이어질 수 있습니다냥. 이동 상태도 작업 완료 콜백과 동일하게 `IsTargetValid()`로 먼저 검증한 뒤 위치를 가져오도록 통일하는 것이 좋습니다냥.

**반영 확인냥**

이전 리뷰에서 남았던 `FarmerConfig` 런타임 값 보정은 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:38)의 `Normalize()`와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:28)의 호출로 반영된 것으로 보입니다냥.

행동 실행 로직을 `FarmerBehaviors.cs`로 이동하고 생명주기와 의사결정을 `FarmerAI.cs`에 남긴 큰 분리 방향은 요구사항과 맞습니다냥.

Unity 컴파일은 직접 실행하지 못했고, git 상태 확인은 저장소 소유권 안전 설정 문제로 확인하지 못했습니다냥.

---

## 2회차 리뷰 — 2026-04-22 16:30:57 (✅ 승인 (REVIEW_DONE))

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 2번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
FarmerAI.cs를 partial class로 분리. 행동 실행 로직(Handle*, On*Done, IsTargetValid, MoveTowards, FindBy*)을 FarmerBehaviors.cs로 이동. 생명주기+AI의사결정은 FarmerAI.cs에 유지. 동작 변경 없는 순수 파일 분할 리팩터링. 1회차 수정: FarmerBehaviors.cs에서 MonoBehaviour 중복 선언 제거, HandleMoveToCell에서 IsTargetValid() 활용.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 3회차 리뷰 (2026-04-22 00:48:22)
**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

### 1회차 리뷰 (2026-04-22 16:27:57)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:5)에서 분리된 partial 파일도 `public partial class FarmerAI : MonoBehaviour`로 선언되어 있습니다냥. Unity는 `MonoBehaviour` 스크립트의 파일명과 클래스명 매칭에 민감하므로 `FarmerBehaviors.cs`가 `FarmerAI`라는 public `MonoBehaviour`를 담고 있으면 스크립트 임포트 경고나 “클래스 이름과 파일 이름 불일치” 문제가 날 수 있습니다냥. 보조 partial 파일은 `public partial class FarmerAI { ... }`처럼 base class를 생략하고, 실제 `MonoBehaviour` 상속 표기는 `FarmerAI.cs` 한 곳에만 두는 편이 안전합니다냥.

2. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:13)의 `HandleMoveToCell()`은 `_targetCell == null`과 `TryGetCellPosition()`만 확인하고, [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:92)의 `IsTargetValid()`를 사용하지 않습니다냥. `_targetCell`이 Unity 객체로 파괴된 상태라면 인터페이스 참조의 null 비교는 통과할 수 있고, `FarmlandManager.TryGetCellPosition()` 내부에서 파괴된 `_cells[i].transform` 접근이 `MissingReferenceException`으로 이어질 수 있습니다냥. 이동 상태도 작업 완료 콜백과 동일하게 `IsTargetValid()`로 먼저 검증한 뒤 위치를 가져오도록 통일하는 것이 좋습니다냥.

**반영 확인냥**

이전 리뷰에서 남았던 `FarmerConfig` 런타임 값 보정은 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:38)의 `Normalize()`와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:28)의 호출로 반영된 것으로 보입니다냥.

행동 실행 로직을 `FarmerBehaviors.cs`로 이동하고 생명주기와 의사결정을 `FarmerAI.cs`에 남긴 큰 분리 방향은 요구사항과 맞습니다냥.

Unity 컴파일은 직접 실행하지 못했고, git 상태 확인은 저장소 소유권 안전 설정 문제로 확인하지 못했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
// AI 농부 에이전트의 생명주기 및 의사결정. 가중치 기반 확률로 다음 행동을 선택한다.
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public partial class FarmerAI : MonoBehaviour {
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

```

### 파일: Assets/Scripts/Actor/FarmerBehaviors.cs
```
// FarmerAI의 상태별 행동 실행 로직. 이동, 작업, 수확, 창고 반납 등 Handle 메서드를 담당한다.
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
}

```

```

### Codex 응답

**Key Findings냥**

추가 개선점은 없습니다냥.

1회차 지적사항이었던 `FarmerBehaviors.cs`의 `MonoBehaviour` 중복 선언은 제거되어 `public partial class FarmerAI` 형태로 정리됐습니다냥.

`HandleMoveToCell()`도 `IsTargetValid()`를 먼저 사용하도록 수정되어, 작업 완료 콜백과 부스트 처리 경로의 타겟 유효성 검증 방식과 일관성이 맞습니다냥.

요구사항대로 생명주기와 의사결정 로직은 `FarmerAI.cs`에 남고, 행동 실행 로직은 `FarmerBehaviors.cs`로 분리되어 순수 파일 분할 리팩터링 범위에 부합합니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

---

## 1회차 리뷰 — 2026-04-22 17:27:16 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 1번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
농경지 시스템 전면 재설계. FarmCell이 WorkProgress 소유, 농부들이 ContributeWork로 기여. 효율 체감 곡선(AnimationCurve), Growing만 자연 성장, Seeded는 자연 지연만, 전용 밭(_dedicatedSeed), 다중 수확(5회), TryConsumeHarvest 토큰 패턴. FarmerAI는 Register/Unregister 기반으로 재설계, HandleWorking 통합, Boosting→HelpingGrow 리네임.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 3회차 리뷰 (2026-04-22 00:48:22)
**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

### 1회차 리뷰 (2026-04-22 16:27:57)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:5)에서 분리된 partial 파일도 `public partial class FarmerAI : MonoBehaviour`로 선언되어 있습니다냥. Unity는 `MonoBehaviour` 스크립트의 파일명과 클래스명 매칭에 민감하므로 `FarmerBehaviors.cs`가 `FarmerAI`라는 public `MonoBehaviour`를 담고 있으면 스크립트 임포트 경고나 “클래스 이름과 파일 이름 불일치” 문제가 날 수 있습니다냥. 보조 partial 파일은 `public partial class FarmerAI { ... }`처럼 base class를 생략하고, 실제 `MonoBehaviour` 상속 표기는 `FarmerAI.cs` 한 곳에만 두는 편이 안전합니다냥.

2. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:13)의 `HandleMoveToCell()`은 `_targetCell == null`과 `TryGetCellPosition()`만 확인하고, [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:92)의 `IsTargetValid()`를 사용하지 않습니다냥. `_targetCell`이 Unity 객체로 파괴된 상태라면 인터페이스 참조의 null 비교는 통과할 수 있고, `FarmlandManager.TryGetCellPosition()` 내부에서 파괴된 `_cells[i].transform` 접근이 `MissingReferenceException`으로 이어질 수 있습니다냥. 이동 상태도 작업 완료 콜백과 동일하게 `IsTargetValid()`로 먼저 검증한 뒤 위치를 가져오도록 통일하는 것이 좋습니다냥.

**반영 확인냥**

이전 리뷰에서 남았던 `FarmerConfig` 런타임 값 보정은 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:38)의 `Normalize()`와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:28)의 호출로 반영된 것으로 보입니다냥.

행동 실행 로직을 `FarmerBehaviors.cs`로 이동하고 생명주기와 의사결정을 `FarmerAI.cs`에 남긴 큰 분리 방향은 요구사항과 맞습니다냥.

Unity 컴파일은 직접 실행하지 못했고, git 상태 확인은 저장소 소유권 안전 설정 문제로 확인하지 못했습니다냥.

### 2회차 리뷰 (2026-04-22 16:30:57)
**Key Findings냥**

추가 개선점은 없습니다냥.

1회차 지적사항이었던 `FarmerBehaviors.cs`의 `MonoBehaviour` 중복 선언은 제거되어 `public partial class FarmerAI` 형태로 정리됐습니다냥.

`HandleMoveToCell()`도 `IsTargetValid()`를 먼저 사용하도록 수정되어, 작업 완료 콜백과 부스트 처리 경로의 타겟 유효성 검증 방식과 일관성이 맞습니다냥.

요구사항대로 생명주기와 의사결정 로직은 `FarmerAI.cs`에 남고, 행동 실행 로직은 `FarmerBehaviors.cs`로 분리되어 순수 파일 분할 리팩터링 범위에 부합합니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

## 리뷰 대상 코드

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    HelpingGrow,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
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

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
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

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
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
}

```

### 파일: Assets/Scripts/Data/SeedConfig.cs
```
// 씨앗 종류의 정체성 데이터. 작물 구분, 표시 이름, 수확량 등 메타데이터를 보관한다.
using UnityEngine;

[CreateAssetMenu(fileName = "SeedConfig", menuName = "Project_FAD/SeedConfig")]
public class SeedConfig : ScriptableObject {
    [Header("Identity")]
    public string DisplayName  = "Unknown Crop";
    public int    HarvestYield = 1;
    public Sprite Icon;
}

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
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

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (_cells.Length != 4)
            Debug.LogError($"[FarmlandManager] _cells 배열은 정확히 4개여야 합니다. 현재: {_cells.Length}", this);

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] == null)
                Debug.LogError($"[FarmlandManager] _cells[{i}]가 null입니다. Inspector에서 연결해주세요.", this);
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 position에 담아 반환한다. 존재하지 않으면 false.
    public bool TryGetCellPosition(IFarmCell cell, out Vector3 position) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) {
                position = _cells[i].transform.position;
                return true;
            }
        }
        position = Vector3.zero;
        return false;
    }
}

```

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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
        if (IsWorkingState(_state) && _targetCell != null)
            _targetCell.UnregisterFarmer();
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
        if (IsWorkingState(_state) && _targetCell != null) _targetCell.UnregisterFarmer();
        _state       = next;
        _loiterTimer = next == FarmerState.Loitering
            ? UnityEngine.Random.Range(_config.LoiterMinSeconds, _config.LoiterMaxSeconds)
            : 0f;
        if (IsWorkingState(next) && _targetCell != null) _targetCell.RegisterFarmer();
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
                FindTarget    = () => FindByState(cells, FarmCellState.Untilled),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Tilling, _config.TillBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.Planting,
                FindTarget    = () => FindByState(cells, FarmCellState.Tilled),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.Planting, _config.PlantBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.HelpingGrow,
                FindTarget    = () => FindByState(cells, FarmCellState.Growing),
                ComputeWeight = hasTarget => ComputeWeight(FarmerState.HelpingGrow, _config.HelpGrowBaseWeight, hasTarget),
                Execute       = cell => { _targetCell = cell; TransitionTo(FarmerState.MovingToCell); }
            },
            new ActionCandidate {
                Kind          = FarmerState.Harvesting,
                FindTarget    = () => FindByState(cells, FarmCellState.Grown),
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

```

### 파일: Assets/Scripts/Actor/FarmerBehaviors.cs
```
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
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:81)의 `FindByState()`가 `State`만 보고 타겟을 고르기 때문에, 실제 `CanPlant()`나 `CanTill()`이 false인 셀도 실행 가능한 후보로 계산됩니다냥. 특히 `_dedicatedSeed`가 없는 `Tilled` 셀은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:121)에서 Planting 타겟으로 계속 선택되지만, 도착 후 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:22)에서 실패해 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. `FindByState()` 대신 `FindByPredicate(cells, cell => cell.CanPlant())`처럼 각 행동의 `CanXxx()` 기준으로 후보를 찾는 편이 맞습니다냥.

2. `_targetCell` 파괴 방어가 `IsTargetValid()`에는 들어갔지만, 작업 중 타겟이 파괴된 뒤 Idle로 전이할 때 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:96)의 `TransitionTo()`가 다시 `_targetCell.UnregisterFarmer()`를 직접 호출합니다냥. 예를 들어 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:30)에서 `IsTargetValid()` 실패 후 `TransitionTo(FarmerState.Idle)`로 들어오면 stale Unity 객체에 `UnregisterFarmer()`를 호출할 수 있어 `MissingReferenceException` 계열 문제가 남습니다냥. `SafeUnregisterTarget()` 같은 공통 메서드에서 Unity 객체 생존 여부를 확인하고, `OnDisable()`과 `TransitionTo()` 모두 그 메서드를 쓰는 방식이 안전합니다냥.

3. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:7)의 작업량, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:17)의 `FarmerEfficiencyCurve`, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:21)의 수확 설정에 대한 `OnValidate()` 또는 런타임 `Normalize()`가 없습니다냥. `FarmerEfficiencyCurve`가 null이면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:71)에서 즉시 예외가 나고, `MaxWork <= 0`, `NaturalGrowthRate < 0`, `MaxHarvestCount <= 0`, `HarvestYield <= 0` 같은 값은 자동 전이, 성장, 토큰 수확 흐름을 비정상 상태로 만들 수 있습니다냥. `FarmerConfig`처럼 `FarmCellConfig.Normalize()`와 `OnValidate()`를 추가해 최소값과 null 곡선을 보정하는 편이 좋습니다냥.

**반영 확인냥**

이전 흐름에서 지적됐던 `Boosting` 명칭은 `HelpingGrow`로 정리됐고, `HandleWorking()` 통합과 `RegisterFarmer()`/`UnregisterFarmer()` 기반 협력 구조도 반영된 것으로 보입니다냥. `TryConsumeHarvest(out int yield)` 토큰 패턴과 다중 수확 카운트도 큰 방향은 요구사항에 맞지만, 위 stale target 해제 경로와 실행 불가능한 셀 타겟팅은 아직 보완이 필요합니다냥.

이번 1회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥

---

## 2회차 리뷰 — 2026-04-22 17:32:11 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 2번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
셀 단위 협력 작업 시스템 재설계. FarmCell이 WorkProgress 소유, 농부들이 ContributeWork로 매 프레임 기여. AnimationCurve 효율 체감, 5회 수확 후 Untilled 리셋, Seeded 상태 자연 성장만.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 3회차 리뷰 (2026-04-22 00:48:22)
**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

### 1회차 리뷰 (2026-04-22 16:27:57)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:5)에서 분리된 partial 파일도 `public partial class FarmerAI : MonoBehaviour`로 선언되어 있습니다냥. Unity는 `MonoBehaviour` 스크립트의 파일명과 클래스명 매칭에 민감하므로 `FarmerBehaviors.cs`가 `FarmerAI`라는 public `MonoBehaviour`를 담고 있으면 스크립트 임포트 경고나 “클래스 이름과 파일 이름 불일치” 문제가 날 수 있습니다냥. 보조 partial 파일은 `public partial class FarmerAI { ... }`처럼 base class를 생략하고, 실제 `MonoBehaviour` 상속 표기는 `FarmerAI.cs` 한 곳에만 두는 편이 안전합니다냥.

2. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:13)의 `HandleMoveToCell()`은 `_targetCell == null`과 `TryGetCellPosition()`만 확인하고, [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:92)의 `IsTargetValid()`를 사용하지 않습니다냥. `_targetCell`이 Unity 객체로 파괴된 상태라면 인터페이스 참조의 null 비교는 통과할 수 있고, `FarmlandManager.TryGetCellPosition()` 내부에서 파괴된 `_cells[i].transform` 접근이 `MissingReferenceException`으로 이어질 수 있습니다냥. 이동 상태도 작업 완료 콜백과 동일하게 `IsTargetValid()`로 먼저 검증한 뒤 위치를 가져오도록 통일하는 것이 좋습니다냥.

**반영 확인냥**

이전 리뷰에서 남았던 `FarmerConfig` 런타임 값 보정은 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:38)의 `Normalize()`와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:28)의 호출로 반영된 것으로 보입니다냥.

행동 실행 로직을 `FarmerBehaviors.cs`로 이동하고 생명주기와 의사결정을 `FarmerAI.cs`에 남긴 큰 분리 방향은 요구사항과 맞습니다냥.

Unity 컴파일은 직접 실행하지 못했고, git 상태 확인은 저장소 소유권 안전 설정 문제로 확인하지 못했습니다냥.

### 2회차 리뷰 (2026-04-22 16:30:57)
**Key Findings냥**

추가 개선점은 없습니다냥.

1회차 지적사항이었던 `FarmerBehaviors.cs`의 `MonoBehaviour` 중복 선언은 제거되어 `public partial class FarmerAI` 형태로 정리됐습니다냥.

`HandleMoveToCell()`도 `IsTargetValid()`를 먼저 사용하도록 수정되어, 작업 완료 콜백과 부스트 처리 경로의 타겟 유효성 검증 방식과 일관성이 맞습니다냥.

요구사항대로 생명주기와 의사결정 로직은 `FarmerAI.cs`에 남고, 행동 실행 로직은 `FarmerBehaviors.cs`로 분리되어 순수 파일 분할 리팩터링 범위에 부합합니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

### 1회차 리뷰 (2026-04-22 17:27:16)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:81)의 `FindByState()`가 `State`만 보고 타겟을 고르기 때문에, 실제 `CanPlant()`나 `CanTill()`이 false인 셀도 실행 가능한 후보로 계산됩니다냥. 특히 `_dedicatedSeed`가 없는 `Tilled` 셀은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:121)에서 Planting 타겟으로 계속 선택되지만, 도착 후 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:22)에서 실패해 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. `FindByState()` 대신 `FindByPredicate(cells, cell => cell.CanPlant())`처럼 각 행동의 `CanXxx()` 기준으로 후보를 찾는 편이 맞습니다냥.

2. `_targetCell` 파괴 방어가 `IsTargetValid()`에는 들어갔지만, 작업 중 타겟이 파괴된 뒤 Idle로 전이할 때 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:96)의 `TransitionTo()`가 다시 `_targetCell.UnregisterFarmer()`를 직접 호출합니다냥. 예를 들어 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:30)에서 `IsTargetValid()` 실패 후 `TransitionTo(FarmerState.Idle)`로 들어오면 stale Unity 객체에 `UnregisterFarmer()`를 호출할 수 있어 `MissingReferenceException` 계열 문제가 남습니다냥. `SafeUnregisterTarget()` 같은 공통 메서드에서 Unity 객체 생존 여부를 확인하고, `OnDisable()`과 `TransitionTo()` 모두 그 메서드를 쓰는 방식이 안전합니다냥.

3. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:7)의 작업량, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:17)의 `FarmerEfficiencyCurve`, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:21)의 수확 설정에 대한 `OnValidate()` 또는 런타임 `Normalize()`가 없습니다냥. `FarmerEfficiencyCurve`가 null이면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:71)에서 즉시 예외가 나고, `MaxWork <= 0`, `NaturalGrowthRate < 0`, `MaxHarvestCount <= 0`, `HarvestYield <= 0` 같은 값은 자동 전이, 성장, 토큰 수확 흐름을 비정상 상태로 만들 수 있습니다냥. `FarmerConfig`처럼 `FarmCellConfig.Normalize()`와 `OnValidate()`를 추가해 최소값과 null 곡선을 보정하는 편이 좋습니다냥.

**반영 확인냥**

이전 흐름에서 지적됐던 `Boosting` 명칭은 `HelpingGrow`로 정리됐고, `HandleWorking()` 통합과 `RegisterFarmer()`/`UnregisterFarmer()` 기반 협력 구조도 반영된 것으로 보입니다냥. `TryConsumeHarvest(out int yield)` 토큰 패턴과 다중 수확 카운트도 큰 방향은 요구사항에 맞지만, 위 stale target 해제 경로와 실행 불가능한 셀 타겟팅은 아직 보완이 필요합니다냥.

이번 1회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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
        if (obj != null && !obj) return;
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

```

### 파일: Assets/Scripts/Actor/FarmerBehaviors.cs
```
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

    // 조건 술어를 만족하는 첫 번째 칸을 반환한다.
    private static IFarmCell FindFirstWhere(IReadOnlyList<IFarmCell> cells, System.Func<IFarmCell, bool> predicate) {
        foreach (var cell in cells)
            if (cell != null && predicate(cell)) return cell;
        return null;
    }
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
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

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
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

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
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
        HarvestYield      = Mathf.Max(0, HarvestYield);
        if (FarmerEfficiencyCurve == null || FarmerEfficiencyCurve.length == 0)
            FarmerEfficiencyCurve = AnimationCurve.Linear(0f, 0f, 5f, 2.7f);
    }
}

```

### 파일: Assets/Scripts/Data/SeedConfig.cs
```
// 씨앗 종류의 정체성 데이터. 작물 구분, 표시 이름, 수확량 등 메타데이터를 보관한다.
using UnityEngine;

[CreateAssetMenu(fileName = "SeedConfig", menuName = "Project_FAD/SeedConfig")]
public class SeedConfig : ScriptableObject {
    [Header("Identity")]
    public string DisplayName  = "Unknown Crop";
    public int    HarvestYield = 1;
    public Sprite Icon;
}

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
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

```

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    HelpingGrow,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (_cells.Length != 4)
            Debug.LogError($"[FarmlandManager] _cells 배열은 정확히 4개여야 합니다. 현재: {_cells.Length}", this);

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] == null)
                Debug.LogError($"[FarmlandManager] _cells[{i}]가 null입니다. Inspector에서 연결해주세요.", this);
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 position에 담아 반환한다. 존재하지 않으면 false.
    public bool TryGetCellPosition(IFarmCell cell, out Vector3 position) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) {
                position = _cells[i].transform.position;
                return true;
            }
        }
        position = Vector3.zero;
        return false;
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:24)에서 `_config` null만 검사하고 [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:33)의 `Normalize()`를 런타임에 호출하지 않습니다냥. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:31)의 `OnValidate()`는 에디터 중심이라 빌드 런타임에서 다른 코드가 public 값을 바꾸거나 잘못된 에셋 상태가 들어온 경우를 보장하지 못합니다냥. 특히 `FarmerEfficiencyCurve`가 null이 되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:71)에서 `Evaluate()` 호출 시 예외가 날 수 있으므로, `FarmCell.Awake()`에서 `_config.Normalize()`를 호출하거나 clamp된 읽기 전용 프로퍼티로 접근하는 편이 안전합니다냥.

2. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:42)에서 `HarvestYield`를 `Mathf.Max(0, HarvestYield)`로 보정해 0 수확량을 허용합니다냥. 이 값이 0이면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:107)에서 수확 작업 완료 후 `_lastHarvestYield`가 0이 되고, [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:78)의 `TryConsumeHarvest()`가 false를 반환해서 농부는 수확 작업을 끝냈지만 창고 반납 없이 Idle로 돌아갑니다냥. 요구사항상 수확이 실제 산출물을 의미한다면 `HarvestYield`는 최소 1로 clamp하고, 0 수확량을 의도적으로 지원하려면 `CanHarvest()`나 작업 완료 흐름에서 “수확 없음” 상태를 명시적으로 처리해야 합니다냥.

**반영 확인냥**

1회차에서 지적한 `FindByState()` 기반 타겟 선정 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:122)부터 `FindFirstWhere(cells, c => c.CanXxx())` 방식으로 바뀌어 해결된 것으로 보입니다냥.

stale target 해제 문제도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104)의 `SafeUnregisterTarget()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)의 전이 시 호출로 대부분 반영됐습니다냥.

`FarmCellConfig` 검증은 `Normalize()`와 `OnValidate()`가 추가되어 방향은 맞지만, 위 1번처럼 `FarmCell` 런타임 초기화에서 호출되지 않아 이전 지적이 완전히 닫히지는 않았습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---

## 3회차 리뷰 — 2026-04-22 17:35:57 (🔄 개선 필요)

### Claude → Codex 프롬프트

```
다음 코드를 한국어로 리뷰해주세요. 현재 3번째 리뷰입니다.

## 리뷰 기준
1. 버그 및 잠재적 오류
2. 성능 개선 가능 부분
3. 가독성 및 코드 구조
4. 모범 사례(Best Practice) 준수 여부
5. 이전 리뷰에서 지적된 사항이 반영되었는지 확인

## 중요 규칙
- 개선점이 없으면 응답 **마지막 줄**에 정확히 아래 문자열만 단독으로 출력하세요:
  REVIEW_DONE: NO_FURTHER_IMPROVEMENTS
- 개선점이 있으면 번호 목록으로 구체적으로 설명하세요.

## 요구사항/맥락
셀 단위 협력 작업 시스템 재설계. FarmCell이 WorkProgress 소유, 농부들이 ContributeWork로 매 프레임 기여. AnimationCurve 효율 체감, 5회 수확 후 Untilled 리셋, Seeded 상태 자연 성장만.

## 이전 리뷰 히스토리

### 1회차 리뷰 (2026-04-20 01:48:01)
**Key Findings냥**

1. `Assets/Scripts/Actor/FarmCell.cs:5`, `:28`, `:35`, `:56`에서 `_config`가 할당되지 않은 경우 `Awake()`의 `ApplyStateVisual()` 또는 `PlantSeed()`, `Boost()`, `GetStateColor()`에서 즉시 `NullReferenceException`이 발생할 수 있습니다냥. `FarmCellConfig`는 필수 의존성이므로 `Awake()`에서 null 검증 후 비활성화하거나, 에디터 검증(`OnValidate`)과 명확한 에러 로그를 추가하는 것이 안전합니다냥.

2. `Assets/Scripts/Actor/FarmCell.cs:48`의 `SetState()`가 항상 `ApplyStateVisual()`을 호출해서 선택된 셀의 하이라이트가 행동 직후 사라집니다냥. 예를 들어 선택된 셀에서 `Q/W/E`를 누르면 상태 색상으로 덮어써지고, 다시 숫자키를 눌러 선택을 갱신하기 전까지 선택 표시가 유지되지 않습니다냥. `_isHighlighted` 상태를 `FarmCell` 내부에 저장하고 `ApplyStateVisual()`에서 하이라이트까지 함께 반영하거나, 상태 변경 후 매니저가 `RefreshHighlight()`를 호출하도록 구조를 맞추는 편이 좋습니다냥.

3. `Assets/Scripts/Manager/FarmlandManager.cs:5`, `:38`, `:48`에서 매니저가 `FarmCell` 구체 타입에 직접 의존하고 있어 요구사항의 `Interface DI`와 다소 어긋납니다냥. 행동 호출은 `IFarmCell`을 통해 처리하고, 하이라이트가 필요하다면 별도 뷰 인터페이스를 두거나 `IFarmCell`에 선택 표시 책임을 포함할지 명확히 분리하는 것이 구조적으로 더 낫습니다냥.

4. `Assets/Scripts/Actor/FarmCell.cs:35`와 `:41`의 성장 처리에서 부스트나 큰 `deltaTime`으로 타이머가 크게 음수가 되어도 한 단계만 진행하고 초과 감소분은 버려집니다냥. 현재 값이 의도라면 문제 없지만, “시간 단축” 의미라면 남은 초과 시간을 다음 성장 단계에 이월하거나 반복 advance 처리를 해야 예측 가능한 성장 속도가 됩니다냥.

5. 이전 리뷰는 현재 1번째 리뷰이므로 반영 여부를 비교할 이전 지적사항은 없습니다냥.

### 2회차 리뷰 (2026-04-20 01:50:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리는 `Awake()`에서 로그만 남기고 끝나므로, 이후 `PlantSeed()`가 호출되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:43)에서 `_config.SeededToGrowingSeconds` 접근으로 여전히 `NullReferenceException`이 발생할 수 있습니다냥. 1회차의 null 가드 요구는 `ApplyStateVisual()` 쪽에는 반영됐지만, 공개 액션 메서드 전체에는 아직 완전히 반영되지 않았습니다냥. `Awake()`에서 `enabled = false`로 컴포넌트를 비활성화하거나, `PlantSeed()`, `Boost()`, `AdvanceGrowth()` 진입 전에 `_config` 유효성을 보장하는 처리가 필요합니다냥.

2. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:26)와 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:49)의 `while` 루프는 추가됐지만, 초과 시간을 다음 성장 단계로 이월하지 못합니다냥. 예를 들어 Seeded 상태에서 `_growthTimer`가 `-3f`가 된 경우 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `GrowingToGrownSeconds`로 덮어써져 3초 초과분이 사라집니다냥. 1회차에서 지적된 “부스트나 큰 deltaTime의 초과 감소분 처리”는 루프만으로는 해결되지 않았으므로, `AdvanceGrowth()`가 남은 초과 시간을 받아 다음 타이머에서 차감하도록 구조를 바꾸는 편이 맞습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:6)의 `_cells`가 여전히 `FarmCell[]` 구체 타입이라 1회차의 Interface DI 지적은 부분 반영 상태입니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:32)에서 선택 셀을 `IFarmCell`로 받는 점은 개선됐지만, 직렬화 필드와 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:52)의 캐스팅은 여전히 구현체에 묶여 있습니다냥. Unity 직렬화 제약을 고려하면 `MonoBehaviour[]`로 받고 `IFarmCell` 구현 여부를 `Awake()`나 `OnValidate()`에서 검증하거나, 별도 셀 뷰/모델 인터페이스를 분리하는 방식이 더 명확합니다냥.

**반영 확인냥**

1회차 지적 중 `IFarmCell.SetHighlight()` 추가, `FarmCell`의 `_isHighlighted` 플래그와 시각 갱신 유지, 행동 후 `RefreshHighlight()` 호출은 반영됐습니다냥. 다만 null 의존성 방어, 성장 타이머 초과분 이월, 구체 타입 의존 제거는 아직 개선 여지가 남아 있습니다냥. 이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 01:52:34)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:16)의 `_config == null` 처리에서 `enabled = false`를 추가한 점은 2회차 지적을 일부 반영했지만, 외부에서 `IFarmCell` 참조로 public 메서드를 호출하는 경로는 여전히 막지 못합니다냥. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:17)에서 비활성화된 `FarmCell`도 그대로 `_farmCells`에 캐시되므로, 설정이 빠진 셀도 `Q`로 `Till()` 상태 전환 후 `W` 입력 시 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44)의 `_config.SeededToGrowingSeconds` 접근에서 `NullReferenceException`이 발생할 수 있습니다냥. `CanTill()`, `CanPlant()`, `CanBoost()`가 `_config != null`을 함께 확인하게 하거나, public 액션 메서드 진입부에서 설정 유효성을 공통 가드로 막는 처리가 필요합니다냥.

**반영 확인냥**

2회차의 성장 타이머 초과분 이월 지적은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:62)에서 `overflow`를 계산하고 다음 단계 타이머에 반영하도록 개선되어 해결된 것으로 보입니다냥. FarmlandManager도 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:15)에서 `IFarmCell[]` 캐시를 구성한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:38)부터 캐스팅 없이 사용하는 형태로 반영되었습니다냥. 이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 1회차 리뷰 (2026-04-20 19:20:38)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:26)에서 `_warehouse = WarehouseManager.Instance`를 그대로 캐시한 뒤 null 검증이 없어서, 씬에 `WarehouseManager`가 없거나 비활성 상태면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:87)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:92)에서 `NullReferenceException`이 발생합니다냥. `Start()`에서 `_warehouse == null`이면 에러 로그 후 `enabled = false` 처리하거나, `IWarehouse`를 구현한 `MonoBehaviour`를 직렬화 필드로 받아 검증하는 방식이 안전합니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:46)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:58)에서 `FarmlandManager.Instance`가 항상 존재한다고 가정합니다냥. 씬 조립 누락 시 Idle 진입 즉시 예외가 나므로, `FarmerAI` 초기화 시 `FarmlandManager.Instance`, `AllCells`, `DefaultSeed` 유효성을 한 번 검증하고 실패하면 AI를 비활성화하는 편이 좋습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells` 배열을 검증 없이 `_farmCells`로 복사하므로, 인스펙터 슬롯이 비어 있으면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:125)의 `cell.State` 접근에서 null 예외가 납니다냥. `Awake()` 또는 `OnValidate()`에서 배열 길이 4, null 슬롯 없음, `IFarmCell` 구현 여부를 검사하고 잘못된 셀은 명확히 로그로 알려야 합니다냥.

4. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:98), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:110)의 작업 완료 콜백은 실제 작업 성공 여부와 무관하게 다음 상태로 전이합니다냥. 작업 대기 시간 동안 플레이어나 다른 시스템이 셀 상태를 바꾸면 `Till()`이나 `PlantSeed()`가 no-op이어도 AI는 Planting 또는 Boosting으로 넘어가고, `Harvest()`가 0을 반환해도 창고로 이동해 0개를 반납할 수 있습니다냥. 작업 실행 직전에 `CanTill/CanPlant/CanHarvest`를 재검증하고, 실패 시 `Idle`로 복귀하거나 인터페이스 메서드가 성공 여부를 반환하도록 바꾸는 편이 정확합니다냥.

5. 이전 리뷰의 핵심 지적 중 `FarmCellConfig` null 방어는 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:38), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:41), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:44), [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:70)의 `Can*` 가드로 반영됐고, 성장 타이머 초과분 이월도 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:87)부터 반영되어 해결된 것으로 보입니다냥. 다만 새로 추가된 `FarmerAI`가 Singleton 매니저에 직접 의존하므로 요구사항의 `IFarmCell/IWarehouse` 의존 방향은 행동 호출부에서는 지켜졌지만 초기 참조 획득 방식에서는 아직 씬 전역 객체 결합이 남아 있습니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 2회차 리뷰 (2026-04-20 19:23:04)
**Key Findings냥**

1. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:27)에서 `_cells[i] == null`을 로그로만 알린 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:29)에 그대로 null을 `_farmCells`에 넣습니다냥. 이 상태에서 `FarmerAI`가 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:136)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:143)의 `cell.State`에 접근하면 `NullReferenceException`이 발생합니다냥. 1회차의 `_cells` null 체크와 로그 추가는 반영됐지만, 런타임 실패 경로는 아직 남아 있으므로 잘못된 슬롯이 있으면 매니저나 AI를 비활성화하거나, `_farmCells`에서 null을 제외하고 탐색부도 null을 건너뛰게 해야 합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:9)의 `_defaultSeed` 검증이 아직 없습니다냥. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에서 null 씨앗을 넘기면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:54)가 조용히 no-op으로 빠지지만, AI는 곧바로 `Boosting`으로 전이해서 다음 프레임 Idle로 돌아가고 같은 `Tilled` 셀을 계속 재시도할 수 있습니다냥. `Start()`나 `FarmlandManager.Awake()`에서 `DefaultSeed` 누락 시 명확히 비활성화하거나, `PlantSeed` 성공 여부를 반환받아 성공할 때만 `Boosting`으로 전이하는 편이 안전합니다냥.

3. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:123)에서 `Harvest()` 반환값이 0이어도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:124)에서 항상 창고 이동으로 전이합니다냥. 현재 `FarmCell` 구현만 보면 직전 `CanHarvest()` 재검증으로 대부분 막히지만, `IFarmCell.Harvest()` 계약 자체가 실패 시 0 반환이므로 다른 구현체나 상태 경쟁을 고려하면 0 수확물 반납 경로가 남습니다냥. `_heldCrop <= 0`이면 Idle 복귀와 경고 로그를 남기거나, `Harvest`를 성공 여부와 수량을 함께 표현하는 계약으로 바꾸는 것이 더 견고합니다냥.

**반영 확인냥**

1회차 지적 중 `FarmerAI.Start()`의 `_warehouse`와 `FarmlandManager.Instance` null 시 `enabled = false` 처리는 반영됐습니다냥. `OnTillDone`, `OnPlantDone`, `OnHarvestDone`의 작업 직전 `CanXxx` 재검증 후 Idle 복귀도 반영됐습니다냥. `FarmlandManager.Awake()`의 `_cells` null 로그도 추가됐지만, null 항목이 `AllCells`에 그대로 노출되어 위 1번 문제가 남아 있습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

### 3회차 리뷰 (2026-04-20 19:38:02)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:84), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:115), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:128)에서 `_targetCell`이 계속 유효하다고 가정합니다냥. `Idle`에서 셀을 잡은 뒤 이동이나 작업 대기 중 해당 셀이 파괴되거나 매니저 배열에서 빠지면 `GetCellPosition(_targetCell)` 또는 `_targetCell.CanXxx()` 호출에서 예외가 날 수 있습니다냥. 상태 처리 진입부마다 `_targetCell == null` 및 Unity 객체 파괴 여부를 확인해 `Idle`로 복귀하거나, `FarmlandManager.TryGetCellPosition(IFarmCell cell, out Vector3 position)`처럼 성공 여부를 반환하는 API로 바꾸는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:59)의 `GetCellPosition()`이 대상 셀을 찾지 못하면 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:63)에서 `Vector3.zero`를 반환합니다냥. 이 경우 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:67)의 AI가 실패를 감지하지 못하고 월드 원점으로 이동한 뒤 기존 `_targetCell`에 작업을 시도하는 침묵 실패가 됩니다냥. 존재하지 않는 셀은 명확히 실패로 표현하고, 호출부가 경고 후 `Idle`로 돌아가도록 처리하는 것이 디버깅과 런타임 안정성 면에서 낫습니다냥.

3. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:25)에서 `_cells.Length`를 그대로 신뢰하므로 2x2 매니저라는 주석과 달리 인스펙터에서 배열 크기가 4가 아니어도 런타임이 그대로 진행됩니다냥. 현재 AI 탐색은 null을 건너뛰므로 즉시 예외 가능성은 줄었지만, 씬 조립 오류를 초기에 잡지 못해 선택 인덱스, 하이라이트, AI 탐색 범위가 의도와 달라질 수 있습니다냥. `Awake()` 또는 `OnValidate()`에서 길이 4와 null 슬롯을 검증하고, 잘못된 구성은 명확히 로그 처리하는 편이 좋습니다냥.

**반영 확인냥**

2회차에서 지적된 `FindByState()`와 `FindBoostable()`의 null 항목 건너뛰기는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:146)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:153)에 반영됐습니다냥.

`OnPlantDone()`에서 `DefaultSeed`가 null이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:116)에 반영됐습니다냥.

`OnHarvestDone()`에서 `_heldCrop <= 0`이면 경고 후 `Idle`로 복귀하는 처리도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:129)에 반영됐습니다냥.

이번 3회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 1회차 리뷰 (2026-04-22 00:43:28)
**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:69)와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:255)에서 타겟이 없는 행동도 `AvailableTargetMultiplier`만 빠질 뿐 기본 가중치가 그대로 남아 선택될 수 있습니다냥. 예를 들어 `Untilled` 셀이 없는데 `TillBaseWeight > 0`이면 Tilling 후보가 뽑히고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:228)에서 `_targetCell = null`인 채 `MovingToCell`로 전이한 뒤 다음 프레임 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. 실행 불가능한 후보는 Loitering 같은 예외 행동을 제외하고 가중치를 0으로 만들거나, `ActionCandidate`에 `CanRun` 개념을 두는 편이 맞습니다냥.

2. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:141), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:148), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:161)의 작업 완료 콜백은 `_targetCell`이 여전히 유효하다고 가정합니다냥. 이동 중 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)에 반영됐지만, 작업 대기 중 셀이 파괴되거나 매니저에서 제거되면 `CanBoost()`, `CanTill()`, `CanPlant()`, `CanHarvest()` 호출에서 예외가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드로 null 및 `TryGetCellPosition` 실패를 재검증한 뒤 실패 시 Idle로 복귀하는 처리가 필요합니다냥.

3. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:7)부터 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:32)까지 런타임에 민감한 값들이 public으로 열려 있지만 범위 검증이 없습니다냥. 특히 `RecencyHistorySize`가 음수면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:273)의 `while`에서 빈 큐에 `Dequeue()`를 호출할 수 있고, `MoveSpeed <= 0`, `InteractionRange < 0`, 작업 시간이나 `BoostIntervalSeconds <= 0`도 AI가 멈추거나 매 프레임 즉시 작업하는 비정상 동작으로 이어질 수 있습니다냥. `OnValidate()`에서 최소값을 clamp하고, 런타임 초기화에서도 치명적인 설정 오류는 로그 후 비활성화하는 것이 안전합니다냥.

**반영 확인냥**

이전 리뷰에서 지적된 `GetCellPosition()`의 `Vector3.zero` 침묵 실패는 `TryGetCellPosition()`으로 바뀌어 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)에서 실패를 감지하도록 개선됐습니다냥. 다만 `_targetCell` 유효성 재검증은 이동 상태에만 반영되어 작업 및 부스트 상태에는 아직 남아 있습니다냥.

리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md:91)에 기록했습니다냥.

### 2회차 리뷰 (2026-04-22 00:45:44)
**Key Findings**냥

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117)의 `HandleBoosting()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)의 작업 완료 콜백은 여전히 `_targetCell == null`과 `CanXxx()`만 확인합니다냥. 1회차에서 지적된 이동 중 유효성 검사는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:97)의 `TryGetCellPosition()`으로 반영됐지만, 작업 대기 중 타겟 셀이 매니저에서 빠지거나 Unity 객체가 파괴된 경우에는 작업 완료 시점에 stale target을 조작하거나 `MissingReferenceException` 계열 문제가 날 수 있습니다냥. `IsTargetValid()` 같은 공통 가드에서 `_targetCell`을 `UnityEngine.Object`로 캐스팅해 생존 여부를 확인하고, `FarmlandManager.Instance.TryGetCellPosition(_targetCell, out _)`까지 재검증한 뒤 `HandleBoosting()`과 `OnTillDone/OnPlantDone/OnHarvestDone()` 모두에서 사용하면 이전 지적이 완전히 닫힙니다냥.

2. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`로 최소값 보정이 추가된 점은 1회차 설정값 검증 지적을 상당 부분 반영했습니다냥. 다만 `OnValidate()`는 에디터 검증 중심이라 런타임 코드가 public 필드를 바꾸거나, 잘못된 값이 빌드에 들어간 경우까지 보장하지 못합니다냥. 특히 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:275)의 `RecencyHistorySize` 의존 로직은 런타임에 음수로 바뀌면 빈 큐에서 `Dequeue()`를 호출할 수 있으므로, `FarmerConfig`에 `Normalize()`나 읽기 전용 프로퍼티를 두고 `FarmerAI.Awake()`에서도 한 번 검증하는 편이 더 안전합니다냥.

**반영 확인**냥

1회차 지적 중 타겟 없는 비-Loiter 행동 후보가 선택되던 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:254)의 `if (kind != FarmerState.Loitering && !hasTarget) return 0f;`로 반영됐습니다냥. `FarmerConfig`의 기본 범위 검증도 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)부터 추가되어 개선됐습니다냥. 다만 타겟 유효성 재검증은 이동 상태에만 충분히 들어갔고, 런타임 설정값 불변성은 아직 보강 여지가 남아 있습니다냥.

이번 2회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥.

### 3회차 리뷰 (2026-04-22 00:48:22)
**Key Findings냥**

1. [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:35)의 `OnValidate()`만으로는 런타임 설정값 안정성이 보장되지 않습니다냥. `FarmerConfig`의 필드들이 모두 public이라 실행 중 다른 코드가 `RecencyHistorySize`를 음수로 바꾸면 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:283)의 `while` 루프가 빈 큐에서 `Dequeue()`를 호출할 수 있고, `MoveSpeed`, `InteractionRange`, 작업 시간, `BoostIntervalSeconds`도 런타임 변경으로 비정상 이동이나 즉시 반복 작업을 만들 수 있습니다냥. 2회차에서 남긴 런타임 검증 지적은 아직 완전히 닫히지 않았으므로, `FarmerConfig.Normalize()` 같은 공통 보정 메서드를 만들고 `FarmerAI.Awake()`에서 호출하거나, public 필드 대신 clamp된 읽기 전용 프로퍼티를 사용하도록 바꾸는 편이 안전합니다냥.

**반영 확인냥**

이전 2회차의 `_targetCell` stale target 지적은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:174)의 `IsTargetValid()`가 추가되고, [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:117), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:142), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:149), [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:162)에 적용되어 해결된 것으로 보입니다냥.

타겟 없는 비-Loiter 행동 후보를 0점 처리하는 로직도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:262)에 유지되어 1회차 지적은 계속 해결된 상태입니다냥.

성능 측면에서는 `Idle` 전이 시 후보 리스트와 클로저가 생성되지만 현재 규모에서는 실질적인 병목으로 보기 어렵고, 구조상 “새 행동은 `BuildActionCandidates`에 추가”라는 요구사항도 대체로 충족합니다냥.

이번 3회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥

### 1회차 리뷰 (2026-04-22 16:27:57)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:5)에서 분리된 partial 파일도 `public partial class FarmerAI : MonoBehaviour`로 선언되어 있습니다냥. Unity는 `MonoBehaviour` 스크립트의 파일명과 클래스명 매칭에 민감하므로 `FarmerBehaviors.cs`가 `FarmerAI`라는 public `MonoBehaviour`를 담고 있으면 스크립트 임포트 경고나 “클래스 이름과 파일 이름 불일치” 문제가 날 수 있습니다냥. 보조 partial 파일은 `public partial class FarmerAI { ... }`처럼 base class를 생략하고, 실제 `MonoBehaviour` 상속 표기는 `FarmerAI.cs` 한 곳에만 두는 편이 안전합니다냥.

2. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:13)의 `HandleMoveToCell()`은 `_targetCell == null`과 `TryGetCellPosition()`만 확인하고, [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:92)의 `IsTargetValid()`를 사용하지 않습니다냥. `_targetCell`이 Unity 객체로 파괴된 상태라면 인터페이스 참조의 null 비교는 통과할 수 있고, `FarmlandManager.TryGetCellPosition()` 내부에서 파괴된 `_cells[i].transform` 접근이 `MissingReferenceException`으로 이어질 수 있습니다냥. 이동 상태도 작업 완료 콜백과 동일하게 `IsTargetValid()`로 먼저 검증한 뒤 위치를 가져오도록 통일하는 것이 좋습니다냥.

**반영 확인냥**

이전 리뷰에서 남았던 `FarmerConfig` 런타임 값 보정은 [FarmerConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmerConfig.cs:38)의 `Normalize()`와 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:28)의 호출로 반영된 것으로 보입니다냥.

행동 실행 로직을 `FarmerBehaviors.cs`로 이동하고 생명주기와 의사결정을 `FarmerAI.cs`에 남긴 큰 분리 방향은 요구사항과 맞습니다냥.

Unity 컴파일은 직접 실행하지 못했고, git 상태 확인은 저장소 소유권 안전 설정 문제로 확인하지 못했습니다냥.

### 2회차 리뷰 (2026-04-22 16:30:57)
**Key Findings냥**

추가 개선점은 없습니다냥.

1회차 지적사항이었던 `FarmerBehaviors.cs`의 `MonoBehaviour` 중복 선언은 제거되어 `public partial class FarmerAI` 형태로 정리됐습니다냥.

`HandleMoveToCell()`도 `IsTargetValid()`를 먼저 사용하도록 수정되어, 작업 완료 콜백과 부스트 처리 경로의 타겟 유효성 검증 방식과 일관성이 맞습니다냥.

요구사항대로 생명주기와 의사결정 로직은 `FarmerAI.cs`에 남고, 행동 실행 로직은 `FarmerBehaviors.cs`로 분리되어 순수 파일 분할 리팩터링 범위에 부합합니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

REVIEW_DONE: NO_FURTHER_IMPROVEMENTS

### 1회차 리뷰 (2026-04-22 17:27:16)
**Key Findings냥**

1. [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:81)의 `FindByState()`가 `State`만 보고 타겟을 고르기 때문에, 실제 `CanPlant()`나 `CanTill()`이 false인 셀도 실행 가능한 후보로 계산됩니다냥. 특히 `_dedicatedSeed`가 없는 `Tilled` 셀은 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:121)에서 Planting 타겟으로 계속 선택되지만, 도착 후 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:22)에서 실패해 Idle로 돌아가는 빈 행동이 반복될 수 있습니다냥. `FindByState()` 대신 `FindByPredicate(cells, cell => cell.CanPlant())`처럼 각 행동의 `CanXxx()` 기준으로 후보를 찾는 편이 맞습니다냥.

2. `_targetCell` 파괴 방어가 `IsTargetValid()`에는 들어갔지만, 작업 중 타겟이 파괴된 뒤 Idle로 전이할 때 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:96)의 `TransitionTo()`가 다시 `_targetCell.UnregisterFarmer()`를 직접 호출합니다냥. 예를 들어 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:30)에서 `IsTargetValid()` 실패 후 `TransitionTo(FarmerState.Idle)`로 들어오면 stale Unity 객체에 `UnregisterFarmer()`를 호출할 수 있어 `MissingReferenceException` 계열 문제가 남습니다냥. `SafeUnregisterTarget()` 같은 공통 메서드에서 Unity 객체 생존 여부를 확인하고, `OnDisable()`과 `TransitionTo()` 모두 그 메서드를 쓰는 방식이 안전합니다냥.

3. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:7)의 작업량, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:17)의 `FarmerEfficiencyCurve`, [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:21)의 수확 설정에 대한 `OnValidate()` 또는 런타임 `Normalize()`가 없습니다냥. `FarmerEfficiencyCurve`가 null이면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:71)에서 즉시 예외가 나고, `MaxWork <= 0`, `NaturalGrowthRate < 0`, `MaxHarvestCount <= 0`, `HarvestYield <= 0` 같은 값은 자동 전이, 성장, 토큰 수확 흐름을 비정상 상태로 만들 수 있습니다냥. `FarmerConfig`처럼 `FarmCellConfig.Normalize()`와 `OnValidate()`를 추가해 최소값과 null 곡선을 보정하는 편이 좋습니다냥.

**반영 확인냥**

이전 흐름에서 지적됐던 `Boosting` 명칭은 `HelpingGrow`로 정리됐고, `HandleWorking()` 통합과 `RegisterFarmer()`/`UnregisterFarmer()` 기반 협력 구조도 반영된 것으로 보입니다냥. `TryConsumeHarvest(out int yield)` 토큰 패턴과 다중 수확 카운트도 큰 방향은 요구사항에 맞지만, 위 stale target 해제 경로와 실행 불가능한 셀 타겟팅은 아직 보완이 필요합니다냥.

이번 1회차 리뷰 요약은 [review_log.md](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/AI_CONTEXT_Codex/review_log.md)에 기록했습니다냥

### 2회차 리뷰 (2026-04-22 17:32:11)
**Key Findings냥**

1. [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:24)에서 `_config` null만 검사하고 [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:33)의 `Normalize()`를 런타임에 호출하지 않습니다냥. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:31)의 `OnValidate()`는 에디터 중심이라 빌드 런타임에서 다른 코드가 public 값을 바꾸거나 잘못된 에셋 상태가 들어온 경우를 보장하지 못합니다냥. 특히 `FarmerEfficiencyCurve`가 null이 되면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:71)에서 `Evaluate()` 호출 시 예외가 날 수 있으므로, `FarmCell.Awake()`에서 `_config.Normalize()`를 호출하거나 clamp된 읽기 전용 프로퍼티로 접근하는 편이 안전합니다냥.

2. [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:42)에서 `HarvestYield`를 `Mathf.Max(0, HarvestYield)`로 보정해 0 수확량을 허용합니다냥. 이 값이 0이면 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:107)에서 수확 작업 완료 후 `_lastHarvestYield`가 0이 되고, [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:78)의 `TryConsumeHarvest()`가 false를 반환해서 농부는 수확 작업을 끝냈지만 창고 반납 없이 Idle로 돌아갑니다냥. 요구사항상 수확이 실제 산출물을 의미한다면 `HarvestYield`는 최소 1로 clamp하고, 0 수확량을 의도적으로 지원하려면 `CanHarvest()`나 작업 완료 흐름에서 “수확 없음” 상태를 명시적으로 처리해야 합니다냥.

**반영 확인냥**

1회차에서 지적한 `FindByState()` 기반 타겟 선정 문제는 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:122)부터 `FindFirstWhere(cells, c => c.CanXxx())` 방식으로 바뀌어 해결된 것으로 보입니다냥.

stale target 해제 문제도 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:104)의 `SafeUnregisterTarget()`과 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:95)의 전이 시 호출로 대부분 반영됐습니다냥.

`FarmCellConfig` 검증은 `Normalize()`와 `OnValidate()`가 추가되어 방향은 맞지만, 위 1번처럼 `FarmCell` 런타임 초기화에서 호출되지 않아 이전 지적이 완전히 닫히지는 않았습니다냥.

이번 2회차 리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

## 리뷰 대상 코드

### 파일: Assets/Scripts/Actor/FarmerAI.cs
```
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
        if (obj != null && !obj) return;
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

```

### 파일: Assets/Scripts/Actor/FarmerBehaviors.cs
```
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

    // 조건 술어를 만족하는 첫 번째 칸을 반환한다.
    private static IFarmCell FindFirstWhere(IReadOnlyList<IFarmCell> cells, System.Func<IFarmCell, bool> predicate) {
        foreach (var cell in cells)
            if (cell != null && predicate(cell)) return cell;
        return null;
    }
}

```

### 파일: Assets/Scripts/Actor/FarmCell.cs
```
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

```

### 파일: Assets/Scripts/Interface/IFarmCell.cs
```
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

```

### 파일: Assets/Scripts/Data/FarmCellConfig.cs
```
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

```

### 파일: Assets/Scripts/Data/SeedConfig.cs
```
// 씨앗 종류의 정체성 데이터. 작물 구분, 표시 이름, 수확량 등 메타데이터를 보관한다.
using UnityEngine;

[CreateAssetMenu(fileName = "SeedConfig", menuName = "Project_FAD/SeedConfig")]
public class SeedConfig : ScriptableObject {
    [Header("Identity")]
    public string DisplayName  = "Unknown Crop";
    public int    HarvestYield = 1;
    public Sprite Icon;
}

```

### 파일: Assets/Scripts/Data/FarmerConfig.cs
```
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

```

### 파일: Assets/Scripts/Enum/FarmerState.cs
```
// AI 농부가 가질 수 있는 작업 상태를 정의한다.
public enum FarmerState {
    Idle,
    Loitering,
    MovingToCell,
    Tilling,
    Planting,
    HelpingGrow,
    Harvesting,
    MovingToWarehouse,
    Depositing
}

```

### 파일: Assets/Scripts/Manager/FarmlandManager.cs
```
// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using System.Collections.Generic;
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // 모든 농경지 칸을 읽기 전용으로 반환한다.
    public IReadOnlyList<IFarmCell> AllCells => _farmCells;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (_cells.Length != 4)
            Debug.LogError($"[FarmlandManager] _cells 배열은 정확히 4개여야 합니다. 현재: {_cells.Length}", this);

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i] == null)
                Debug.LogError($"[FarmlandManager] _cells[{i}]가 null입니다. Inspector에서 연결해주세요.", this);
            _farmCells[i] = _cells[i];
        }
    }

    // 초기 하이라이트 상태를 적용한다.
    private void Start() {
        RefreshHighlight();
    }

    // 선택 인덱스를 변경하고 하이라이트를 갱신한다.
    private void SelectCell(int index) {
        if (index < 0 || index >= _farmCells.Length) return;
        _selectedIndex = index;
        RefreshHighlight();
    }

    // 모든 칸의 하이라이트를 현재 선택 인덱스 기준으로 갱신한다.
    private void RefreshHighlight() {
        for (int i = 0; i < _farmCells.Length; i++) {
            _farmCells[i]?.SetHighlight(i == _selectedIndex);
        }
    }

    // 현재 선택된 칸을 IFarmCell로 반환한다.
    private IFarmCell SelectedCell() {
        if (_selectedIndex < 0 || _selectedIndex >= _farmCells.Length) return null;
        return _farmCells[_selectedIndex];
    }

    // 지정한 IFarmCell 칸의 월드 좌표를 position에 담아 반환한다. 존재하지 않으면 false.
    public bool TryGetCellPosition(IFarmCell cell, out Vector3 position) {
        for (int i = 0; i < _farmCells.Length; i++) {
            if (_farmCells[i] == cell) {
                position = _cells[i].transform.position;
                return true;
            }
        }
        position = Vector3.zero;
        return false;
    }
}

```

```

### Codex 응답

**Key Findings냥**

1. [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:106)와 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:68)의 `obj != null && !obj` 판정은 Unity 파괴 객체 방어로 의도대로 동작하지 않을 수 있습니다냥. 파괴된 `UnityEngine.Object`는 Unity의 overloaded null 비교에서 `obj != null`이 false가 되므로 뒤의 `!obj`까지 가지 못하고, 결국 [FarmerAI.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerAI.cs:108)의 `_targetCell.UnregisterFarmer()`나 [FarmerBehaviors.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmerBehaviors.cs:70)의 `TryGetCellPosition()` 호출로 진행될 수 있습니다냥. 이전 리뷰의 stale target 방어는 구조적으로 추가됐지만 이 조건식 때문에 완전히 닫히지 않았으므로, `cell is UnityEngine.Object obj && obj == null` 형태의 공통 생존 검사로 바꾸고 실패 시 `_targetCell = null`까지 정리하는 편이 안전합니다냥.

2. [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:60)에서 `_farmCells[i] == cell`만 확인한 뒤 [FarmlandManager.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Manager/FarmlandManager.cs:61)의 `_cells[i].transform.position`에 접근하므로, 해당 `FarmCell`이 파괴된 상태면 `MissingReferenceException`이 날 수 있습니다냥. 위 stale target 문제와 같은 축의 런타임 안정성 문제이므로, `TryGetCellPosition()` 내부에서도 `_cells[i] == null` 또는 파괴된 Unity 객체를 건너뛰고 false를 반환하도록 방어하는 것이 좋습니다냥.

**반영 확인냥**

2회차에서 지적한 `FarmCellConfig.Normalize()` 런타임 호출은 [FarmCell.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Actor/FarmCell.cs:31)에 반영됐고, `HarvestYield` 최소 1 보정도 [FarmCellConfig.cs](C:/Users/Merkatte/Desktop/QuickSort/Project_FAD/Assets/Scripts/Data/FarmCellConfig.cs:43)에 반영됐습니다냥.

`FindFirstWhere(cells, c => c.CanXxx())` 기반 타겟 선정도 유지되어 실행 불가능한 셀을 반복 선택하던 문제는 해결된 상태로 보입니다냥.

리뷰 요약은 `AI_CONTEXT_Codex/review_log.md`에 기록했습니다냥.

---
