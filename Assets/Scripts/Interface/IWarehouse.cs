// 창고와 상호작용하는 공용 계약. AI가 수확물을 보관할 때 이 인터페이스를 사용한다.
using UnityEngine;

public interface IWarehouse {
    // 창고의 월드 좌표를 반환한다.
    Vector3 Position { get; }

    // 지정한 수량의 수확물을 보관한다.
    void Deposit(int amount);
}
