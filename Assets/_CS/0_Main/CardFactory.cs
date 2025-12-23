using UnityEngine;
using System.Collections.Generic;

// '카드 ID' 문자열을 기반 실제 Card 객체 생성 데이터베이스
public static class CardFactory
{
    // [중요] 여기에 등록된 ID들만 상점이나 랜덤 보상에서 등장합니다.
    // 제작 전용 아이템(재료, 결과물)은 여기에 적지 않음으로써 상점 등장을 막습니다.
    private static string[] allCardIDs =
    {
        "barbarian_warrior",
        "barbarian_shieldbearer",
        "manual_beginner",
        "potion_heal"
        // "item_wolf_fang", "item_branch", "item_wolf_dagger"는 여기에 넣지 않습니다!
    };

    private static Dictionary<string, Card> _prototypeCache;

    //캐시 초기화 함수
    private static void InitializeCache()
    {
        if (_prototypeCache != null) return;

        _prototypeCache = new Dictionary<string, Card>();

        foreach (string id in allCardIDs)
        {
            // 주인 없는(null) 견본 카드를 생성
            Card proto = CreateCard(id, null, -1);
            if (proto != null)
            {
                _prototypeCache[id] = proto;
            }
        }

        Debug.Log($"[CardFactory] 카드 견본 {_prototypeCache.Count}개 캐싱 완료.");
    }

    // 랜덤 생성 (등급 고려)
    public static Card CreateRandomCard(CardRarity rarity)
    {
        // 단순 랜덤이 아니라 필터링을 거치는 게 안전합니다.
        // (여기서는 기존처럼 배열에서 뽑되, 나중에 최적화 가능)
        string randomID = allCardIDs[Random.Range(0, allCardIDs.Length)];
        return CreateCard(randomID, null, -1);
    }

    // [최적화] 필터링된 랜덤 카드 생성 함수 (Dictionary 캐시 사용)
    public static Card CreateCardByFilter(CardRarity targetRarity, CardType targetType, string requiredTag, object owner)
    {
        // 1. 캐시가 없으면 생성
        InitializeCache();

        List<string> candidateIDs = new List<string>();

        // 2. 미리 만들어둔 견본품(Prototype)들을 순회 (메모리 할당 X)
        foreach (var kvp in _prototypeCache)
        {
            string id = kvp.Key;
            Card proto = kvp.Value;

            // A. 등급 체크
            if ((int)targetRarity != 0 && proto.Rarity != targetRarity) continue;

            // B. 타입 체크 (CardType.None이 0이라고 가정하거나, None 체크)
            if ((int)targetType != 0 && proto.ItemType != targetType) continue;

            // C. 태그 체크
            if (!string.IsNullOrEmpty(requiredTag) && !proto.HasTagKey(requiredTag)) continue;

            // 조건 만족!
            candidateIDs.Add(id);
        }

        // 3. 후보 중에서 랜덤 선택 후 '진짜' 카드 생성
        if (candidateIDs.Count > 0)
        {
            string pickedID = candidateIDs[Random.Range(0, candidateIDs.Count)];
            return CreateCard(pickedID, owner, -1);
        }

        // Debug.LogWarning($"[CardFactory] 조건에 맞는 카드가 없습니다! (Rarity: {targetRarity}, Type: {targetType}, Tag: {requiredTag})");
        return null;
    }

    //카드 ID, 주인, 슬롯을 받아 카드 생성
    // [중요] 여기에 케이스를 추가해야 제작 시스템이나 전리품 시스템에서 해당 카드를 만들 수 있습니다.
    public static Card CreateCard(string cardID, object owner, int index)
    {
        PlayerController playerOwner = owner as PlayerController;
        MonsterController monsterOwner = owner as MonsterController;

        switch (cardID)
        {
            // --- 제작 및 재료 아이템 (상점 미등장) ---
            case "card_wolffang":
                return new Card_WolfFang(owner, index);

            case "card_branch":
                return new Card_Branch(owner, index);

            case "card_wolf_dagger":
                return new Card_WolfDagger(owner, index);


            // --- '아이템' 카드들 --- 
            case "potion_heal":
                return new Card_Potion_Heal(owner, index);

            // --- '재료' 카드들 ---
            case "card_torn_book":
                return new Card_Torn_Book(owner, index);

            // --- '중립' 카드들 ---
            case "manual_beginner":
                return new Card_Manual_Beginner(owner, index);


            // --- '혹한의 성주' 카드들 ---
            case "barbarian_warrior":
                return new Card_BarbarianWarrior(owner, index);

            case "barbarian_shieldbearer":
                return new Card_BarbarianShieldbearer(owner, index);
            case "ice_wolf":
                return new Card_IceWolf(owner, index);
            case "frozen_knight":
                return new Card_FrozenKnight(owner, index);
       
            // ---다른 성주 카드들 ---

            // --- '몬스터' 카드들 ---
            // 양
            case "card_sheep":
                return new Card_Sheep(owner, index);
            //고블린
            case "goblin":
                // 몬스터 전용 카드이므로, 주인이 몬스터일 때만 생성
                if (monsterOwner != null)
                    return new Card_Goblin(monsterOwner, index);
                break;
            // 마녀
            case "witch":
                if (monsterOwner != null)
                    return new Card_Witch(monsterOwner, index);
                break;
        }

        string ownerType = (owner != null) ? owner.GetType().Name : "null (Inventory/Shop)";
        Debug.LogError($"[CardFactory] ID를 찾을 수 없거나 생성 실패: {cardID} (요청자: {ownerType})");
        return null;
    }
}