using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropHandler : PointerManipulator
{
    private bool m_IsDragging = false;
    private VisualElement m_Root; // 전체 화면 루트
    private VisualElement m_GhostIcon; // 마우스 따라다닐 가짜 아이콘

    public int StartSlotIndex { get; private set; } = -1;
    public bool IsFromInventory { get; private set; } = false;
    private object m_OwnerController;
    private Vector2 m_PointerOffset;  // 드래그 시작 시점의 오프셋

    public DragAndDropHandler(VisualElement target, VisualElement root, object controller)
    {
        this.target = target;
        m_Root = root;
        m_OwnerController = controller;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (m_OwnerController is PlayerController playerOwner)
        {
            // 잠금 확인
            if (!playerOwner.GetBattleManager().IsDeckEditingAllowed) return;
            if (m_IsDragging || m_Root == null) return;

            // 어디서 시작했는지 확인 (인벤토리 or 파티)
            string slotName = target.name;
            IsFromInventory = slotName.StartsWith("InvSlot"); // 이름 규칙: InvSlot_X

            // 인덱스 파싱 (이름 끝자리 숫자 가져오기)
            StartSlotIndex = ParseSlotIndex(slotName);
            if (StartSlotIndex == -1) return;

            // 데이터(Card) 가져오기
            Card card = null;
            if (IsFromInventory)
            {
                // 인벤토리에서 가져오기
                // 현재 보고 있는 탭의 리스트에서 가져와야 함
                // 지금은 UI 슬롯에 심어둔 userData를 쓰는 게 가장 확실함
                VisualElement img = target.Q<VisualElement>("CardImage");
                if (img != null && img.userData is Card c) card = c;
            }
            else
            {
                // 파티 슬롯에서 가져오기
                card = playerOwner.GetCardAtIndex(StartSlotIndex);
            }

            if (card == null) return; // 빈 슬롯이면 무시

            // 4. 드래그 시작
            m_IsDragging = true;
            target.CapturePointer(evt.pointerId);
            playerOwner.ClearTooltipScheduler(); // 툴팁 끄기

            CreateGhostIcon(card, evt.position);

            target.style.opacity = 0.3f; // 원본 흐리게
            evt.StopPropagation();
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        // 고스트 아이콘 이동
        if (m_GhostIcon != null)
        {
            // 마우스 위치(World)를 Root 기준 로컬 좌표로 변환
            Vector2 mousePos = evt.position; // 명시적 Vector2
            Vector2 localPos = m_Root.WorldToLocal(mousePos);

            // 오프셋 적용하여 위치 설정
            m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
            m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);
        target.style.opacity = 1f; // 원본 복구

        // 고스트 삭제
        if (m_GhostIcon != null)
        {
            if (m_Root.Contains(m_GhostIcon)) m_Root.Remove(m_GhostIcon);
            m_GhostIcon = null;
        }

        if (m_OwnerController is PlayerController playerOwner)
        {
            // 드롭한 위치 찾기
            VisualElement dropTarget = m_Root.panel.Pick(evt.position);

            // 판매 존 확인
            if (IsSellZone(dropTarget))
            {
                Debug.Log("[Drop] 판매 존에 드롭!");
                playerOwner.SellCard(StartSlotIndex, IsFromInventory); // 판매 함수 호출
                return;
            }

            // 슬롯 확인 (파티 or 인벤토리)
            VisualElement droppedSlot = FindParentSlot(dropTarget);

            if (droppedSlot != null)
            {
                bool isToInventory = droppedSlot.name.StartsWith("InvSlot");
                int dropIndex = ParseSlotIndex(droppedSlot.name);

                if (dropIndex == -1) return;

                // --- 경우의 수 처리 ---

                // 1. 인벤토리 -> 파티 (장착)
                if (IsFromInventory && !isToInventory)
                {
                    Debug.Log($"[Drop] 장착: 인벤({StartSlotIndex}) -> 파티({dropIndex})");
                    playerOwner.EquipCard(StartSlotIndex, dropIndex);
                }
                // 2. 파티 -> 인벤토리 (해제)
                else if (!IsFromInventory && isToInventory)
                {
                    Debug.Log($"[Drop] 해제: 파티({StartSlotIndex}) -> 인벤({dropIndex})");
                    playerOwner.UnequipCard(StartSlotIndex);
                }
                // 3. 파티 -> 파티 (자리 교체)
                else if (!IsFromInventory && !isToInventory)
                {
                    if (StartSlotIndex != dropIndex)
                    {
                        Debug.Log($"[Drop] 이동: 파티({StartSlotIndex}) -> 파티({dropIndex})");
                        playerOwner.MoveCard(StartSlotIndex, dropIndex);
                    }
                }
                // 4. 인벤토리 -> 인벤토리 (자리 교체 - 나중에 구현)
                else if (IsFromInventory && isToInventory)
                {
                    Debug.Log("[Drop] 인벤토리 내 이동 (아직 미구현)");
                }
            }
            else
            {
                Debug.Log("[Drop] 허공에 드롭 (취소)");
            }
        }
        evt.StopPropagation();
    }

    // 고스트 아이콘 생성 함수
    private void CreateGhostIcon(Card card, Vector2 mousePosition)
    {
        m_GhostIcon = new VisualElement();

        // 스타일 복사 (카드 이미지처럼 보이게)
        if (card.CardImage != null)
        {
            m_GhostIcon.style.backgroundImage = new StyleBackground(card.CardImage);
        }

        // 크기 설정 (원본 슬롯 크기 따라감)
        m_GhostIcon.style.width = target.resolvedStyle.width;
        m_GhostIcon.style.height = target.resolvedStyle.height;
        m_GhostIcon.style.position = Position.Absolute;

        // 마우스 커서가 카드의 중앙에 오도록 오프셋 설정
        m_PointerOffset = new Vector2(target.resolvedStyle.width / 2, target.resolvedStyle.height / 2);

        // 초기 위치 설정
        Vector2 localPos = m_Root.WorldToLocal(mousePosition);
        m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
        m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;

        // 터치 무시 (드롭 시 밑에 있는 슬롯을 감지해야 하므로 필수)
        m_GhostIcon.pickingMode = PickingMode.Ignore;

        m_Root.Add(m_GhostIcon);
    }

    private int ParseSlotIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;

        string numberPart = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;

        if (int.TryParse(numberPart, out int index))
        {
            return index;
        }

        return -1;
    }

    private VisualElement FindParentSlot(VisualElement element)
    {
        while (element != null)
        {
            // 슬롯 이름 규칙: InvSlot_X 또는 CardSlot_X
            if (element.name != null && (element.name.StartsWith("InvSlot") || element.name.StartsWith("CardSlot")))
            {
                return element;
            }
            element = element.parent;
        }
        return null;
    }

    private bool IsSellZone(VisualElement element)
    {
        while (element != null)
        {
            if (element.name == "SellZone") return true;
            element = element.parent;
        }
        return false;
    }
}