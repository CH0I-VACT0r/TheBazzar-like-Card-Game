using UnityEngine;

public enum StatType { Attack, Health, Shield }

[CreateAssetMenu(fileName = "New Buff Event", menuName = "Game/Events/Stat Buff")]
public class Event_StatBuff : GameEvent
{
    [Header("강화 설정")]
    public StatType targetStat; // 무엇을 올릴지
    public int increaseAmount;  // 얼마나 올릴지 (브론즈:10, 실버:20...)

    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] '{rarity}' 강화 이벤트 발생! ({targetStat} +{increaseAmount})");

        // TODO: UIManager에게 '강화 창'을 띄우라고 요청
        // UIManager.Instance.ShowTrainingWindow(targetStat, increaseAmount);
    }
}
