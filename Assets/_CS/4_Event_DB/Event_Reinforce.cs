using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Reinforce Event", menuName = "Game/Events/Reinforce")]
public class Event_Reinforce : GameEvent
{
    [Header("보강 설정")]
    public int addAmount = 3; // 내구도 추가량

    [Header("필터 (비워두면 모든 카드 허용)")]
    // 예: "tag_weapon", "tag_armor" -> 대장간용
    // 예: "tag_book" -> 서점용
    public List<string> requiredTags = new List<string>();

    // 1. 이벤트 시작 -> UI 열기
    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] {eventID} 시작: 보강(수리) UI 오픈");

        // EventInteractionManager에게 요청
        if (EventInteractionManager.Instance != null)
        {
            EventInteractionManager.Instance.StartInteraction(this);
        }
    }

    // 2. 카드 유효성 검사 (태그 + 내구도)
    public override bool IsValidCard(Card card, out string failReason)
    {
        // A. 내구도 무한 체크
        if (card.Durability == -1)
        {
            failReason = "Infinite durability items cannot be reinforced.";
            return false;
        }

        // B. 태그 체크 (리스트가 비어있으면 통과)
        if (requiredTags.Count > 0)
        {
            bool hasTag = false;
            foreach (string tag in requiredTags)
            {
                if (card.HasTagKey(tag))
                {
                    hasTag = true;
                    break;
                }
            }

            if (!hasTag)
            {
                failReason = "This shop does not accept this type of item.";
                return false;
            }
        }

        failReason = "";
        return true;
    }

    // 3. 실제 효과 적용
    public override void ApplyEffect(Card card)
    {
        if (card != null)
        {
            card.IncreaseDurability(addAmount); // Card.cs의 함수 호출
        }
    }
}