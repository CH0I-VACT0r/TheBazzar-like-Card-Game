using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private Event_Shop _currentShopData;
    private List<Card> _currentStock = new List<Card>(); // 현재 매물 (팔리면 null로 바뀜)

    // [테스트용] 상점에 등장할 수 있는 카드 ID 목록 (나중엔 팩토리에서 랜덤으로 가져옴)
    private string[] _testItemIDs = { "barbarian_warrior", "barbarian_shieldbearer", "potion_hp" };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. 상점 열기
    public void OpenShop(Event_Shop shopData)
    {
        _currentShopData = shopData;
        Debug.Log($"[Shop] {shopData.eventID} 오픈!");

        // 첫 오픈 시 무료 리롤(물건 채우기)
        RerollStock(true);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowShopUI(_currentShopData, _currentStock);
        }
    }

    // 2. 상점 닫기
    public void CloseShop()
    {
        // 다음 날로 넘어가거나, 다시 이벤트 선택창으로 가거나...
        // 기획상 '다음' 버튼 누르면 하루 종료라면:
        // FindFirstObjectByType<GameManager>().SetPhase(GameManager.GamePhase.DayEnd);

        // 일단은 몬스터 화면(기본화면)으로 복귀한다고 가정
        UIManager.Instance.SwitchToBattlePage();
    }

    // 3. 리롤 (물건 새로고침)
    public void RerollStock(bool isFree = false)
    {
        PlayerController player = null;

        // PlayerController 찾기 (MonoBehaviour가 아니라면 BattleManager 경유)
        if (BattleManager.Instance != null)
            player = BattleManager.Instance.playerController;

        // 돈 체크 (공짜가 아니면)
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

        List<string> pickedCardNames = new List<string>();

        // 슬롯 3개 채우기
        for (int i = 0; i < 3; i++)
        {
            Card newCard = null;

            // [중복 방지 루프]
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // 1. 등급 결정 & 생성
                CardRarity rarity = GameManager.Instance != null ?
                                    GameManager.Instance.GetRandomRarityByLevel() : CardRarity.Bronze;

                Card candidate = CardFactory.CreateRandomCard(rarity);

                if (candidate != null)
                {
                    // [핵심 수정] 일단 후보로 등록해둡니다! (중복이라도 비어있는 것보단 나으니까)
                    // 이렇게 하면 10번 다 실패해도 마지막에 뽑힌 중복 카드가 들어갑니다.
                    if (newCard == null) newCard = candidate;

                    // 2. 중복 체크
                    if (!pickedCardNames.Contains(candidate.CardNameKey))
                    {
                        // 와! 중복 아님! 이걸로 확정!
                        newCard = candidate;
                        pickedCardNames.Add(newCard.CardNameKey);
                        break; // 루프 탈출
                    }
                }
            }

            // 3. 최종적으로 결정된 카드(유니크하거나, 정 없으면 중복된 것)를 진열
            if (newCard != null)
            {
                int finalPrice = (int)(newCard.BasePrice * _currentShopData.priceMultiplier);
                newCard.SetInitPrice(finalPrice);
                _currentStock.Add(newCard);
            }
            else
            {
                // 팩토리에서조차 카드를 못 만들었을 때만 null (이건 진짜 에러 상황)
                _currentStock.Add(null);
            }
        }

        // UI 갱신
        UIManager.Instance.UpdateShopSlots(_currentStock);
    }

    // 4. 구매 시도
    public void TryBuyItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _currentStock.Count) return;
        Card item = _currentStock[slotIndex];

        if (item == null)
        {
            Debug.Log("이미 팔린 상품입니다.");
            return;
        }

        PlayerController player = null;
        if (BattleManager.Instance != null) player = BattleManager.Instance.playerController;

        if (player == null) return;

        // 돈 체크
        if (player.Gold < item.GetCurrentPrice())
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        // 구매 진행
        // 1. 돈 차감
        player.SpendGold(item.GetCurrentPrice());

        // 2. 물건 주기 (파티 우선 -> 인벤토리)
        bool equippedToParty = false;

        if (item.ItemType == CardType.Mercenary)
        {
            int emptySlot = player.GetFirstEmptyPartySlot();
            if (emptySlot != -1)
            {
                player.EquipCardDirectly(item, emptySlot);
                equippedToParty = true;
                Debug.Log($"[Shop] 파티 슬롯 {emptySlot}에 장착됨");
            }
        }

        if (!equippedToParty)
        {
            InventoryManager.Instance.AddCardObject(item);
            Debug.Log($"[Shop] 인벤토리로 이동됨");
        }

        // 3. 품절 처리
        _currentStock[slotIndex] = null; // 리스트에서 비움
        UIManager.Instance.UpdateShopSlots(_currentStock); // UI 갱신 (SOLD 표시)
    }
}