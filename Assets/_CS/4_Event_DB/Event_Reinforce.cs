using UnityEngine;

[CreateAssetMenu(fileName = "New Reinforce Event", menuName = "Game/Events/Reinforce")]
public class Event_Reinforce : GameEvent
{
    [Header("보강 설정")]
    public int addAmount = 3; // 내구도 추가량

    // 1. 이벤트 시작 -> UI 열기
    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] {eventID} 시작: 대장간 UI 오픈");
        if (EventInteractionManager.Instance != null)
        {
            EventInteractionManager.Instance.StartInteraction(this);
        }
    }

    // 카드 유효성 검사
    public override bool IsValidCard(Card card, out string failReason)
    {
        if (card.Durability == -1)
        {
            failReason = "Cannot reinforce infinite durability.";
            return false;
        }

        failReason = "";
        return true;
    }

    // 실제 효과 적용
    public override void ApplyEffect(Card card)
    {
        if (card != null)
        {
            card.IncreaseDurability(addAmount); // Card.cs에 만든 함수 호출
            Debug.Log($"[Event] {card.CardNameKey} 내구도 {addAmount} 증가 완료!");
        }
    }
}