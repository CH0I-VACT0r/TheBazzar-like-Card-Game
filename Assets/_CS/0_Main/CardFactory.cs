using UnityEngine;
using System.Collections.Generic;

public static class CardFactory
{
    private static string[] allCardIDs =
    {
        "card_barbarian_warrior",
        "card_barbarian_shieldbearer",
        "card_manual_beginner",
        "card_potion_heal",
        "card_icewolf",
        "card_frozenknight"
    };

    private static Dictionary<string, Card> _prototypeCache;

    private static void InitializeCache()
    {
        if (_prototypeCache != null) return;

        _prototypeCache = new Dictionary<string, Card>();
        foreach (string id in allCardIDs)
        {
            // 견본 카드 생성 (데이터 검사용)
            Card proto = CreateCard(id, null, -1);
            if (proto != null)
            {
                _prototypeCache[id] = proto;
            }
        }
        Debug.Log($"[CardFactory] 카드 견본 {_prototypeCache.Count}개 캐싱 완료.");
    }

    public static Card CreateRandomCard(CardRarity rarity)
    {
        string randomID = allCardIDs[Random.Range(0, allCardIDs.Length)];
        return CreateCard(randomID, null, -1);
    }

    // 필터링된 랜덤 카드 생성 함수
    public static Card CreateCardByFilter(CardRarity targetRarity, CardType targetType, string requiredTag, LordType lord, MonsterController owner)
    {
        InitializeCache();

        // 조건을 만족하는 카드 ID들을 담을 리스트 (string 리스트)
        List<string> candidateIDs = new List<string>();

        // 캐시된 견본품들을 순회하며 필터링
        foreach (var kvp in _prototypeCache)
        {
            string id = kvp.Key;
            Card proto = kvp.Value;

            // 등급 체크 (targetRarity가 0이 아니면 체크)
            if ((int)targetRarity != 0 && proto.Rarity != targetRarity) continue;

            // 타입 체크 (targetType이 0이 아니면 체크)
            if ((int)targetType != 0 && proto.ItemType != targetType) continue;

            // 태그 체크
            if (!string.IsNullOrEmpty(requiredTag) && !proto.HasTagKey(requiredTag)) continue;

            // 영주 필터링 (공용이거나 현재 영주 소속이어야 함)
            // 사용자 정보: OwnerLord 프로퍼티 사용
            if (proto.OwnerLord != LordType.Common && proto.OwnerLord != lord) continue;

            // 모든 조건 만족 시 후보 추가
            candidateIDs.Add(id);
        }

        // 랜덤 선택
        if (candidateIDs.Count > 0)
        {
            string pickedID = candidateIDs[Random.Range(0, candidateIDs.Count)];
            return CreateCard(pickedID, owner, -1);
        }

        return null;
    }

    public static Card CreateCard(string cardID, object owner, int index)
    {
        MonsterController monsterOwner = owner as MonsterController;

        switch (cardID)
        {
            // --- 제작 및 재료 아이템 (상점 미등장) ---
            case "card_slime_jelly": return new Card_SlimeJelly(owner, index);
            case "card_wolffang": return new Card_WolfFang(owner, index);
            case "card_branch": return new Card_Branch(owner, index);
            case "card_wolf_dagger": return new Card_WolfDagger(owner, index);

            // --- '아이템' 카드들 --- 
            case "card_potion_heal": return new Card_Potion_Heal(owner, index);
            case "card_torn_book": return new Card_Torn_Book(owner, index);

            // --- '중립' 카드들 ---
            case "card_manual_beginner": return new Card_Manual_Beginner(owner, index);

            // --- '혹한의 성주' 카드들 ---
            case "card_barbarian_warrior": return new Card_BarbarianWarrior(owner, index);
            case "card_barbarian_shieldbearer": return new Card_BarbarianShieldbearer(owner, index);
            case "card_icewolf": return new Card_IceWolf(owner, index);
            case "card_frozenknight": return new Card_FrozenKnight(owner, index);

            // --- '몬스터' 카드들 ---
            case "card_sheep": return new Card_Sheep(owner, index);

            case "card_slime_green":
                if (monsterOwner != null) return new Card_Slime_Green(monsterOwner, index);
                break;
            case "card_slime_yellow":
                if (monsterOwner != null) return new Card_Slime_Yellow(monsterOwner, index);
                break;
            case "card_slime_red":
                if (monsterOwner != null) return new Card_Slime_Red(monsterOwner, index);
                break;
            case "card_slime_purple":
                if (monsterOwner != null) return new Card_Slime_Purple(monsterOwner, index);
                break;

            case "card_goblin":
                if (monsterOwner != null) return new Card_Goblin(monsterOwner, index);
                break;
            case "card_witch":
                if (monsterOwner != null) return new Card_Witch(monsterOwner, index);
                break;
            
        }

        return null;
    }
}