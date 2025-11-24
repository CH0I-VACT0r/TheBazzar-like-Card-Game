using UnityEngine;
using UnityEngine.UIElements;

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
        InitializeGameLayer(); // 플레이어(아래)
        InitializeHUD();       // 버튼(위)
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
        if (btnClose != null)
        {
            btnClose.clicked += CloseInventory;
        }
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
}
