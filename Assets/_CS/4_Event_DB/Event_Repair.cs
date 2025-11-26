using UnityEngine;

[CreateAssetMenu(fileName = "New Repair Event", menuName = "Game/Events/Repair")]
public class Event_Repair : GameEvent
{
    [Header("수리 설정")]
    public int repairAmount; // 내구도 회복량 (브론즈:3, 실버:5 ...)

    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] '{rarity}' 수리 이벤트 발생! (내구도 +{repairAmount})");

        // TODO: UIManager에게 '수리 창'을 띄우라고 요청
        // UIManager.Instance.ShowRepairWindow(repairAmount);
    }
}