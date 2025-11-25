using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글톤 접근용

    [Header("UXML Files")]
    public VisualTreeAsset mainLayoutAsset;      // "MainLayout"
    public VisualTreeAsset fixedPlayerAsset;     // "Fixed_Player"
    public VisualTreeAsset battlePageAsset;      // "Battle_Page"
    public VisualTreeAsset hudAsset;             // UI_HUD (버튼, 판매존)
    public VisualTreeAsset inventoryPageAsset;   // Page_Inventory (인벤토리)

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

    public bool IsInventoryOpen
    {
        get
        {
            if (_topContainer == null) return false;
            return _topContainer.Q(null, "inventory-box") != null;
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

        if (inventoryPageAsset == null || _topContainer == null) return;

        _topContainer.Clear();

        // 인벤토리 생성 및 배치
        VisualElement invUI = inventoryPageAsset.Instantiate();
        invUI.style.flexGrow = 1; 
        _topContainer.Add(invUI); 

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
        _topContainer.Clear();
        SwitchToBattlePage();
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
        if (_topContainer == null) return;
        VisualElement invUI = _topContainer.Q<VisualElement>("InventoryRoot");
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

            // ▼▼▼ [삭제됨] 여기서 이벤트(OnSlotPointerDown)를 연결하던 코드를 다 지웠습니다! ▼▼▼
            // 왜냐하면 드래그 기능은 'OpenInventory'에서 한 번만 딱 붙여줄 거니까요.

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
        if (_topContainer == null) return;
        VisualElement invUI = _topContainer.Q<VisualElement>("InventoryRoot");
        if (invUI == null) return;

        // 버튼들 찾기
        Button btnMerc = invUI.Q<Button>("Btn_Tab_Mercenary");
        Button btnCons = invUI.Q<Button>("Btn_Tab_Consumable");
        Button btnMat = invUI.Q<Button>("Btn_Tab_Material");

        if (btnMerc == null || btnCons == null || btnMat == null) return;

        btnMerc.text = LocalizationManager.GetText("ui_tab_mercenary");
        btnCons.text = LocalizationManager.GetText("ui_tab_consumable");
        btnMat.text = LocalizationManager.GetText("ui_tab_material");

        Label title = invUI.Q<Label>("Inventory"); // 이름이 Title이라고 가정
        if (title != null) title.text = LocalizationManager.GetText("ui_inventory_title");

        // 2. 스타일 초기화 (모두 active 떼기)
        btnMerc.RemoveFromClassList("active");
        btnCons.RemoveFromClassList("active");
        btnMat.RemoveFromClassList("active");

        // 3. 현재 탭만 active 붙이기
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
}
