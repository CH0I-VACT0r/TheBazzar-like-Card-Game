using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropHandler : PointerManipulator
{
    private bool m_IsDragging = false;
    private VisualElement m_Root;
    private VisualElement m_GhostIcon;

    public int StartSlotIndex { get; private set; } = -1;
    public bool IsFromInventory { get; private set; } = false;
    private bool m_IsFromInteractionSlot = false;

    // [신규] 제작 슬롯 여부 확인용 변수
    private bool m_IsCraftingInput = false;
    private bool m_IsCraftingResult = false;

    private object m_OwnerController; // PlayerController 등
    private Vector2 m_PointerOffset;

    // 드래그 중인 카드를 임시 저장
    private Card m_HeldCard = null;

    public DragAndDropHandler(VisualElement target, VisualElement root, object controller, bool isInteractionSlot = false)
    {
        this.target = target;
        this.m_Root = root;
        this.m_OwnerController = controller;
        this.m_IsFromInteractionSlot = isInteractionSlot;

        // [신규] 제작 슬롯 여부 판별
        if (target.name != null)
        {
            m_IsCraftingInput = target.name.StartsWith("CraftInput");
            m_IsCraftingResult = target.name == "ResultIcon";
        }

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
        if (m_IsDragging || m_Root == null) return;

        Vector2 pointerPos = evt.position;
        Card card = null;

        // 제작 결과물 슬롯 (완성품 수령)
        if (m_IsCraftingResult)
        {
            if (CraftingManager.Instance != null)
            {
                card = CraftingManager.Instance.ClaimResultCard();
            }
        }
        // 제작 재료 슬롯 (재료 빼기)
        else if (m_IsCraftingInput)
        {
            int slotIndex = ParseSlotIndex(target.name);
            if (CraftingManager.Instance != null)
            {
                card = CraftingManager.Instance.TryRemoveCardFromSlot(slotIndex);
            }
        }
        // 이벤트 상호작용 슬롯 (대장간 등에서 카드 빼기)
        else if (m_IsFromInteractionSlot)
        {
            if (EventInteractionManager.Instance != null)
            {
                // 기존엔 TakeCardOut() 같은 걸 썼지만, 여기선 HeldCard를 참조
                card = EventInteractionManager.Instance.HeldCard;

                // 드래그 시작 시 슬롯에서 빼는 처리 (화면 갱신 포함)
                if (card != null)
                    EventInteractionManager.Instance.TakeCardOut();
            }
        }
        // 일반 인벤토리/장착 슬롯
        else
        {
            if (m_OwnerController is PlayerController playerOwner)
            {
                if (!playerOwner.GetBattleManager().IsDeckEditingAllowed) return;

                string slotName = target.name;
                IsFromInventory = slotName.StartsWith("InvSlot");
                StartSlotIndex = ParseSlotIndex(slotName);

                if (IsFromInventory)
                {
                    VisualElement img = target.Q<VisualElement>("CardImage");
                    if (img != null && img.userData is Card c) card = c;
                }
                else
                {
                    card = playerOwner.GetCardAtIndex(StartSlotIndex);
                }

                if (card != null) playerOwner.ClearTooltipScheduler();
            }
        }

        // 카드가 없으면 드래그 시작 안 함
        if (card == null) return;

        // 고스트 아이콘 생성
        CreateGhostIcon(card, pointerPos);

        m_IsDragging = true;
        target.CapturePointer(evt.pointerId);
        target.style.opacity = 0.3f;
        evt.StopPropagation();
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        if (m_GhostIcon != null)
        {
            Vector2 localPos = m_Root.WorldToLocal(evt.position);
            m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
            m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);
        target.style.opacity = 1f;

        // 고스트 제거
        if (m_GhostIcon != null)
        {
            if (m_Root.Contains(m_GhostIcon)) m_Root.Remove(m_GhostIcon);
            m_GhostIcon = null;
        }

        // 드롭 위치 판별
        VisualElement dropTarget = m_Root.panel.Pick(evt.position);

        // -------------------------------------------------------------
        // 1. 제작 슬롯에 드롭했는지 확인
        // -------------------------------------------------------------
        VisualElement craftingSlot = FindCraftingInputSlot(dropTarget);
        if (craftingSlot != null)
        {
            HandleDropOnCrafting(craftingSlot);
            evt.StopPropagation();
            return;
        }

        // -------------------------------------------------------------
        // 2. 이벤트 상호작용 슬롯(TargetSlot)에 드롭했는지 확인
        // -------------------------------------------------------------
        VisualElement interactionSlot = FindInteractionSlot(dropTarget);
        if (interactionSlot != null)
        {
            // 상호작용 슬롯에서 나온 카드를 다시 상호작용 슬롯에 놓는 건 무의미하므로 패스하거나 원복
            if (m_IsFromInteractionSlot)
            {
                // 다시 제자리로 (취소)
                if (m_HeldCard != null && EventInteractionManager.Instance != null)
                    EventInteractionManager.Instance.PlaceCard(m_HeldCard);
            }
            else
            {
                // 인벤/장비창 -> 상호작용 슬롯
                if (m_HeldCard != null && EventInteractionManager.Instance != null)
                {
                    // 인벤토리/장비창에서 실제 데이터 제거 (소유권 이전)
                    RemoveCardFromSource();
                    // 이벤트 매니저에 카드 등록
                    EventInteractionManager.Instance.PlaceCard(m_HeldCard);
                    m_HeldCard = null; // 처리 완료
                }
            }
            UIManager.Instance.RefreshPlayerUI();
            UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);
            evt.StopPropagation();
            return;
        }

        // -------------------------------------------------------------
        // 3. 판매존, 인벤토리, 장착 슬롯 처리
        // -------------------------------------------------------------
        if (m_OwnerController is PlayerController playerOwner)
        {
            // 드래그 중인 카드 확인
            if (m_HeldCard == null) return;

            // 3-1. 판매존
            if (IsSellZone(dropTarget))
            {
                // 제작/이벤트 슬롯에서 바로 판매는 일단 금지 (실수 방지)
                if (!m_IsCraftingInput && !m_IsCraftingResult && !m_IsFromInteractionSlot)
                {
                    playerOwner.SellCard(StartSlotIndex, IsFromInventory);
                }
                else
                {
                    // 복구 로직 (원래 자리로)
                    ReturnCardToSource();
                }
                m_HeldCard = null;
                evt.StopPropagation();
                return;
            }

            // 3-2. 인벤토리/장착 슬롯 드롭
            VisualElement droppedSlot = FindParentSlot(dropTarget);
            if (droppedSlot != null)
            {
                // 인벤/장비 -> 인벤/장비 이동의 경우 (기존 로직 사용을 위해 m_HeldCard 반환 대신 로직 수행)

                // 특수 슬롯(제작, 이벤트)에서 온 경우 -> 무조건 '새로 획득' 처리
                if (m_IsCraftingInput || m_IsCraftingResult || m_IsFromInteractionSlot)
                {
                    bool isToInventory = droppedSlot.name.StartsWith("InvSlot");
                    int dropIndex = ParseSlotIndex(droppedSlot.name);

                    if (isToInventory)
                    {
                        InventoryManager.Instance.AddCardObject(m_HeldCard);
                    }
                    else if (dropIndex != -1)
                    {
                        // 장착 슬롯에 바로 드롭
                        playerOwner.EquipCardDirectly(m_HeldCard, dropIndex);
                    }
                    m_HeldCard = null; // 처리 완료
                }
                else
                {
                    // 인벤 <-> 장비 이동 (기존 로직 유지)
                    HandleStandardSwap(droppedSlot, playerOwner);
                }
            }
            // 3-3. 허공에 드롭 (인벤토리로 복귀 or 원래 자리로 복귀)
            else
            {
                ReturnCardToSource();
            }
        }

        // UI 갱신
        UIManager.Instance.RefreshPlayerUI();
        UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);

        evt.StopPropagation();
    }

    // --- 헬퍼 함수 ---

    // 드래그 시작 시점의 위치에서 카드를 제거하는 함수 (이동 확정 시 호출)
    private void RemoveCardFromSource()
    {
        if (m_IsCraftingInput || m_IsCraftingResult || m_IsFromInteractionSlot)
        {
            // 이미 PointerDown에서 제거되었으므로 추가 동작 불필요
            return;
        }

        // 인벤토리/장비창에서 제거
        if (m_OwnerController is PlayerController player)
        {
            if (IsFromInventory)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.RemoveCard(m_HeldCard);
            }
            else
            {
                // 장비 해제 (빈 카드로 교체)
                player.ExtractCard(StartSlotIndex);
            }
        }
    }

    // 드래그 취소 시 원래 자리로 되돌리는 함수
    private void ReturnCardToSource()
    {
        if (m_HeldCard == null) return;

        // 1. 제작 슬롯
        if (m_IsCraftingInput)
        {
            int slotIndex = ParseSlotIndex(target.name);
            CraftingManager.Instance?.TryDropCardOnSlot(slotIndex, m_HeldCard);
        }
        // 2. 제작 결과물 (취소하면 인벤토리로 들어감)
        else if (m_IsCraftingResult)
        {
            InventoryManager.Instance?.AddCardObject(m_HeldCard);
        }
        // 3. 이벤트 슬롯
        else if (m_IsFromInteractionSlot)
        {
            EventInteractionManager.Instance?.PlaceCard(m_HeldCard);
        }
        // 4. 인벤토리/장비창
        else
        {
            // 허공에 드롭 시 인벤토리로 돌아가는 기능
            if (InventoryManager.Instance != null) InventoryManager.Instance.AddCardObject(m_HeldCard);
        }
    }

    private void HandleDropOnCrafting(VisualElement slot)
    {
        if (m_HeldCard == null) return;

        // 소스에서 제거
        RemoveCardFromSource();

        // 제작 슬롯에 투입
        int slotIndex = ParseSlotIndex(slot.name);
        if (CraftingManager.Instance != null)
        {
            if (CraftingManager.Instance.TryDropCardOnSlot(slotIndex, m_HeldCard))
            {
                m_HeldCard = null; // 성공적으로 들어감
            }
            else
            {
                // 실패 시 (이미 꽉 찼거나 등) -> 인벤토리로
                InventoryManager.Instance?.AddCardObject(m_HeldCard);
            }
        }
    }

    private void HandleStandardSwap(VisualElement dropSlot, PlayerController player)
    {
        // 인벤 <-> 장비, 장비 <-> 장비 교환 로직
        bool isToInventory = dropSlot.name.StartsWith("InvSlot");
        int dropIndex = ParseSlotIndex(dropSlot.name);

        if (dropIndex == -1) return;

        if (IsFromInventory && !isToInventory)
            player.EquipCard(StartSlotIndex, dropIndex);
        else if (!IsFromInventory && isToInventory)
            player.UnequipCard(StartSlotIndex);
        else if (!IsFromInventory && !isToInventory && StartSlotIndex != dropIndex)
            player.MoveCard(StartSlotIndex, dropIndex);
        else if (IsFromInventory && isToInventory)
        {
            // 인벤 -> 인벤 (순서 변경은 추후 구현)
        }
    }

    private void CreateGhostIcon(Card card, Vector2 mousePosition)
    {
        m_HeldCard = card; // [중요] 카드 저장

        m_GhostIcon = new VisualElement();
        if (card != null && card.CardImage != null)
        {
            m_GhostIcon.style.backgroundImage = new StyleBackground(card.CardImage);
        }
        m_GhostIcon.style.width = target.resolvedStyle.width;
        m_GhostIcon.style.height = target.resolvedStyle.height;
        m_GhostIcon.style.position = Position.Absolute;

        m_PointerOffset = new Vector2(target.resolvedStyle.width / 2, target.resolvedStyle.height / 2);
        Vector2 localPos = m_Root.WorldToLocal(mousePosition);
        m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
        m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;
        m_GhostIcon.style.opacity = 0.7f;
        m_GhostIcon.pickingMode = PickingMode.Ignore;

        m_Root.Add(m_GhostIcon);
    }

    // --- 유틸리티 ---

    private int ParseSlotIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;
        var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int index)) return index;
        return -1;
    }

    private VisualElement FindParentSlot(VisualElement element)
    {
        while (element != null)
        {
            if (element.name != null && (element.name.StartsWith("InvSlot") || element.name.StartsWith("CardSlot")))
                return element;
            element = element.parent;
        }
        return null;
    }

    private VisualElement FindCraftingInputSlot(VisualElement element)
    {
        while (element != null)
        {
            if (element.name != null && element.name.StartsWith("CraftInput")) return element;
            element = element.parent;
        }
        return null;
    }

    // [복구] 상호작용 슬롯 찾기
    private VisualElement FindInteractionSlot(VisualElement element)
    {
        while (element != null)
        {
            // UIManager에서 "TargetSlot"이라고 이름 지었음
            if (element.name == "TargetSlot") return element;
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