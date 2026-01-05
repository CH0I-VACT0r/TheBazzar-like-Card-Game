using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- 싱글톤 (어디서든 접근 가능하게) ---
    public static InventoryManager Instance;

    // --- 인벤토리 (카드 ID 문자열 리스트) ---
    [Header("Save Data")]
    public List<string> OwnedCardIDs = new List<string>(); // 전체 보유 카드 ID 목록 (저장용)
    public int Gold { get; private set; } = 15;

    [Header("Runtime Lists")]
    public List<Card> mercenaryList = new List<Card>();    // 용병 리스트
    public List<Card> consumableList = new List<Card>();   // 소모품 리스트
    public List<Card> materialList = new List<Card>();     // 재료 리스트

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
            LoadInventory(); // 시작 시 저장 데이터 불러오기
        }
        else { Destroy(gameObject); }
    }

    // --- 자원 관리 --- 
    public bool ModifyGold(int amount)
    {
        if (Gold + amount < 0)
        {
            Debug.LogWarning("잔액이 부족합니다!");
            return false;
        }

        Gold += amount;
        Debug.Log($"[Economy] 골드 변동: {amount}G (현재: {Gold}G)");

        // UI 업데이트 호출
        if (UIManager.Instance != null) UIManager.Instance.UpdateGoldUI(Gold);

        // 데이터 저장
        SaveInventory();
        return true;
    }

    // --- 기능 구현 ---
    // [기능 1] 카드 획득
    public void AddCard(string cardID)
    {
        Card newCard = CardFactory.CreateCard(cardID, null, -1);
        if (newCard == null) return;

        AddCardObject(newCard); // 런타임 리스트 추가

        OwnedCardIDs.Add(cardID);

        SaveInventory();
        Debug.Log($"[Inventory] 카드 획득: {cardID} (총 소유: {OwnedCardIDs.Count}장)");
    }

    // [기능 2] 카드 제거
    public void RemoveCardPermanently(Card card)
    {
        if (card == null) return;

        // 1. UI 리스트에서 제거
        RemoveCard(card);

        // 2. 마스터 명단(세이브 데이터)에서 제거
        if (OwnedCardIDs.Contains(card.CardID))
        {
            OwnedCardIDs.Remove(card.CardID);
        }

        SaveInventory(); // 변경사항 저장
    }

    // --- [기능 3] 저장  ---
    public void SaveInventory()
    {
        PlayerPrefs.SetInt("PlayerGold", Gold);

        // 리스트에 ID가 있을 때만 저장
        if (OwnedCardIDs != null && OwnedCardIDs.Count > 0)
        {
            string cardData = string.Join(",", OwnedCardIDs);
            PlayerPrefs.SetString("OwnedCards", cardData);
        }
        else
        {
            PlayerPrefs.SetString("OwnedCards", "");
        }

        PlayerPrefs.Save();
        // Debug.Log($"[Inventory] 데이터 저장 완료 (카드 {OwnedCardIDs.Count}장)");
    }

    // --- [기능 4] 불러오기 ---
    private void LoadInventory()
    {
        Gold = PlayerPrefs.GetInt("PlayerGold", 15);
        string cards = PlayerPrefs.GetString("OwnedCards", "");

        mercenaryList.Clear();
        consumableList.Clear();
        materialList.Clear();
        OwnedCardIDs.Clear();

        if (!string.IsNullOrEmpty(cards))
        {
            // 쉼표로 쪼갤 때 빈 칸 제거
            string[] idArray = cards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string id in idArray)
            {
                OwnedCardIDs.Add(id); // 명단 복구

                Card loadedCard = CardFactory.CreateCard(id, null, -1);
                if (loadedCard != null)
                {
                    AddCardInternal(loadedCard); // UI 리스트 분류
                }
            }
        }
        Debug.Log($"[Inventory] 로드 완료: {Gold}G, 카드 {OwnedCardIDs.Count}장 복원됨.");
    }

    // 내부 분류용 헬퍼 함수 (AddCardObject와 유사하지만 저장 로직 제외용)
    private void AddCardInternal(Card card)
    {
        switch (card.ItemType)
        {
            case CardType.Mercenary: mercenaryList.Add(card); break;
            case CardType.Consumable: consumableList.Add(card); break;
            case CardType.Material: materialList.Add(card); break;
        }
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

    public void RemoveCard(Card card)
    {
        if (card == null) return;

        switch (card.ItemType)
        {
            case CardType.Mercenary: mercenaryList.Remove(card); break;
            case CardType.Consumable: consumableList.Remove(card); break;
            case CardType.Material: materialList.Remove(card); break;
        }
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

    public Card PullCardFromInventory(string cardID)
    {
        List<Card> targetList = null;
        Card foundCard = null;

        // 용병 리스트 확인
        foundCard = mercenaryList.Find(c => c.CardID == cardID);
        if (foundCard != null) targetList = mercenaryList;

        // 못 찾았다면 소모품 리스트 확인
        if (foundCard == null)
        {
            foundCard = consumableList.Find(c => c.CardID == cardID);
            if (foundCard != null) targetList = consumableList;
        }

        // 못 찾았다면 재료 리스트 확인
        if (foundCard == null)
        {
            foundCard = materialList.Find(c => c.CardID == cardID);
            if (foundCard != null) targetList = materialList;
        }

        // 객체를 찾았다면 인벤토리 '런타임 리스트'에서 제거 후 반환
        if (foundCard != null && targetList != null)
        {
            targetList.Remove(foundCard);
            return foundCard;
        }

        return null; // 인벤토리에 해당 카드가 없음
    }
}
