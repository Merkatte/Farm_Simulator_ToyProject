// 수확물을 보관하는 창고 Singleton. 총 보관량을 관리한다.
using UnityEngine;

public class WarehouseManager : MonoBehaviour, IWarehouse {
    public static WarehouseManager Instance { get; private set; }

    private int _storedCount;

    public Vector3 Position => new Vector3(4, -2, 0);

    // Singleton 중복 가드를 수행한다.
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 수확물을 보관하고 로그를 출력한다.
    public void Deposit(int amount) {
        _storedCount += amount;
        Debug.Log($"[WarehouseManager] Deposited: {amount} (total={_storedCount})");
    }
}
