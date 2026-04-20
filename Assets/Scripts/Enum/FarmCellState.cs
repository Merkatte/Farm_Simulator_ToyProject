// 농경지 칸이 가질 수 있는 상태를 정의한다. Untilled → Tilled → Seeded → Growing → Grown 순으로 전이한다.
public enum FarmCellState {
    Untilled,
    Tilled,
    Seeded,
    Growing,
    Grown
}
