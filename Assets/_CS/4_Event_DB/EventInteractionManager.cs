using UnityEngine;

public class EventInteractionManager : MonoBehaviour
{
    public static EventInteractionManager Instance;

    private GameEvent _currentEvent;

    // 현재 슬롯에 보관 중인 카드
    public Card HeldCard { get; private set; }

    // [신규] 현재 '이벤트'에서 행동을 이미 수행했는지 여부 (카드 교체해도 유지)
    private bool _isActionUsed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartInteraction(GameEvent evtData)
    {
        // 안전장치: 혹시 이전에 남은 카드가 있다면 돌려주고 시작
        if (HeldCard != null) ReturnHeldCard();

        _currentEvent = evtData;

        // [중요] 이벤트 시작 시에만 '행동 가능' 상태로 초기화
        _isActionUsed = false;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowEventInteractionUI(evtData);
    }

    // 1. 카드 등록
    public void PlaceCard(Card card)
    {
        if (_currentEvent == null) return;

        if (HeldCard != null) ReturnHeldCard();

        // 유효성 검사
        string reason;
        if (!_currentEvent.IsValidCard(card, out reason))
        {
            Debug.LogWarning($"[Event] 조건 불만족: {reason}");

            PlayerController pc = null;
            if (BattleManager.Instance != null) pc = BattleManager.Instance.playerController;

            if (pc != null) pc.ReturnCardToBestSlot(card);

            // UI 초기화 (실패 시 잔상 제거)
            UIManager.Instance.UpdateInteractionSlot(null);
            return;
        }

        // 보관 시작
        HeldCard = card;
        Debug.Log($"[Event] {card.CardNameKey} 보관 중.");

        // UI 갱신 
        // [중요] 이미 행동을 했다면(_isActionUsed), 카드는 보여주되 버튼은 끈 상태(false)로 갱신
        bool canUseButton = !_isActionUsed;
        UIManager.Instance.UpdateInteractionSlot(HeldCard, canUseButton);
    }

    // 카드 교체 (제작/교환 이벤트용)
    public void ReplaceHeldCard(Card newCard)
    {
        HeldCard = newCard;
        // 교체 후에도 버튼 상태는 유지 (이미 제작했으니 비활성화)
        UIManager.Instance.UpdateInteractionSlot(HeldCard, !_isActionUsed);
        Debug.Log($"[Event] 슬롯의 카드가 {newCard.CardNameKey}(으)로 교체되었습니다.");
    }

    // 2. 카드 꺼내기
    public Card TakeCardOut()
    {
        if (HeldCard == null)
        {
            return null;
        }

        Card card = HeldCard;
        HeldCard = null;

        // [중요] 카드를 꺼내도 _isActionUsed는 초기화하지 않음 (방문당 1회 제한)

        // UI 갱신 (빈 슬롯)
        UIManager.Instance.UpdateInteractionSlot(null);

        return card;
    }

    // 3. 액션 실행 (버튼 클릭)
    public void OnActionButtonClick()
    {
        if (HeldCard == null || _currentEvent == null) return;

        // [중요] 중복 실행 방지
        if (_isActionUsed) return;

        // 1. 효과 적용
        _currentEvent.ApplyEffect(HeldCard);

        // 2. 사용 완료 플래그 세팅 (이후 카드를 바꿔도 버튼은 계속 비활성화)
        _isActionUsed = true;

        // 3. UI 갱신 (버튼 끄기)
        UIManager.Instance.UpdateInteractionSlot(HeldCard, false);

        Debug.Log("[Event] 액션 완료. 버튼 비활성화.");
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