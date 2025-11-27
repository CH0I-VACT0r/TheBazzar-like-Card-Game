using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글톤 접근용

    [Header("UXML Files")]
    public VisualTreeAsset mainLayoutAsset;         // "MainLayout"
    public VisualTreeAsset fixedPlayerAsset;        // "Fixed_Player"
    public VisualTreeAsset battlePageAsset;         // "Battle_Page"
    public VisualTreeAsset hudAsset;                // UI_HUD (버튼, 판매존)
    public VisualTreeAsset inventoryPageAsset;      // Page_Inventory (인벤토리)
    public VisualTreeAsset eventSelectionPageAsset; // 이벤트
    public VisualTreeAsset shopPageAsset;           // Shop
    public VisualTreeAsset eventInteractionPageAsset; // 인터랙션

    [Header("Controllers")]
    private PlayerController playerController;
    private MonsterController monsterController;

    [Header("Document")]
    public UIDocument document;

    // 내부 변수
    private VisualElement _root;             // 전체 화면 루트 (MainLayout)
    private bool _isBattleActive = false;    // 전투 중인지 체크

    // 레이어 컨테이너
    private VisualElement _gameLayer;        // 1층
    private VisualElement _hudContainer;     // 2층
    private VisualElement _overlayContainer; // 3층

    private VisualElement _topContainer;     // 교체 영역 (위)
    private VisualElement _bottomContainer;  // 고정 영역 (아래)

    public CardType CurrentTab { get; private set; } = CardType.Mercenary; // 현재 인벤토리 탭

    // 툴팁 UI
    private VisualElement m_TooltipRoot;
    private Label m_TooltipName;
    private VisualElement m_TooltipTagContainer;
    private Label m_TooltipSkillDesc;
    private Label m_TooltipCooldown;
    private VisualElement m_TooltipQuestContainer;
    private Label m_TooltipQuestTitle;
    private Label m_TooltipQuestDesc;
    private Label m_TooltipQuestStatus;
    private VisualElement m_TooltipStatContainer;
    private VisualElement m_TooltipCritContainer;
    private Label m_TooltipCritChance;
    private VisualElement m_TooltipDurabilityContainer;
    private Label m_TooltipDurability;
    private Label m_TooltipFlavorText;
    private VisualElement m_TooltipDivider1;
    private VisualElement m_TooltipDivider2;

    public bool IsInventoryOpen
    {
        get
        {
            if (_overlayContainer == null) return false;
            return _overlayContainer.childCount > 0;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        BattleManager bm = FindFirstObjectByType<BattleManager>();
        if (bm != null)
        {
            this.playerController = bm.playerController;
            this.monsterController = bm.monsterController;
        }
        else
        {
            Debug.LogError("[UIManager] BattleManager를 찾을 수 없습니다!");
        }

        // 메인 레이아웃 로드
        if (document == null) document = GetComponent<UIDocument>();
        _root = document.rootVisualElement;

        if (mainLayoutAsset != null)
        {
            _root.Clear();
            mainLayoutAsset.CloneTree(_root);
        }
        else
        {
            Debug.LogError("MainLayoutAsset이 연결되지 않았습니다!");
            return;
        }

        // 각 층(Layer) 찾기
        _gameLayer = _root.Q<VisualElement>("GameLayer");
        _hudContainer = _root.Q<VisualElement>("HUDContainer");
        _overlayContainer = _root.Q<VisualElement>("OverlayContainer");

        // GameLayer 안의 위/아래 찾기
        if (_gameLayer != null)
        {
            _topContainer = _gameLayer.Q<VisualElement>("TopContentContainer");
            _bottomContainer = _gameLayer.Q<VisualElement>("BottomFixedContainer");
        }

        // UI 배치 시작
        InitializeHUD();       // 버튼(위)
        InitializeGameLayer(); // 플레이어(아래)     
        SwitchToBattlePage();  // 몬스터(제일 위) -> 시작 화면
        InitializeTooltipUI();
        SetBattleState(false);
    }

    // --- [1층 하단] 플레이어 UI 초기화 ---
    private void InitializeGameLayer()
    {
        if (fixedPlayerAsset == null || _bottomContainer == null) return;

        VisualElement playerUI = fixedPlayerAsset.Instantiate();
        playerUI.style.flexGrow = 1;
        _bottomContainer.Add(playerUI);

        // 컨트롤러 연결
        if (playerController != null)
        {
            playerController.InitializeUI(playerUI, _root);
        }
    }

    private void InitializeTooltipUI()
    {
        m_TooltipRoot = _root.Q<VisualElement>("TooltipRoot");
        if (m_TooltipRoot == null) return;

        m_TooltipName = m_TooltipRoot.Q<Label>("TooltipName");
        m_TooltipTagContainer = m_TooltipRoot.Q<VisualElement>("TooltipTagContainer");
        m_TooltipSkillDesc = m_TooltipRoot.Q<Label>("TooltipSkillDesc");
        m_TooltipCooldown = m_TooltipRoot.Q<Label>("TooltipCooldown");

        m_TooltipQuestContainer = m_TooltipRoot.Q<VisualElement>("TooltipQuestContainer");
        if (m_TooltipQuestContainer != null)
        {
            m_TooltipQuestTitle = m_TooltipQuestContainer.Q<Label>("QuestName");
            m_TooltipQuestDesc = m_TooltipQuestContainer.Q<Label>("TooltipQuestDesc");
            m_TooltipQuestStatus = m_TooltipQuestContainer.Q<Label>("TooltipQuestStatus");
        }

        m_TooltipStatContainer = m_TooltipRoot.Q<VisualElement>("TooltipStatContainer");

        m_TooltipCritContainer = m_TooltipRoot.Q<VisualElement>("TooltipCritContainer");
        if (m_TooltipCritContainer != null) m_TooltipCritChance = m_TooltipCritContainer.Q<Label>("TooltipCritChance");

        m_TooltipDurabilityContainer = m_TooltipRoot.Q<VisualElement>("TooltipDurabilityContainer");
        if (m_TooltipDurabilityContainer != null) m_TooltipDurability = m_TooltipDurabilityContainer.Q<Label>("TooltipDurability");

        m_TooltipFlavorText = m_TooltipRoot.Q<Label>("TooltipFlavorText");
        m_TooltipDivider1 = m_TooltipRoot.Q<VisualElement>("TooltipDivider1");
        m_TooltipDivider2 = m_TooltipRoot.Q<VisualElement>("TooltipDivider2");

        m_TooltipRoot.style.display = DisplayStyle.None; // 초기엔 숨김
    }

    // --- [1층 상단] 몬스터(전투) 페이지 전환 ---
    public void SwitchToBattlePage()
    {
        if (IsInventoryOpen)
        {
            Debug.LogWarning("인벤토리가 열려 있어서 전투를 시작할 수 없습니다!");
            return;
        }

        if (battlePageAsset == null || _topContainer == null) return;

        _topContainer.Clear(); // 기존 내용 비우기

        VisualElement battleUI = battlePageAsset.Instantiate();
        battleUI.style.flexGrow = 1;
        _topContainer.Add(battleUI);

        // 몬스터 컨트롤러 연결
        if (monsterController != null)
        {
            monsterController.InitializeUI(battleUI, _root);
        }
    }

    // --- [2층] HUD (버튼, 판매존) 초기화 ---
    private void InitializeHUD()
    {
        if (hudAsset == null || _hudContainer == null) return;

        VisualElement hudUI = hudAsset.Instantiate();
        hudUI.style.flexGrow = 1;
        hudUI.pickingMode = PickingMode.Ignore; // 빈 공간 클릭 통과
        _hudContainer.Add(hudUI);

        // [인벤토리 버튼] 기능 연결
        Button btnBag = hudUI.Q<Button>("Btn_OpenInventory");
        if (btnBag != null)
        {
            btnBag.clicked -= OpenInventory;
            btnBag.clicked -= ToggleInventory;
            btnBag.clicked += ToggleInventory;
        }
    }

    public void SetBattleState(bool isActive)
    {
        _isBattleActive = isActive;
        Debug.Log($"전투 상태 변경: {isActive}");
    }

    // --- [3층] 인벤토리 열기/닫기 ---
    public void OpenInventory()
    {
        if (_isBattleActive)
        {
            Debug.Log("전투 중에는 인벤토리를 열 수 없습니다!");
            return;
        }

        if (inventoryPageAsset == null || _overlayContainer == null) return;

        // 인벤토리 생성 및 배치
        VisualElement invUI = inventoryPageAsset.Instantiate();
        invUI.pickingMode = PickingMode.Ignore;
        invUI.style.flexGrow = 1;
        invUI.style.justifyContent = Justify.Center; // 중앙 정렬 보조
        invUI.style.alignItems = Align.Center;

        _overlayContainer.Add(invUI);

        VisualElement windowBox = invUI.Q<VisualElement>("InventoryRoot"); // 움직일 몸체
        VisualElement header = invUI.Q<VisualElement>("HeaderRow"); // 잡고 흔들 손잡이 (이름이 HeaderRow인지 확인!)

        // 창 이동 핸들러 부착
        if (header != null && windowBox != null)
        {
            WindowDragHandler dragHandler = new WindowDragHandler(header, windowBox);
            header.AddManipulator(dragHandler);
        }

        // 닫기 버튼 연결
        Button btnClose = invUI.Q<Button>("Btn_CloseInventory");
        if (btnClose != null) btnClose.clicked += CloseInventory;

        var btnMerc = invUI.Q<Button>("Btn_Tab_Mercenary");
        var btnCons = invUI.Q<Button>("Btn_Tab_Consumable");
        var btnMat = invUI.Q<Button>("Btn_Tab_Material");

        if (btnMerc != null) btnMerc.clicked += () => OnTabClicked(CardType.Mercenary);
        if (btnCons != null) btnCons.clicked += () => OnTabClicked(CardType.Consumable);
        if (btnMat != null) btnMat.clicked += () => OnTabClicked(CardType.Material);

        for (int i = 0; i < 7; i++)
        {
            VisualElement slot = invUI.Q<VisualElement>($"InvSlot_{i}");
            if (slot != null)
            {
                // DragAndDropHandler를 슬롯에 붙여줍니다.
                // (생성자 인자: 타겟슬롯, 전체루트, 컨트롤러)
                DragAndDropHandler handler = new DragAndDropHandler(slot, _root, playerController);
                slot.AddManipulator(handler);
            }
        }
        UpdateTabState();
        // 3. 열리자마자 현재 탭(기본: Mercenary) 내용 보여주기
        RefreshInventoryGrid(CurrentTab);

        Debug.Log("인벤토리 열림 (탭 연결 완료)");
    }

    public void CloseInventory()
    {
        if (_overlayContainer != null)
        {
            _overlayContainer.Clear();
        }
    }

    public void ToggleInventory()
    {
        if (IsInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void RefreshInventoryGrid(CardType type)
    {
        if (_overlayContainer == null) return;
        VisualElement invUI = _overlayContainer.Q<VisualElement>("InventoryRoot");
        if (invUI == null) return;

        // 데이터 리스트 가져오기
        List<Card> dataList = null;
        if (InventoryManager.Instance != null)
        {
            dataList = InventoryManager.Instance.GetListByType(type);
        }
        else
        {
            dataList = new List<Card>();
        }

        Debug.Log($"[UI] {type} 탭 갱신 중... (데이터: {dataList.Count}개)");

        // 슬롯 갱신 루프
        for (int i = 0; i < 7; i++)
        {
            VisualElement slot = invUI.Q<VisualElement>($"InvSlot_{i}");
            if (slot == null) continue;

            VisualElement cardImage = slot.Q<VisualElement>("CardImage");
            if (cardImage == null) continue;

            if (dataList != null && i < dataList.Count)
            {
                Card card = dataList[i];
                if (card != null)
                {
                    if (card.CardImage != null)
                        cardImage.style.backgroundImage = new StyleBackground(card.CardImage);
                    else
                        cardImage.style.backgroundColor = new StyleColor(Color.gray);

                    // [중요] 데이터 심기 (이걸 DragHandler가 읽어갑니다)
                    cardImage.userData = card;
                }
                else
                {
                    cardImage.style.backgroundImage = null;
                    cardImage.style.backgroundColor = new StyleColor(Color.clear);
                    cardImage.userData = null;
                }
            }
            else
            {
                // 빈 슬롯
                cardImage.style.backgroundImage = null;
                cardImage.style.backgroundColor = new StyleColor(Color.clear);
                cardImage.userData = null;
            }
        }
    }

    // 탭 클릭 시 실행할 함수
    private void OnTabClicked(CardType type)
    {
        CurrentTab = type;
        UpdateTabState();
        RefreshInventoryGrid(type);
    }

    // 플레이어 UI 갱신 요청
    public void RefreshPlayerUI()
    {
        if (playerController != null)
        {
            playerController.UpdatePartyUI();
        }
    }

    // 골드 UI 갱신
    public void UpdateGoldUI(int currentGold)
    {
        if (_hudContainer == null) return;
        Label goldLabel = _hudContainer.Q<Label>("GoldDisplay");
        if (goldLabel != null)
        {
            goldLabel.text = $"{currentGold} G";
        }
    }

    // 탭 버튼의 스타일과 텍스트를 갱신하는 함수
    private void UpdateTabState()
    {
        if (_overlayContainer == null) return;
        VisualElement invUI = _overlayContainer.Q<VisualElement>("InventoryRoot");
        if (invUI == null) return;

        // 버튼들 찾기
        Button btnMerc = invUI.Q<Button>("Btn_Tab_Mercenary");
        Button btnCons = invUI.Q<Button>("Btn_Tab_Consumable");
        Button btnMat = invUI.Q<Button>("Btn_Tab_Material");

        if (btnMerc == null || btnCons == null || btnMat == null) return;

        btnMerc.text = LocalizationManager.GetText("ui_tab_mercenary");
        btnCons.text = LocalizationManager.GetText("ui_tab_consumable");
        btnMat.text = LocalizationManager.GetText("ui_tab_material");

        Label title = invUI.Q<Label>("Inventory");
        if (title != null) title.text = LocalizationManager.GetText("ui_inventory_title");

        // 스타일 초기화
        btnMerc.RemoveFromClassList("active");
        btnCons.RemoveFromClassList("active");
        btnMat.RemoveFromClassList("active");

        // 현재 탭만 active
        switch (CurrentTab)
        {
            case CardType.Mercenary:
                btnMerc.AddToClassList("active");
                break;
            case CardType.Consumable:
                btnCons.AddToClassList("active");
                break;
            case CardType.Material:
                btnMat.AddToClassList("active");
                break;
        }
    }

    public void SwitchTab(CardType type)
    {
        CurrentTab = type; // 탭 변수 변경

        // 인벤토리가 열려있을 때만 UI 갱신
        if (IsInventoryOpen)
        {
            UpdateTabState();
            RefreshInventoryGrid(type);
        }
    }

    // 이벤트 선택창 띄우기
    public void ShowEventSelectionWindow(List<GameEvent> events)
    {
        if (eventSelectionPageAsset == null || _topContainer == null) return;

        _topContainer.Clear();
        VisualElement screen = eventSelectionPageAsset.Instantiate();
        screen.style.flexGrow = 1;
        _topContainer.Add(screen);

        for (int i = 0; i < 3; i++)
        {
            VisualElement slot = screen.Q<VisualElement>($"EventSlot_{i}");

            if (i < events.Count && events[i] != null)
            {
                GameEvent evtData = events[i];

                VisualElement img = slot.Q<VisualElement>("EventImage");
                Label nameLbl = slot.Q<Label>("EventName");
                // (EventDesc 라벨이 있다면 여기서 채워도 되지만, 툴팁을 쓸 거니 패스)

                if (img != null) img.style.backgroundImage = new StyleBackground(evtData.eventImage);
                if (nameLbl != null) nameLbl.text = LocalizationManager.GetText(evtData.titleKey);

                // 테두리 등급 설정
                slot.RemoveFromClassList("border-bronze");
                slot.RemoveFromClassList("border-silver");
                slot.RemoveFromClassList("border-gold");
                slot.RemoveFromClassList("border-diamond");

                switch (evtData.rarity)
                {
                    case CardRarity.Bronze: slot.AddToClassList("border-bronze"); break;
                    case CardRarity.Silver: slot.AddToClassList("border-silver"); break;
                    case CardRarity.Gold: slot.AddToClassList("border-gold"); break;
                    case CardRarity.Diamond: slot.AddToClassList("border-diamond"); break;
                }

                // 클릭 이벤트
                slot.RegisterCallback<ClickEvent>(e => OnEventSelected(evtData));

                // 툴팁 이벤트
                // 마우스 위치(e.position)가 아니라 'slot' 자체를 넘겨줍니다.
                slot.RegisterCallback<PointerEnterEvent>(e => ShowEventTooltip(evtData, slot));
                slot.RegisterCallback<PointerLeaveEvent>(e => HideEventTooltip());
            }
            else
            {
                slot.style.display = DisplayStyle.None;
            }
        }
    }


    // 툴팁 표시 함수
    public void ShowEventTooltip(GameEvent evtData, VisualElement targetSlot)
    {
        if (_root == null) return;

        // 찾는 대상이 'EventTooltipRoot'로 바뀜
        VisualElement tooltip = _root.Q<VisualElement>("EventTooltipRoot");

        if (tooltip != null && targetSlot != null)
        {
            //내용 채우기
            Label titleLbl = tooltip.Q<Label>("EvtTitle");
            Label descLbl = tooltip.Q<Label>("EvtDesc");
            Label rarityLbl = tooltip.Q<Label>("EvtRarity");

            if (titleLbl != null) titleLbl.text = LocalizationManager.GetText(evtData.titleKey);
            if (descLbl != null) descLbl.text = LocalizationManager.GetText(evtData.descKey);

            // 등급 표시는 별도 라벨로 뺌 (색상은 USS에서 처리하거나 여기서 태그 사용)
            if (rarityLbl != null)
            {
                rarityLbl.text = evtData.rarity.ToString();
                // 필요하다면 색상 변경 로직 추가 (gold 등)
                rarityLbl.style.color = GetRarityColor(evtData.rarity);
            }

            // 2. 위치 계산
            Rect slotRect = targetSlot.worldBound;
            float targetX = slotRect.x + 12.5f;
            float targetY = slotRect.yMax + 30f;

            float screenHeight = _root.resolvedStyle.height;
            float tipHeight = tooltip.layout.height;
            if (float.IsNaN(tipHeight)) tipHeight = 150f;

            if (targetY + tipHeight > screenHeight)
            {
                targetY = slotRect.y - tipHeight - 10f;
            }

            tooltip.style.position = Position.Absolute;
            tooltip.style.left = targetX;
            tooltip.style.top = targetY;

            // 3. 표시
            tooltip.style.display = DisplayStyle.Flex;
            tooltip.BringToFront();
        }
    }

    // 툴팁 숨기기 (이벤트용)
    public void HideEventTooltip()
    {
        if (_root == null) return;
        VisualElement tooltip = _root.Q<VisualElement>("EventTooltipRoot");
        if (tooltip != null) tooltip.style.display = DisplayStyle.None;
    }

    private StyleColor GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Bronze: return new StyleColor(new Color(0.8f, 0.5f, 0.2f)); // 구리색
            case CardRarity.Silver: return new StyleColor(Color.white);
            case CardRarity.Gold: return new StyleColor(new Color(1f, 0.84f, 0f));
            case CardRarity.Diamond: return new StyleColor(Color.cyan);
            default: return new StyleColor(Color.white);
        }
    }

    // 이벤트 선택 시 실행 로직
    private void OnEventSelected(GameEvent selectedEvent)
    {
        Debug.Log($"[UI] 이벤트 선택됨: {selectedEvent.eventID}");

        HideEventTooltip(); // (아까 만든 툴팁 끄기 함수)

        PlayerController player = null;
        BattleManager bm = FindFirstObjectByType<BattleManager>();
        if (bm != null)
        {
            player = bm.playerController;
        }

        if (player != null)
        {
            selectedEvent.Execute(player);
        }
        else
        {
            Debug.LogError("[UIManager] PlayerController를 찾을 수 없습니다!");
        }
    }

    // 상점 UI
    public void ShowShopUI(Event_Shop shopData, List<Card> stock)
    {
        if (shopPageAsset == null || _topContainer == null) return;

        _topContainer.Clear();
        VisualElement screen = shopPageAsset.Instantiate();
        screen.style.flexGrow = 1;
        _topContainer.Add(screen);

        // 상점 주인 설정
        VisualElement keeperImg = screen.Q<VisualElement>("ShopkeeperImage");
        Label dialogue = screen.Q<Label>("ShopkeeperDialogue");

        if (keeperImg != null) keeperImg.style.backgroundImage = new StyleBackground(shopData.shopkeeperImage);
        if (dialogue != null) dialogue.text = LocalizationManager.GetText(shopData.greetingKey);

        // 버튼 연결
        Button btnReroll = screen.Q<Button>("Btn_Reroll");
        Button btnLeave = screen.Q<Button>("Btn_Leave");

        if (btnReroll != null)
        {
            btnReroll.text = $"Reroll ({shopData.rerollPrice}G)"; // 텍스트 갱신
            btnReroll.clicked += () => ShopManager.Instance.RerollStock();
        }
        if (btnLeave != null) btnLeave.clicked += () => ShopManager.Instance.CloseShop();

        // 슬롯 클릭 이벤트 일괄 등록
        for (int i = 0; i < 3; i++)
        {
            int index = i; // 캡처 (람다식 내부에서 쓰기 위함)
            VisualElement slot = screen.Q<VisualElement>($"ShopSlot_{index}");

            if (slot != null)
            {
                // 1. 구매 클릭 이벤트
                slot.RegisterCallback<ClickEvent>(evt =>
                {
                    ShopManager.Instance.TryBuyItem(index);
                });

                // 툴팁 표시 (마우스 올렸을 때)
                slot.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (slot.userData is Card cardData)
                    {
                        ShowCardTooltip(cardData, slot);
                    }
                });

                // 툴팁 숨기기 (마우스 나갔을 때)
                slot.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    HideTooltip();
                });
            }
        }

        // 상품 진열
        UpdateShopSlots(stock);
    }

    // 상품 슬롯만 갱신
    public void UpdateShopSlots(List<Card> stock)
    {
        if (_topContainer == null) return;
        VisualElement screen = _topContainer.Q<VisualElement>("ShopWindow"); // ShopRoot나 ShopWindow
        if (screen == null) return;

        for (int i = 0; i < 3; i++)
        {
            VisualElement slot = screen.Q<VisualElement>($"ShopSlot_{i}");
            if (slot == null) continue;

            // 데이터 확인
            Card item = (stock != null && i < stock.Count) ? stock[i] : null;

            VisualElement img = slot.Q<VisualElement>("CardImage");
            VisualElement priceTag = slot.Q<VisualElement>("PriceTag");
            Label priceLbl = slot.Q<Label>("PriceLabel");
            VisualElement soldCover = slot.Q<VisualElement>("SoldOutCover");
            VisualElement roleContainer = slot.Q<VisualElement>("RoleUIContainer");

            if (item != null)
            {
                // [판매 중]
                if (img != null)
                {
                    img.style.backgroundImage = new StyleBackground(item.CardImage);
                    img.style.backgroundColor = new StyleColor(Color.clear); // 배경 투명
                }

                if (priceTag != null)
                {
                    priceTag.style.display = DisplayStyle.Flex; // 켜기
                    if (priceLbl != null) priceLbl.text = $"{item.GetCurrentPrice()}";
                }
                if (soldCover != null) soldCover.style.display = DisplayStyle.None;

                if (roleContainer != null)
                {
                    roleContainer.Clear(); // 기존 아이콘 삭제

                    // 1. 대미지
                    float dmg = item.GetCurrentDamage();
                    if (dmg > 0) CreateRoleIcon(roleContainer, "role-attacker", dmg.ToString());

                    // 2. 상태이상 (출혈, 화상, 중독, 빙결)
                    int bleed = item.GetCurrentBleedStacks();
                    if (bleed > 0) CreateRoleIcon(roleContainer, "role-bleed", bleed.ToString());

                    int burn = item.GetCurrentBurnStacks();
                    if (burn > 0) CreateRoleIcon(roleContainer, "role-burn", burn.ToString());

                    int poison = item.GetCurrentPoisonStacks();
                    if (poison > 0) CreateRoleIcon(roleContainer, "role-poison", poison.ToString());

                    float freeze = item.GetCurrentFreezeDuration();
                    if (freeze > 0) CreateRoleIcon(roleContainer, "role-freeze", freeze.ToString("0.0"));

                    // 3. 쉴드
                    float shield = item.GetCurrentShield();
                    if (shield > 0) CreateRoleIcon(roleContainer, "role-tanker", shield.ToString());

                    // 4. 힐
                    float heal = item.GetCurrentHeal();
                    if (heal > 0) CreateRoleIcon(roleContainer, "role-healer", heal.ToString());

                    int healDot = item.GetCurrentHealStacks();
                    if (healDot > 0) CreateRoleIcon(roleContainer, "role-heal-dot", healDot.ToString());
                }

                // 슬롯에 클릭 가능 표시 (활성화)
                slot.userData = item; // 인덱스를 저장해둠
                slot.pickingMode = PickingMode.Position; // 클릭 받음
            }
            else
            {
                // 품절
                if (img != null)
                {
                    img.style.backgroundImage = null;
                    img.style.backgroundColor = new StyleColor(Color.clear);
                }
                if (priceTag != null) priceTag.style.display = DisplayStyle.None;
                if (roleContainer != null) roleContainer.Clear();
                if (soldCover != null) soldCover.style.display = DisplayStyle.Flex;
                // 품절된 슬롯은 클릭 안 받음
                slot.userData = null;
                slot.pickingMode = PickingMode.Ignore;
            }
        }
    }

    public void ShowCardTooltip(Card card, VisualElement slotElement)
    {
        // null 체크
        if (m_TooltipRoot == null || card == null || slotElement == null) return;

        // --- 카드 데이터로 툴팁 UI 내용 채우기 ---
        // 이름 (Key -> 번역)
        m_TooltipName.text = LocalizationManager.GetText(card.CardNameKey);
        // 태그 (동적 Label 생성)
        m_TooltipTagContainer.Clear();
        m_TooltipTagContainer.style.display = DisplayStyle.None;
        if (card.TagKeys != null && card.TagKeys.Count > 0)
        {
            foreach (string tagKey in card.TagKeys)
            {
                string translatedTag = LocalizationManager.GetText(tagKey);
                Label tagLabel = new Label(translatedTag);
                tagLabel.AddToClassList("tooltip-tag-label");
                m_TooltipTagContainer.Add(tagLabel);
            }
            m_TooltipTagContainer.style.display = DisplayStyle.Flex;
        }

        // --- 쿨타임 표시 (별도 라벨) ---
        if (card.ShowCooldownUI && card.BaseCooldownTime > 0)
        {
            m_TooltipCooldown.text = LocalizationManager.GetText("stat_cooldown", card.BaseCooldownTime.ToString());
            m_TooltipCooldown.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipCooldown.style.display = DisplayStyle.None;
        }
        //  메인 스킬 설명
        m_TooltipSkillDesc.text = LocalizationManager.GetText(card.CardSkillDescriptionKey);
        // 부가 스탯/효과 목록
        m_TooltipStatContainer.Clear();

        System.Action<string, float> AddStatLine = (string key, float value) =>
        {
            if (value > 0)
            {
                Label line = new Label(LocalizationManager.GetText(key, value.ToString()));
                line.AddToClassList("tooltip-stat-line");
                m_TooltipStatContainer.Add(line);
            }
        };

        System.Action<string> AddTextLine = (string text) =>
        {
            Label line = new Label(text);
            line.AddToClassList("tooltip-stat-line");
            m_TooltipStatContainer.Add(line);
        };

        // 모든 스탯 추가
        AddStatLine("stat_damage", card.GetCurrentDamage());
        AddStatLine("stat_shield", card.GetCurrentShield());
        AddStatLine("stat_heal", card.GetCurrentHeal());
        AddStatLine("stat_crit_chance", card.GetCurrentCritChance() * 100f);
        AddStatLine("stat_apply_bleed", card.GetCurrentBleedStacks());
        AddStatLine("stat_apply_poison", card.GetCurrentPoisonStacks());
        AddStatLine("stat_apply_burn", card.GetCurrentBurnStacks());
        AddStatLine("stat_apply_heal_dot", card.GetCurrentHealStacks());
        AddStatLine("stat_apply_freeze", card.GetCurrentFreezeDuration());
        AddStatLine("stat_apply_haste", card.GetCurrentHasteDuration());
        AddStatLine("stat_apply_slow", card.GetCurrentSlowDuration());
        AddStatLine("stat_apply_cooldown_reduction", card.GetCurrentCooldownReduction());
        AddStatLine("stat_apply_cooldown_increase", card.GetCurrentCooldownIncrease());
        AddStatLine("stat_apply_echo", card.GetCurrentEchoStacks());
        AddStatLine("stat_apply_shock", card.GetCurrentShockDuration());
        AddStatLine("stat_apply_sturdy", card.GetCurrentSturdyDuration());
        AddStatLine("stat_price_inflate", card.GetCurrentPriceInflate());
        AddStatLine("stat_price_extort", card.GetCurrentPriceExtort());
        AddStatLine("stat_triggers_shuffle", card.TriggersTargetShuffle);
        AddStatLine("stat_triggers_chain", card.TriggersChainCount);

        // 소환
        if (card.SummonCount > 0 && !string.IsNullOrEmpty(card.SummonCardNameKey))
        {
            // 소환할 카드 이름 번역
            string summonName = LocalizationManager.GetText(card.SummonCardNameKey);
            // 최종 문장 완성 ("소환: "카드 이름" x 2")
            string text = LocalizationManager.GetText("stat_summon", summonName, card.SummonCount);
            AddTextLine(text);
        }

        // 유언
        if (!string.IsNullOrEmpty(card.DeathrattleDescKey))
        {
            // 유언 효과 설명 번역 
            string effectDesc = LocalizationManager.GetText(card.DeathrattleDescKey);
            // 최종 문장 완성 ("유언: ~~~")
            string text = LocalizationManager.GetText("stat_deathrattle", effectDesc);
            AddTextLine(text);
        }

        // 변이 
        if (card.PolymorphDurationToApply > 0 && !string.IsNullOrEmpty(card.PolymorphTargetNameKey))
        {
            // 변이 대상 이름
            string targetName = LocalizationManager.GetText(card.PolymorphTargetNameKey);

            // 최종 문장 완성
            string text = LocalizationManager.GetText("stat_apply_polymorph", targetName, card.PolymorphDurationToApply);

            // 텍스트 라인 추가
            Label line = new Label(text);
            line.AddToClassList("tooltip-stat-line");
            m_TooltipStatContainer.Add(line);
        }

        // 컨테이너 보이기/숨기기
        if (m_TooltipStatContainer.childCount > 0)
        {
            m_TooltipStatContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipStatContainer.style.display = DisplayStyle.None;
        }

        // --- [이하 동일] 퀘스트, 내구도, 플레이버 텍스트 ---

        // 퀘스트 (조건부 표시)
        if (card.HasQuest)
        {
            m_TooltipQuestTitle.text = LocalizationManager.GetText(card.QuestTitleKey);
            m_TooltipQuestDesc.text = LocalizationManager.GetText(card.QuestDescriptionKey);
            bool isDone = card.IsQuestComplete;
            string statusKey = isDone ? "quest_status_complete" : "quest_status_incomplete";
            m_TooltipQuestStatus.text = LocalizationManager.GetText(statusKey);
            m_TooltipQuestStatus.EnableInClassList("quest-status-complete", isDone);
            m_TooltipQuestStatus.EnableInClassList("quest-status-incomplete", !isDone);
            m_TooltipQuestContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipQuestContainer.style.display = DisplayStyle.None;
        }

        // 내구도 (조건부 표시)
        if (card.Durability > -1)
        {
            m_TooltipDurability.text = LocalizationManager.GetText("stat_durability", card.Durability.ToString());
            m_TooltipDurabilityContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipDurabilityContainer.style.display = DisplayStyle.None;
        }

        // 플레이버 텍스트 (Key -> 번역)
        string flavor = LocalizationManager.GetText(card.FlavorTextKey);
        if (!string.IsNullOrEmpty(flavor) && !flavor.StartsWith("["))
        {
            m_TooltipFlavorText.text = $"\"{flavor}\"";
            m_TooltipFlavorText.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipFlavorText.style.display = DisplayStyle.None;
        }

        //Divider1 (태그/쿨타임과 스킬설명 사이)
        bool isTopSectionVisible = (m_TooltipTagContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipCooldown.style.display == DisplayStyle.Flex);

        m_TooltipDivider1.style.display = isTopSectionVisible ? DisplayStyle.Flex : DisplayStyle.None;

        // Divider2 (부가 스탯/퀘스트/내구도와 플레이버 텍스트 사이)
        bool isMidSectionVisible = (
                                    m_TooltipQuestContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipDurabilityContainer.style.display == DisplayStyle.Flex);

        bool isFlavorTextVisible = (m_TooltipFlavorText.style.display == DisplayStyle.Flex);

        m_TooltipDivider2.style.display = (isMidSectionVisible && isFlavorTextVisible) ? DisplayStyle.Flex : DisplayStyle.None;

        // --- 툴팁 위치 조절 (슬롯의 오른쪽 상단) ---
        Rect slotRect = slotElement.worldBound;
        float worldX = slotRect.xMax + 10f; // 오른쪽
        float worldY = slotRect.yMin;       // 위쪽 라인

        float screenWidth = _root.resolvedStyle.width;
        float screenHeight = _root.resolvedStyle.height;
        float tipWidth = 260f; // 예상 너비
        float tipHeight = 300f;

        // 오른쪽 뚫으면 왼쪽으로
        if (worldX + tipWidth > screenWidth)
        {
            worldX = slotRect.xMin - tipWidth - 10f;
        }

        // 아래 뚫으면 위로
        if (worldY + tipHeight > screenHeight)
        {
            worldY = screenHeight - tipHeight - 10f;
        }

        // [중요] World 좌표를 Root 기준 Local 좌표로 변환!
        Vector2 localPos = _root.WorldToLocal(new Vector2(worldX, worldY));

        m_TooltipRoot.style.position = Position.Absolute;
        m_TooltipRoot.style.left = localPos.x;
        m_TooltipRoot.style.top = localPos.y;

        // 최상단 이동 및 표시
        if (_root != null && m_TooltipRoot.parent != _root)
        {
            _root.Add(m_TooltipRoot);
        }

        m_TooltipRoot.style.display = DisplayStyle.Flex;
        m_TooltipRoot.BringToFront();
    }

    private void CreateRoleIcon(VisualElement container, string roleClass, string valueText)
    {
        // 1) 아이콘 생성
        VisualElement icon = new VisualElement();
        icon.AddToClassList("card-role-icon"); // 공통 스타일 (Battle.uss에 있음)
        icon.AddToClassList(roleClass);        // 개별 색상 스타일

        // 2) 텍스트 라벨 생성
        Label label = new Label(valueText);
        label.AddToClassList("card-role-label");

        // 3) 조립
        icon.Add(label);
        container.Add(icon);

        // (상점에서는 아이콘 클릭 막기)
        icon.pickingMode = PickingMode.Ignore;
        label.pickingMode = PickingMode.Ignore;
    }

    public void HideTooltip()
    {
        if (m_TooltipRoot != null)
        {
            m_TooltipRoot.style.display = DisplayStyle.None;
        }
    }

    // 1. 상호작용 UI 열기
    public void ShowEventInteractionUI(GameEvent evtData)
    {
        if (eventInteractionPageAsset == null || _topContainer == null) return;

        _topContainer.Clear();
        VisualElement screen = eventInteractionPageAsset.Instantiate();
        screen.style.flexGrow = 1;
        _topContainer.Add(screen);

        // 텍스트 설정
        Label title = screen.Q<Label>("EventTitle");
        Label desc = screen.Q<Label>("EventDesc");
        if (title != null) title.text = LocalizationManager.GetText(evtData.titleKey);
        if (desc != null) desc.text = LocalizationManager.GetText(evtData.descKey);

        // 버튼 연결
        Button btnAction = screen.Q<Button>("Btn_Action");
        Button btnLeave = screen.Q<Button>("Btn_Leave");

        if (btnAction != null)
        {
            btnAction.clicked += () => EventInteractionManager.Instance.OnActionButtonClick();
            btnAction.SetEnabled(false); // 처음엔 비활성화
            btnAction.AddToClassList("disabled"); // 스타일 적용
        }
        if (btnLeave != null)
        {
            btnLeave.clicked += () => EventInteractionManager.Instance.CloseInteraction();
        }

        // 타겟 슬롯 초기화 (비우기)
        UpdateInteractionSlot(null);
    }

    // 2. 슬롯 갱신 (카드가 드롭되면 호출됨)
    public void UpdateInteractionSlot(Card card)
    {
        if (_topContainer == null) return;

        VisualElement screen = _topContainer.Q<VisualElement>("InteractionWindow");
        if (screen == null) screen = _topContainer.Q<VisualElement>("InteractionRoot"); // 혹시 모르니
        if (screen == null) return;

        VisualElement targetSlot = screen.Q<VisualElement>("TargetSlot");
        VisualElement img = targetSlot?.Q<VisualElement>("CardImage");
        VisualElement roleContainer = targetSlot?.Q<VisualElement>("RoleUIContainer");
        Button btnAction = screen.Q<Button>("Btn_Action");

        if (card != null)
        {
            // 카드 보여주기
            if (img != null)
            {
                img.style.backgroundImage = new StyleBackground(card.CardImage);
                img.style.display = DisplayStyle.Flex;
            }

            // Role UI (기존 함수 재활용!)
            if (roleContainer != null)
            {
                roleContainer.Clear();
                float dmg = card.GetCurrentDamage();
                if (dmg > 0) CreateRoleIcon(roleContainer, "role-attacker", dmg.ToString());
                // ... (필요한 스탯들 추가)
            }

            // 버튼 활성화
            if (btnAction != null)
            {
                btnAction.SetEnabled(true);
                btnAction.RemoveFromClassList("disabled");
            }
        }
        else
        {
            // 비우기
            if (img != null) img.style.display = DisplayStyle.None;
            if (roleContainer != null) roleContainer.Clear();

            // 버튼 비활성화
            if (btnAction != null)
            {
                btnAction.SetEnabled(false);
                btnAction.AddToClassList("disabled");
            }
        }
    }
}
