using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Event", menuName = "Game/Event/Crafting")]
public class Event_Crafting : GameEvent
{
    [Header("제작 이벤트 설정")]
    // 만약 특정 재료만 받는 제작대라면 여기에 태그를 추가할 수 있음 (예: "tag_ore")
    // 비워두면 모든 "재료(Material)" 타입 허용
    public string requiredTagFilter = "";

    // 제작 이벤트가 실행되면 호출되는 함수
    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] {titleKey} 시작 -> 제작 UI 오픈");

        // 제작 매니저에게 UI를 열라고 지시
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OpenCraftingUI(this);
        }
        else
        {
            Debug.LogError("CraftingManager 인스턴스가 없습니다!");
        }
    }

    // 카드 유효성 검사 (재료 카드만 허용 + 태그 필터)
    public override bool IsValidCard(Card card, out string failReason)
    {
        // 재료 타입인지 확인
        if (card.ItemType != CardType.Material)
        {
            failReason = "Only materials can be used for crafting."; // "재료 카드만 사용할 수 있습니다."
            return false;
        }

        // 추가 필터: 특정 태그가 필요한 경우 (예: 대장간 모루에는 광석만)
        if (!string.IsNullOrEmpty(requiredTagFilter))
        {
            if (!card.HasTagKey(requiredTagFilter))
            {
                failReason = "This material cannot be used here.";
                return false;
            }
        }

        failReason = "";
        return true;
    }
}