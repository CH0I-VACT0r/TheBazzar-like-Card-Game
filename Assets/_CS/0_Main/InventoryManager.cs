using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- 싱글톤 (어디서든 접근 가능하게) ---
    public static InventoryManager Instance;

    // --- 인벤토리 (카드 ID 문자열 리스트) ---
    public List<string> OwnedCardIDs = new List<string>(); // 전체 보유 카드 ID 목록 (저장용)
    public List<Card> mercenaryList = new List<Card>();    // 용병 리스트
    public List<Card> consumableList = new List<Card>();   // 소모품 리스트
    public List<Card> materialList = new List<Card>();     // 재료 리스트

    // --- 자원 ---
    public int Gold { get; private set; } = 15; // 초기 자금

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // [테스트] 시작 아이템 지급 (CardFactory에 있는 case 이름이어야 함!)
        // AddCard("merc_barbarian"); // 바바리안
    }

    // --- 기능 구현 ---

    // 아이템 획득
    public void AddCard(string cardID)
    {
        Card newCard = CardFactory.CreateCard(cardID, null, -1);

        if (newCard == null)
        {
            Debug.LogError($"[Inventory] 카드 생성 실패 (ID: {cardID}) - Factory를 확인하세요.");
            return;
        }

        // 타입에 따라 알맞은 리스트에 저장
        switch (newCard.ItemType)
        {
            case CardType.Mercenary:
                mercenaryList.Add(newCard);
                break;
            case CardType.Consumable:
                consumableList.Add(newCard);
                break;
            case CardType.Material:
                materialList.Add(newCard);
                break;
        }

        Debug.Log($"[Inven] {newCard.CardNameKey} 획득! (탭: {newCard.ItemType})");
    }

    public List<Card> GetListByType(CardType type)
    {
        switch (type)
        {
            case CardType.Mercenary: return mercenaryList;
            case CardType.Consumable: return consumableList;
            case CardType.Material: return materialList;
            default: return new List<Card>(); // 빈 리스트 반환
        }
    }

    public void RemoveCard(string cardID)
    {
        if (OwnedCardIDs.Contains(cardID))
        {
            OwnedCardIDs.Remove(cardID); // 리스트에서 하나만 제거됨
            Debug.Log($"[Inventory] 소모/판매: {cardID}");
        }
    }

    // 골드 변경 (획득/소모)
    public bool ModifyGold(int amount)
    {
        if (Gold + amount < 0) return false;
        Gold += amount;
        return true;
    }

    // 카드 객체 자체를 다시 인벤토리에 넣는 함수
    public void AddCardObject(Card card)
    {
        if (card == null) return;

        // 주인 정보 초기화
        card.Owner = null;
        card.SetSlotIndex(-1);

        switch (card.ItemType)
        {
            case CardType.Mercenary: mercenaryList.Add(card); break;
            case CardType.Consumable: consumableList.Add(card); break;
            case CardType.Material: materialList.Add(card); break;
        }
    }

    // 인덱스로 카드 가져오는 함수
    public Card GetCardAtIndex(CardType type, int index)
    {
        List<Card> list = GetListByType(type);
        if (list != null && index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return null;
    }

    // 리스트에서 카드 제거하는 함수 (장착/판매 시 사용)
    public void RemoveCard(Card card)
    {
        switch (card.ItemType)
        {
            case CardType.Mercenary: mercenaryList.Remove(card); break;
            case CardType.Consumable: consumableList.Remove(card); break;
            case CardType.Material: materialList.Remove(card); break;
        }
    }
}
