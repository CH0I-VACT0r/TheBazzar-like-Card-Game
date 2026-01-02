using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private Event_Shop _currentShopData;
    private List<Card> _currentStock = new List<Card>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 상점 열기
    public void OpenShop(Event_Shop shopData)
    {
        _currentShopData = shopData;

        // 상점에 입장하면 자동으로 리롤 1회 (무료)
        RerollStock(true);

        // UI 열기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowShopUI(shopData, _currentStock);
        }
    }

    // 상점 닫기
    public void CloseShop()
    {
        _currentStock.Clear();
        _currentShopData = null;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNextDay();
        }
    }

    // 리롤 (상품 갱신)
    public void RerollStock(bool isFree = false)
    {
        PlayerController player = null;
        if (BattleManager.Instance != null) player = BattleManager.Instance.playerController;

        // 돈 체크 (무료가 아니면)
        if (!isFree && player != null)
        {
            if (player.Gold < _currentShopData.rerollPrice)
            {
                Debug.Log("골드가 부족합니다!");
                return;
            }
            player.SpendGold(_currentShopData.rerollPrice);
        }

        _currentStock.Clear();
        List<string> pickedCardNames = new List<string>(); // 중복 방지용

        for (int i = 0; i < 3; i++)
        {
            Card newCard = null;

            // [중복 방지 루프]
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // 1. 등급 결정
                CardRarity rarity;

                // 이벤트 파일에 특정 등급이 지정되어 있다면 강제 적용 (None/0이 아니면)
                // (주의: CardRarity Enum에 None이 없다면 (CardRarity)0 으로 비교)
                if ((int)_currentShopData.targetRarity != 0)
                {
                    rarity = _currentShopData.targetRarity;
                }
                else
                {
                    // 아니면 레벨 비례 랜덤
                    rarity = GameManager.Instance != null ?
                             GameManager.Instance.GetRandomRarityByProgression() : CardRarity.Bronze;
                }

                // 2. [핵심 수정] 필터링된 카드 생성 요청
                // Event_Shop에 있는 targetType, requiredTag 정보를 넘겨줍니다.
                Card candidate = CardFactory.CreateCardByFilter(
                    rarity,
                    _currentShopData.targetType,
                    _currentShopData.requiredTag,
                    null // 상점 진열품은 아직 주인이 없음
                );

                if (candidate != null)
                {
                    // 일단 후보로 등록 (실패 시 마지막 후보라도 쓰기 위함)
                    if (newCard == null) newCard = candidate;

                    // 중복 체크
                    if (!pickedCardNames.Contains(candidate.CardNameKey))
                    {
                        newCard = candidate;
                        pickedCardNames.Add(newCard.CardNameKey);
                        break; // 성공!
                    }
                }
            }

            // 3. 가격 변동 적용 및 진열
            if (newCard != null)
            {
                int finalPrice = (int)(newCard.BasePrice * _currentShopData.priceMultiplier);
                newCard.SetInitPrice(finalPrice);
                _currentStock.Add(newCard);
            }
            else
            {
                // 조건에 맞는 카드가 아예 없으면 품절 처리
                _currentStock.Add(null);
            }
        }

        // UI 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateShopSlots(_currentStock);
        }
    }

    // 아이템 구매 시도 (UI에서 호출)
    public void TryBuyItem(int slotIndex)
    {
        if (_currentStock == null || slotIndex < 0 || slotIndex >= _currentStock.Count) return;

        Card item = _currentStock[slotIndex];
        if (item == null) return; // 이미 팔림

        PlayerController player = null;
        if (BattleManager.Instance != null) player = BattleManager.Instance.playerController;

        if (player != null)
        {
            int price = item.GetCurrentPrice();
            if (player.Gold >= price)
            {
                // 구매 성공!

                // 1. 빈 슬롯 확인 (파티 -> 인벤 순)
                int emptySlot = player.GetFirstEmptyPartySlot();

                if (emptySlot != -1)
                {
                    // 파티 창으로 즉시 영입
                    player.SpendGold(price);
                    player.EquipCardDirectly(item, emptySlot);

                    // 재고에서 제거 (품절 처리)
                    _currentStock[slotIndex] = null;
                    UIManager.Instance.UpdateShopSlots(_currentStock);

                    Debug.Log($"[Shop] {item.CardNameKey} 구매 완료 (파티 합류)");
                }
                else if (InventoryManager.Instance != null)
                {
                    // 인벤토리로 보내기
                    player.SpendGold(price);
                    InventoryManager.Instance.AddCardObject(item);

                    // 재고에서 제거
                    _currentStock[slotIndex] = null;
                    UIManager.Instance.UpdateShopSlots(_currentStock);

                    Debug.Log($"[Shop] {item.CardNameKey} 구매 완료 (인벤토리 이동)");
                }
                else
                {
                    Debug.Log("가방이 가득 찼습니다!"); // (UI 메시지 띄우기)
                }
            }
            else
            {
                Debug.Log("골드가 부족합니다."); // (UI 메시지 띄우기)
            }
        }
    }
}