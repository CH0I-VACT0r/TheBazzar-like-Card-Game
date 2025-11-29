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

    private object m_OwnerController;
    private Vector2 m_PointerOffset;

    public DragAndDropHandler(VisualElement target, VisualElement root, object controller, bool isInteractionSlot = false)
    {
        this.target = target;
        this.m_Root = root;
        this.m_OwnerController = controller;
        this.m_IsFromInteractionSlot = isInteractionSlot;

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
        // [디버그] 함수 진입 확인 (PickingMode 문제인지 확인용)
        // 이 로그조차 안 뜨면 UI 요소(TargetSlot의 자식들)가 가리고 있는 것임
        // Debug.Log($"[Drag] PointerDown 시도! Target: {target.name}, IsInteraction: {m_IsFromInteractionSlot}");

        if (m_OwnerController == null)
        {
            Debug.LogError($"[Drag] m_OwnerController가 null입니다! Target: {target.name}");
            return;
        }

        if (m_OwnerController is PlayerController playerOwner)
        {
            // 덱 편집 권한 확인
            if (!playerOwner.GetBattleManager().IsDeckEditingAllowed)
            {
                Debug.Log($"[Drag] 덱 편집 잠금 상태입니다. (IsDeckEditingAllowed: false)");
                return;
            }

            if (m_IsDragging || m_Root == null) return;

            Debug.Log($"[Drag] 조건 통과. 드래그 시작 로직 진입. Target: {target.name}");

            Vector2 pointerPos = evt.position;

            if (m_IsFromInteractionSlot)
            {
                // 대장간 슬롯에서 시작
                if (EventInteractionManager.Instance == null)
                {
                    Debug.LogWarning("[Drag] EventInteractionManager Instance가 null입니다.");
                    return;
                }

                if (EventInteractionManager.Instance.HeldCard == null)
                {
                    Debug.LogWarning("[Drag] HeldCard가 없습니다 (빈 슬롯 클릭함).");
                    return;
                }

                CreateGhostIcon(EventInteractionManager.Instance.HeldCard, pointerPos);
            }
            else
            {
                // 일반 슬롯에서 시작
                string slotName = target.name;
                IsFromInventory = slotName.StartsWith("InvSlot");
                StartSlotIndex = ParseSlotIndex(slotName);
                if (StartSlotIndex == -1)
                {
                    Debug.LogWarning($"[Drag] 슬롯 인덱스 파싱 실패: {slotName}");
                    return;
                }

                Card card = null;
                if (IsFromInventory)
                {
                    VisualElement img = target.Q<VisualElement>("CardImage");
                    if (img != null && img.userData is Card c) card = c;
                }
                else
                {
                    card = playerOwner.GetCardAtIndex(StartSlotIndex);
                }

                if (card == null)
                {
                    // 빈 슬롯 클릭 시 무시
                    return;
                }

                playerOwner.ClearTooltipScheduler();
                CreateGhostIcon(card, pointerPos);
            }

            m_IsDragging = true;
            target.CapturePointer(evt.pointerId);
            target.style.opacity = 0.3f;
            evt.StopPropagation();
        }
        else
        {
            Debug.LogError($"[Drag] OwnerController 타입 불일치! 예상: PlayerController, 실제: {m_OwnerController.GetType()}");
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        if (m_GhostIcon != null)
        {
            // [수정] 월드 좌표(evt.position)를 Root 기준 로컬 좌표로 변환하여 배치
            Vector2 localPos = m_Root.WorldToLocal(evt.position);

            m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
            m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        Debug.Log("[Drag] PointerUp (드롭 시도)");

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);
        target.style.opacity = 1f;

        if (m_GhostIcon != null)
        {
            if (m_Root.Contains(m_GhostIcon)) m_Root.Remove(m_GhostIcon);
            m_GhostIcon = null;
        }

        if (m_OwnerController is PlayerController playerOwner)
        {
            VisualElement dropTarget = m_Root.panel.Pick(evt.position);

            if (dropTarget != null) Debug.Log($"[Drag] Dropped on: {dropTarget.name}");
            else Debug.Log("[Drag] Dropped on null");

            VisualElement interactionSlot = FindInteractionSlot(dropTarget);

            if (interactionSlot != null)
            {
                Debug.Log("[Drop] -> TargetSlot 감지됨.");

                if (m_IsFromInteractionSlot)
                {
                    evt.StopPropagation();
                    return;
                }

                Card cardToPlace = null;

                if (IsFromInventory)
                {
                    if (InventoryManager.Instance != null)
                    {
                        CardType type = UIManager.Instance.CurrentTab;
                        cardToPlace = InventoryManager.Instance.GetCardAtIndex(type, StartSlotIndex);
                        if (cardToPlace != null) InventoryManager.Instance.RemoveCard(cardToPlace);
                    }
                }
                else
                {
                    cardToPlace = playerOwner.ExtractCard(StartSlotIndex);
                }

                if (cardToPlace != null && EventInteractionManager.Instance != null)
                {
                    EventInteractionManager.Instance.PlaceCard(cardToPlace);
                }

                UIManager.Instance.RefreshPlayerUI();
                if (UIManager.Instance.IsInventoryOpen) UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);

                evt.StopPropagation();
                return;
            }

            if (IsSellZone(dropTarget))
            {
                if (m_IsFromInteractionSlot)
                {
                    Debug.Log("[Drop] 이벤트 슬롯에서 바로 판매는 불가능합니다.");
                }
                else
                {
                    playerOwner.SellCard(StartSlotIndex, IsFromInventory);
                }
                evt.StopPropagation();
                return;
            }

            VisualElement droppedSlot = FindParentSlot(dropTarget);

            if (droppedSlot != null)
            {
                bool isToInventory = droppedSlot.name.StartsWith("InvSlot");
                int dropIndex = ParseSlotIndex(droppedSlot.name);

                if (dropIndex != -1)
                {
                    if (m_IsFromInteractionSlot)
                    {
                        if (EventInteractionManager.Instance != null)
                        {
                            Card cardRetrieved = EventInteractionManager.Instance.TakeCardOut();
                            if (cardRetrieved != null)
                            {
                                if (isToInventory)
                                {
                                    if (InventoryManager.Instance != null)
                                    {
                                        InventoryManager.Instance.AddCardObject(cardRetrieved);
                                        UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);
                                    }
                                }
                                else
                                {
                                    Card existing = playerOwner.GetCardAtIndex(dropIndex);
                                    if (existing != null && InventoryManager.Instance != null)
                                    {
                                        InventoryManager.Instance.AddCardObject(existing);
                                    }
                                    playerOwner.EquipCardDirectly(cardRetrieved, dropIndex);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (IsFromInventory && !isToInventory)
                            playerOwner.EquipCard(StartSlotIndex, dropIndex);
                        else if (!IsFromInventory && isToInventory)
                            playerOwner.UnequipCard(StartSlotIndex);
                        else if (!IsFromInventory && !isToInventory && StartSlotIndex != dropIndex)
                            playerOwner.MoveCard(StartSlotIndex, dropIndex);
                    }
                }
            }
        }

        UIManager.Instance.RefreshPlayerUI();
        if (UIManager.Instance.IsInventoryOpen) UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);

        evt.StopPropagation();
    }

    // --- 도우미 함수들 ---

    private void CreateGhostIcon(Card card, Vector2 mousePosition)
    {
        m_GhostIcon = new VisualElement();

        if (card != null && card.CardImage != null)
        {
            m_GhostIcon.style.backgroundImage = new StyleBackground(card.CardImage);
        }

        m_GhostIcon.style.width = target.resolvedStyle.width;
        m_GhostIcon.style.height = target.resolvedStyle.height;
        m_GhostIcon.style.position = Position.Absolute;

        m_PointerOffset = new Vector2(target.resolvedStyle.width / 2, target.resolvedStyle.height / 2);

        // [중요] 마우스 위치(월드)를 Root의 로컬 위치로 변환해야 정확히 배치됨
        Vector2 localPos = m_Root.WorldToLocal(mousePosition);

        m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
        m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;

        m_GhostIcon.style.opacity = 0.7f;
        m_GhostIcon.pickingMode = PickingMode.Ignore;

        m_Root.Add(m_GhostIcon);
    }

    private int ParseSlotIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;
        string numberPart = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        if (int.TryParse(numberPart, out int index)) return index;
        return -1;
    }

    private VisualElement FindParentSlot(VisualElement element)
    {
        while (element != null)
        {
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

    private VisualElement FindInteractionSlot(VisualElement element)
    {
        while (element != null)
        {
            if (element.name == "TargetSlot") return element;
            element = element.parent;
        }
        return null;
    }
}