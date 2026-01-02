using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Event", menuName = "Events/Quest Event")]
public class Event_Quest : Event_Battle
{
    private void OnEnable()
    {
        eventType = EventType.Quest; // 에셋 생성 시 타입을 Quest로 고정
    }

    public override void Execute(PlayerController player)
    {
        // 부모(Event_Battle)의 실행 로직을 그대로 사용 (전투 화면 전환)
        base.Execute(player);

        // 이 전투가 '의뢰'임을 BattleManager에게 알림
        BattleManager.Instance.IsQuestBattle = true;
    }
}
