// 2x2 농경지 칸 배열을 관리하는 Singleton Manager. 칸 선택 상태와 하이라이트를 제어한다.
using UnityEngine;

public class FarmlandManager : MonoBehaviour {
    public static FarmlandManager Instance { get; private set; }

    [SerializeField] private FarmCell[] _cells = new FarmCell[4];

    private IFarmCell[] _farmCells;
    private int _selectedIndex = 0;

    // Singleton 중복 가드 및 IFarmCell 캐시 배열을 초기화한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _farmCells = new IFarmCell[_cells.Length];
        for (int i = 0; i < _cells.Length; i++) {
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
}
