using UnityEngine;
using UnityEngine.UIElements;

public class EventInteractionManager : MonoBehaviour
{
    public static EventInteractionManager Instance;

    private GameEvent _currentEvent;

    // 현재 슬롯에 보관 중인 카드
    public Card HeldCard { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartInteraction(GameEvent evtData)
    {
        // 안전장치: 혹시 이전에 남은 카드가 있다면 돌려주고 시작
        if (HeldCard != null) ReturnHeldCard();

        _currentEvent = evtData;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowEventInteractionUI(evtData);
    }

    // 1. 카드 등록 (드래그 핸들러가 호출)
    public void PlaceCard(Card card)
    {
        if (_currentEvent == null) return;

        // [중요] 이미 슬롯에 카드가 있다면? -> 플레이어에게 돌려줌 (데이터 반환)
        if (HeldCard != null) ReturnHeldCard();

        // 유효성 검사
        string reason;
        if (!_currentEvent.IsValidCard(card, out reason))
        {
            Debug.LogWarning($"[Event] 조건 불만족: {reason}");

            // PlayerController 찾기 (BattleManager 경유)
            PlayerController pc = null;
            if (BattleManager.Instance != null) pc = BattleManager.Instance.playerController;

            if (pc != null) pc.ReturnCardToBestSlot(card); // 넣으려던 카드 반환
            UIManager.Instance.UpdateInteractionSlot(null);

            return;
        }

        // 보관 시작
        HeldCard = card;
        Debug.Log($"[Event] {card.CardNameKey} 보관 중.");

        // UI 갱신 (성공 시)
        UIManager.Instance.UpdateInteractionSlot(HeldCard);
    }

    // [신규] 카드 교체 (교환 이벤트용)
    public void ReplaceHeldCard(Card newCard)
    {
        HeldCard = newCard;
        UIManager.Instance.UpdateInteractionSlot(HeldCard);
        Debug.Log($"[Event] 슬롯의 카드가 {newCard.CardNameKey}(으)로 교체되었습니다.");
    }

    // 2. 카드 꺼내기
    public Card TakeCardOut()
    {
        if (HeldCard == null)
        {
            Debug.LogWarning("[EventInteractionManager] 꺼낼 카드가 없습니다 (HeldCard is null).");
            return null;
        }

        Card card = HeldCard;
        HeldCard = null;

        Debug.Log($"[Event] {card.CardNameKey} 카드를 이벤트 슬롯에서 꺼냈습니다."); // 로그 추가

        // UI 갱신 (빈 슬롯으로)
        UIManager.Instance.UpdateInteractionSlot(null);

        return card;
    }

    // 3. 액션 실행 (버튼 클릭)
    public void OnActionButtonClick()
    {
        if (HeldCard == null || _currentEvent == null) return;

        // 효과 적용
        _currentEvent.ApplyEffect(HeldCard);

        // 효과 적용 후 UI 갱신
        UIManager.Instance.UpdateInteractionSlot(HeldCard);
    }

    // 4. 나가기
    public void CloseInteraction()
    {
        if (HeldCard != null)
        {
            ReturnHeldCard();
        }

        _currentEvent = null;
        UIManager.Instance.SwitchToBattlePage();
    }

    // 내부용: 플레이어에게 카드 돌려주기
    private void ReturnHeldCard()
    {
        if (HeldCard == null) return;

        PlayerController pc = null;
        if (BattleManager.Instance != null) pc = BattleManager.Instance.playerController;

        if (pc != null)
        {
            pc.ReturnCardToBestSlot(HeldCard);
        }
        else
        {
            Debug.LogError("[Event] PlayerController를 찾을 수 없어 카드를 돌려줄 수 없습니다!");
        }
        HeldCard = null;
    }
}