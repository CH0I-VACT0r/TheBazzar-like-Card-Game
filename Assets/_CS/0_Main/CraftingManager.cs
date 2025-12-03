using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("UI References")]
    public VisualTreeAsset craftingPageAsset; // Page_Crafting.uxml 연결 필요
    private VisualElement _root;
    private VisualElement[] _inputSlots = new VisualElement[2];
    private VisualElement _resultSlot;
    private Label _resultNameLabel;
    private Button _craftButton;
    private Button _closeButton;

    [Header("State")]
    // 현재 슬롯에 올라가 있는 카드 데이터 (없으면 null)
    private Card[] _inputCards = new Card[2];
    private Card _craftedResultCard = null; // 제작 완료되어 결과창에 있는 카드

    // 현재 유효한 레시피 (없으면 null)
    private CraftingRecipe _currentValidRecipe = null;

    [Header("Data")]
    public List<CraftingRecipe> allRecipes; // 에디터에서 할당

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 제작 UI 열기
    public void OpenCraftingUI()
    {
        if (craftingPageAsset == null) return;

        // UI 인스턴스화
        _root = craftingPageAsset.Instantiate();
        _root.style.flexGrow = 1;

        var uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc != null)
        {
            uiDoc.rootVisualElement.Add(_root);
        }
        else
        {
            Debug.LogError("[CraftingManager] 씬에 UIDocument가 없습니다!");
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        // UI 요소 찾기
        _inputSlots[0] = _root.Q<VisualElement>("CraftInput_0");
        _inputSlots[1] = _root.Q<VisualElement>("CraftInput_1");
        _resultSlot = _root.Q<VisualElement>("ResultIcon");
        _resultNameLabel = _root.Q<Label>("ResultName");

        _craftButton = _root.Q<Button>("Btn_Craft");
        _closeButton = _root.Q<Button>("Btn_Close");

        // 초기화
        _inputCards[0] = null;
        _inputCards[1] = null;
        _craftedResultCard = null;
        UpdateSlotVisuals();
        CheckRecipe();

        // 버튼 이벤트 연결
        if (_craftButton != null) _craftButton.clicked += OnCraftButtonClicked;
        if (_closeButton != null) _closeButton.clicked += OnCloseButtonClicked;

        RegisterSlotCallbacks();
    }

    // ---------------------------------------------------------
    //  외부(DragDropManager)에서 호출할 함수들
    // ---------------------------------------------------------

    public bool TryDropCardOnSlot(int slotIndex, Card card)
    {
        if (slotIndex < 0 || slotIndex >= 2) return false;
        if (_craftedResultCard != null) return false; // 결과물이 있으면 재료 투입 불가

        _inputCards[slotIndex] = card;
        UpdateSlotVisuals();
        CheckRecipe();
        return true;
    }

    public Card TryRemoveCardFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 2) return null;

        Card card = _inputCards[slotIndex];
        _inputCards[slotIndex] = null;

        UpdateSlotVisuals();
        CheckRecipe();
        return card;
    }

    public Card ClaimResultCard()
    {
        if (_craftedResultCard == null) return null;

        Card card = _craftedResultCard;
        _craftedResultCard = null;

        UpdateSlotVisuals();

        // UI 초기화
        if (_resultNameLabel != null) _resultNameLabel.text = "Select Ingredients";

        if (_craftButton != null)
        {
            _craftButton.SetEnabled(false);
            _craftButton.RemoveFromClassList("disabled"); // 스타일 초기화
            _craftButton.AddToClassList("disabled");
        }

        return card;
    }

    // ---------------------------------------------------------
    //  내부 로직
    // ---------------------------------------------------------

    private void UpdateSlotVisuals()
    {
        // 입력 슬롯 갱신
        for (int i = 0; i < 2; i++)
        {
            if (_inputSlots[i] == null) continue;

            var slotImage = _inputSlots[i].Q<VisualElement>("CardImage");
            if (slotImage == null) continue;

            if (_inputCards[i] != null)
            {
                // [수정] Card.cs에 맞춰 CardImage 사용
                slotImage.style.backgroundImage = new StyleBackground(_inputCards[i].CardImage);
                slotImage.style.opacity = 1;
            }
            else
            {
                slotImage.style.backgroundImage = null;
                slotImage.style.opacity = 0;
            }
        }

        // 결과 슬롯 갱신
        if (_resultSlot == null) return;
        var resultImage = _resultSlot.Q<VisualElement>("CardImage");
        if (resultImage == null)
        {
            // 안전장치: UXML에 CardImage가 없을 경우 생성
            resultImage = new VisualElement();
            resultImage.name = "CardImage";
            resultImage.AddToClassList("card-image");
            _resultSlot.Add(resultImage);
        }

        if (_craftedResultCard != null)
        {
            // [수정] Card.cs에 맞춰 CardImage 사용
            resultImage.style.backgroundImage = new StyleBackground(_craftedResultCard.CardImage);
            resultImage.style.opacity = 1;
        }
        else
        {
            resultImage.style.backgroundImage = null;
            resultImage.style.opacity = 0;
        }
    }

    private void CheckRecipe()
    {
        List<string> currentInputIds = new List<string>();
        foreach (var card in _inputCards)
        {
            // [수정] Card.cs에는 ID 필드가 없으므로 CardNameKey를 식별자로 사용
            if (card != null) currentInputIds.Add(card.CardNameKey);
        }

        if (currentInputIds.Count < 2)
        {
            SetValidRecipe(null);
            return;
        }

        CraftingRecipe matchedRecipe = null;
        if (allRecipes != null)
        {
            foreach (var recipe in allRecipes)
            {
                // 레시피의 ingredientIDs와 현재 투입된 카드의 CardNameKey들을 비교
                if (AreIngredientsMatch(currentInputIds, recipe.ingredientIDs))
                {
                    matchedRecipe = recipe;
                    break;
                }
            }
        }

        SetValidRecipe(matchedRecipe);
    }

    private bool AreIngredientsMatch(List<string> inputs, List<string> requirements)
    {
        if (inputs.Count != requirements.Count) return false;

        // 순서 상관없이 구성 요소가 같은지 확인 (Dictionary 카운팅 방식)
        var inputCounts = inputs.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var reqCounts = requirements.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        return inputCounts.Count == reqCounts.Count && !inputCounts.Except(reqCounts).Any();
    }

    private void SetValidRecipe(CraftingRecipe recipe)
    {
        _currentValidRecipe = recipe;

        if (_craftButton == null || _resultNameLabel == null) return;

        if (_currentValidRecipe != null)
        {
            _craftButton.SetEnabled(true);
            _craftButton.RemoveFromClassList("disabled");

            // [참고] CardFactory의 구현 내용을 모르므로, 일단 GetCardData가 Card 객체를 반환한다고 가정하거나
            // 단순히 레시피에 있는 결과물 ID를 표시합니다.
            // Card resultData = CardFactory.GetCardData(_currentValidRecipe.resultCardID);

            // 임시: 레시피의 결과 ID를 그대로 표시 (실제로는 Localized string이 필요할 수 있음)
            _resultNameLabel.text = _currentValidRecipe.resultCardID;
        }
        else
        {
            _craftButton.SetEnabled(false);
            _craftButton.AddToClassList("disabled");
            _resultNameLabel.text = (_inputCards[0] != null && _inputCards[1] != null) ? "Unknown Recipe" : "Select Ingredients";
        }
    }

    private void OnCraftButtonClicked()
    {
        if (_currentValidRecipe == null) return;

        // 1. 재료 소모 (참조 제거)
        _inputCards[0] = null;
        _inputCards[1] = null;

        // 2. 결과물 카드 생성
        // [주의] CardFactory.CreateCard가 'Card' 객체를 반환해야 합니다.
        _craftedResultCard = CardFactory.CreateCard(_currentValidRecipe.resultCardID, null, -1);

        // 3. UI 갱신
        UpdateSlotVisuals();

        // 4. 상태 변경
        _craftButton.SetEnabled(false);
        _craftButton.AddToClassList("disabled");
        _resultNameLabel.text = "Crafted!";

        Debug.Log($"[Crafting] {_craftedResultCard.CardNameKey} 제작 완료!");
    }

    private void OnCloseButtonClicked()
    {
        ReturnIngredientsToInventory();

        if (_root != null) _root.RemoveFromHierarchy();

        // [수정] UIManager 에러 부분 주석 처리 (추후 구현 시 주석 해제)
        // UIManager.Instance.CloseCraftingUI(); 

        Debug.Log("제작 창 닫힘");

        // 이벤트 매니저가 있다면 상호작용 종료 알림
        if (EventInteractionManager.Instance != null)
            EventInteractionManager.Instance.CloseInteraction();
    }

    private void ReturnIngredientsToInventory()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_inputCards[i] != null)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.AddCardObject(_inputCards[i]);
                _inputCards[i] = null;
            }
        }

        if (_craftedResultCard != null)
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddCardObject(_craftedResultCard);
            _craftedResultCard = null;
        }
    }

    private void RegisterSlotCallbacks()
    {
        // 드래그 앤 드롭 구현 방식에 따라 이곳에 콜백을 등록합니다.
    }
}