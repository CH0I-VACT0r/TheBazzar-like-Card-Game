using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("UI References")]
    public VisualTreeAsset craftingPageAsset; // Page_Crafting.uxml

    // UI 요소 캐싱
    private VisualElement _root;
    private VisualElement[] _inputSlots = new VisualElement[2];
    private VisualElement _resultSlot;
    private Label _resultNameLabel;
    private Button _craftButton;
    private Button _closeButton;
    private ScrollView _recipeListContainer;

    [Header("State")]
    private Card[] _inputCards = new Card[2];
    private Card _craftedResultCard = null;
    private CraftingRecipe _currentValidRecipe = null;

    private Event_Crafting _currentEvent;
    private bool _hasCraftedThisSession = false;

    [Header("Data")]
    public List<CraftingRecipe> allRecipes;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // [진단] 시작 시 데이터 체크
        if (allRecipes == null || allRecipes.Count == 0)
        {
            Debug.LogError(" [CraftingManager] 'All Recipes' 리스트가 비어있습니다! 인스펙터에서 레시피를 할당해주세요.");
        }
        else
        {
            Debug.Log($" [CraftingManager] 로드된 레시피 개수: {allRecipes.Count}");
        }
    }

    public void OpenCraftingUI(Event_Crafting evt)
    {
        if (craftingPageAsset == null)
        {
            Debug.LogError(" [Crafting] Page_Crafting.uxml이 연결되지 않았습니다!");
            return;
        }
        if (UIManager.Instance == null) return;

        _currentEvent = evt;
        _root = UIManager.Instance.OpenCraftingPage(craftingPageAsset);

        if (_root == null)
        {
            Debug.LogError("[Crafting] UI 생성 실패 (Root is null)");
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        // UI 찾기 & 디버깅
        _inputSlots[0] = _root.Q<VisualElement>("CraftInput_0");
        _inputSlots[1] = _root.Q<VisualElement>("CraftInput_1");
        _resultSlot = _root.Q<VisualElement>("ResultIcon");
        _recipeListContainer = _root.Q<ScrollView>("RecipeList");

        _craftButton = _root.Q<Button>("Btn_Craft");
        _closeButton = _root.Q<Button>("Btn_Close");

        if (_recipeListContainer == null) Debug.LogError("[Crafting] UI에서 'RecipeList' ScrollView를 찾지 못했습니다! UXML 이름을 확인하세요.");
        if (_resultSlot == null) Debug.LogError("[Crafting] UI에서 'ResultIcon'을 찾지 못했습니다!");

        // 데이터 초기화
        _inputCards[0] = null;
        _inputCards[1] = null;
        _craftedResultCard = null;
        _hasCraftedThisSession = false;
        _currentValidRecipe = null;

        PopulateRecipeList();
        UpdateSlotVisuals();
        CheckRecipe(); // 초기 상태 체크

        if (_craftButton != null) _craftButton.clicked += OnCraftButtonClicked;
        if (_closeButton != null) _closeButton.clicked += OnCloseButtonClicked;

        // 드래그 핸들러 부착
        VisualElement rootForDrag = UIManager.Instance.document.rootVisualElement;
        BattleManager bm = FindFirstObjectByType<BattleManager>();
        PlayerController pc = (bm != null) ? bm.playerController : null;

        if (pc != null && rootForDrag != null)
        {
            if (_inputSlots[0] != null) _inputSlots[0].AddManipulator(new DragAndDropHandler(_inputSlots[0], rootForDrag, pc));
            if (_inputSlots[1] != null) _inputSlots[1].AddManipulator(new DragAndDropHandler(_inputSlots[1], rootForDrag, pc));
            if (_resultSlot != null) _resultSlot.AddManipulator(new DragAndDropHandler(_resultSlot, rootForDrag, pc));
        }
    }

    private void PopulateRecipeList()
    {
        if (_recipeListContainer == null) return;
        _recipeListContainer.Clear();

        foreach (var recipe in allRecipes)
        {
            if (recipe == null) continue;

            VisualElement row = new VisualElement();
            row.AddToClassList("recipe-item");

            List<string> ingredientNames = new List<string>();
            foreach (string id in recipe.ingredientIDs)
            {
                string name = LocalizationManager.GetText(id + "_name");
                ingredientNames.Add(name);
            }

            // "재료1 + 재료2" 형태
            string formula = string.Join(" + ", ingredientNames);
            Label formulaLabel = new Label(formula);
            formulaLabel.AddToClassList("recipe-label");
            row.Add(formulaLabel);

            _recipeListContainer.Add(row);
        }
    }

    // --- 2. 외부 호출 함수 ---

    public bool TryDropCardOnSlot(int slotIndex, Card card)
    {
        if (_hasCraftedThisSession) return false;
        if (slotIndex < 0 || slotIndex >= 2) return false;
        if (_craftedResultCard != null) return false;

        // 유효성 검사
        if (_currentEvent != null)
        {
            if (!_currentEvent.IsValidCard(card, out string failReason))
            {
                Debug.LogWarning($" [Crafting] 카드 거부됨: {failReason}");
                return false;
            }
        }
        else
        {
            if (card.ItemType != CardType.Material) return false;
        }

        // 기존 카드 반환
        if (_inputCards[slotIndex] != null)
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddCardObject(_inputCards[slotIndex]);
        }

        _inputCards[slotIndex] = card;
        Debug.Log($"[Crafting] 슬롯 {slotIndex}에 카드 투입: {card.CardNameKey}");

        UpdateSlotVisuals();
        CheckRecipe();
        return true;
    }

    public Card TryRemoveCardFromSlot(int slotIndex)
    {
        if (_hasCraftedThisSession) return null;
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

        // UI 상태 업데이트
        if (_resultNameLabel != null)
            _resultNameLabel.text = _hasCraftedThisSession ? "Crafting Complete" : "Select Ingredients";

        if (_craftButton != null)
        {
            _craftButton.SetEnabled(false);
            _craftButton.AddToClassList("disabled");
        }

        return card;
    }

    // --- 3. 내부 로직 ---

    private void UpdateSlotVisuals()
    {
        // --- 1. 입력 슬롯 시각화 ---
        for (int i = 0; i < 2; i++)
        {
            if (_inputSlots[i] == null) continue;
            var slotImage = _inputSlots[i].Q<VisualElement>("CardImage");
            if (slotImage == null) continue;

            if (_inputCards[i] != null)
            {
                slotImage.style.backgroundImage = new StyleBackground(_inputCards[i].CardImage);
                slotImage.style.opacity = 1;
                _inputSlots[i].pickingMode = PickingMode.Position;
            }
            else
            {
                slotImage.style.backgroundImage = null;
                slotImage.style.opacity = 0;
                _inputSlots[i].pickingMode = PickingMode.Position;
            }
        }

        // --- 2. 결과 슬롯 시각화 (미리보기 및 툴팁 포함) ---
        if (_resultSlot != null)
        {
            VisualElement resultImage = _resultSlot.Q<VisualElement>("ResultCardImage");

            if (resultImage == null)
            {
                Debug.LogWarning("[Crafting] ResultIcon 내부에 CardImage가 없어 동적으로 생성합니다.");
                resultImage = new VisualElement();
                resultImage.name = "ResultCardImage";
                resultImage.style.width = Length.Percent(100);
                resultImage.style.height = Length.Percent(100);
                _resultSlot.Add(resultImage);
            }

            resultImage.style.display = DisplayStyle.Flex;
            resultImage.style.visibility = Visibility.Visible;

            // 기존 이벤트 해제
            _resultSlot.UnregisterCallback<PointerEnterEvent>(OnResultSlotPointerEnter);
            _resultSlot.UnregisterCallback<PointerLeaveEvent>(OnResultSlotPointerLeave);

            if (_craftedResultCard != null)
            {
                // [제작 완료]
                resultImage.style.backgroundImage = new StyleBackground(_craftedResultCard.CardImage);
                resultImage.style.opacity = 1f;
                resultImage.style.backgroundColor = Color.clear; // 배경 흰색이면 투명

                _resultSlot.userData = _craftedResultCard;
                _resultSlot.pickingMode = PickingMode.Position;
                _resultSlot.RegisterCallback<PointerEnterEvent>(OnResultSlotPointerEnter);
                _resultSlot.RegisterCallback<PointerLeaveEvent>(OnResultSlotPointerLeave);
            }
            else if (_currentValidRecipe != null && !_hasCraftedThisSession)
            {
                // [미리보기 상태]
                Card previewCard = CardFactory.CreateCard(_currentValidRecipe.resultCardID, null, -1);

                if (previewCard != null && previewCard.CardImage != null)
                {
                    // 이미지 입히기
                    resultImage.style.backgroundImage = new StyleBackground(previewCard.CardImage);
                    resultImage.style.opacity = 0.5f; // 반투명

                    resultImage.style.backgroundColor = new StyleColor(new Color(1, 0, 0, 0.5f));

                    _resultSlot.userData = previewCard;
                    _resultSlot.pickingMode = PickingMode.Position;

                    _resultSlot.RegisterCallback<PointerEnterEvent>(OnResultSlotPointerEnter);
                    _resultSlot.RegisterCallback<PointerLeaveEvent>(OnResultSlotPointerLeave);
                }
                else
                {
                    // 생성은 됐는데 Sprite가 없는 경우
                    if (previewCard != null && previewCard.CardImage == null)
                        Debug.LogError($"[Crafting] 카드 {previewCard.CardID}에 Sprite가 할당되지 않았습니다!");

                    resultImage.style.backgroundImage = null;
                    resultImage.style.opacity = 0;
                }
            }
            else
            {
                resultImage.style.backgroundImage = null;
                resultImage.style.opacity = 0;
                _resultSlot.userData = null;
                _resultSlot.pickingMode = PickingMode.Ignore;
            }
        }
    }

    private void CheckRecipe()
    {
        if (_hasCraftedThisSession) return;

        List<string> currentInputIds = new List<string>();
        foreach (var card in _inputCards)
        {
            if (card != null) currentInputIds.Add(card.CardID);
        }

        // 재료가 부족하면 검사 중단
        if (currentInputIds.Count < 2)
        {
            SetValidRecipe(null);
            return;
        }

        // [디버깅] 현재 투입된 재료 확인

        CraftingRecipe matchedRecipe = null;
        if (allRecipes != null)
        {
            foreach (var recipe in allRecipes)
            {
                if (recipe == null) continue;

                if (AreIngredientsMatch(currentInputIds, recipe.ingredientIDs))
                {
                    matchedRecipe = recipe;
                    Debug.Log($"[Crafting] 레시피 매칭 성공! -> {recipe.recipeNameKey} (Result: {recipe.resultCardID})");
                    break;
                }
            }
        }

        SetValidRecipe(matchedRecipe);
    }

    private bool AreIngredientsMatch(List<string> inputs, List<string> requirements)
    {
        if (inputs.Count != requirements.Count) return false;

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

            // 이름 미리보기
            Card previewCard = CardFactory.CreateCard(_currentValidRecipe.resultCardID, null, -1);
            if (previewCard != null)
                _resultNameLabel.text = previewCard.CardNameKey; // 실제 이름
            else
                _resultNameLabel.text = _currentValidRecipe.resultCardID; // ID (백업)

            UpdateSlotVisuals();
        }
        else
        {
            _craftButton.SetEnabled(false);
            _craftButton.AddToClassList("disabled");

            if (_inputCards[0] != null && _inputCards[1] != null)
                _resultNameLabel.text = "Invalid Recipe"; // 매칭 실패
            else
                _resultNameLabel.text = "Select Ingredients";

            UpdateSlotVisuals();
        }
    }

    private void OnCraftButtonClicked()
    {
        Debug.Log("[Crafting Debug] 제작 버튼 클릭됨");

        if (_currentValidRecipe == null)
        {
            Debug.LogError("에러: _currentValidRecipe가 null입니다!");
            return;
        }

        // 2. 결과물 생성 시도 및 확인
        string targetID = _currentValidRecipe.resultCardID;
        Debug.Log($"[Crafting Debug] 생성 시도 ID: {targetID}");

        _craftedResultCard = CardFactory.CreateCard(targetID, null, -1);

        if (_craftedResultCard == null)
        {
            Debug.LogError($"에러: CardFactory가 ID '{targetID}'에 해당하는 카드를 생성하지 못했습니다. Factory를 확인하세요!");
            return;
        }

        // 3. 나머지 로직 실행
        _inputCards[0] = null;
        _inputCards[1] = null;
        _hasCraftedThisSession = true;

        UpdateSlotVisuals();

        if (_craftButton != null)
        {
            _craftButton.SetEnabled(false);
            _craftButton.AddToClassList("disabled");
        }

        if (_resultNameLabel != null)
        {
            _resultNameLabel.text = "Success!";
        }

        Debug.Log($"[Crafting] 제작 완료: {_craftedResultCard.CardID}");
    }

    private void OnCloseButtonClicked()
    {
        ReturnIngredientsToInventory();

        if (_currentEvent != null) _currentEvent = null;

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

    private void OnResultSlotPointerEnter(PointerEnterEvent evt)
    {
        if (_resultSlot.userData is Card card)
        {
            UIManager.Instance.ShowCardTooltip(card, _resultSlot);
        }
    }

    private void OnResultSlotPointerLeave(PointerLeaveEvent evt)
    {
        UIManager.Instance.HideTooltip();
    }
}