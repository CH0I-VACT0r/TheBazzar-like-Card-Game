using UnityEngine;
using UnityEngine.UIElements;

public class EventInteractionManager : MonoBehaviour
{
    public static EventInteractionManager Instance;

    private GameEvent _currentEvent; // 현재 진행 중인 이벤트 (수리? 강화?)
    private Card _selectedCard;      // 슬롯에 올라간 카드

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. 상호작용 시작 (이벤트가 호출함)
    public void StartInteraction(GameEvent evtData)
    {
        _currentEvent = evtData;
        _selectedCard = null;

        // UI 열기 요청
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventInteractionUI(evtData);
        }
    }

    // 2. 카드 선택 (드래그 핸들러가 호출함)
    public void SelectCard(Card card)
    {
        if (_currentEvent == null) return;

        // 유효성 검사
        string reason;
        if (!_currentEvent.IsValidCard(card, out reason))
        {
            Debug.LogWarning($"[Event] 카드 선택 불가: {reason}");
            // TODO: UI에 경고 메시지 띄우기 (Toast Message 등)
            return;
        }

        _selectedCard = card;
        Debug.Log($"[Event] 카드 선택됨: {card.CardNameKey}");

        // UI 갱신 (슬롯에 카드 보여주기, 버튼 활성화)
        UIManager.Instance.UpdateInteractionSlot(_selectedCard);
    }

    // 3. 액션 실행 (버튼 클릭 시 UIManager가 호출)
    public void OnActionButtonClick()
    {
        if (_selectedCard == null || _currentEvent == null) return;

        // 효과 적용
        _currentEvent.ApplyEffect(_selectedCard);

        // 결과 처리 (종료)
        CloseInteraction();
    }

    // 4. 나가기
    public void CloseInteraction()
    {
        _selectedCard = null;
        _currentEvent = null;

        // 몬스터 화면으로 복귀
        UIManager.Instance.SwitchToBattlePage();
    }
}
