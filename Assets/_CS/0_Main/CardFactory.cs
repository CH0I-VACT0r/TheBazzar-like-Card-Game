// 파일명: CardFactory.cs
using UnityEngine;

// '카드 ID' 문자열을 기반 실제 Card 객체 생성 데이터베이스
public static class CardFactory
{
    private static string[] allCardIDs =
    {
        "barbarian_warrior",
        "barbarian_shieldbearer",
        "manual_beginner"
        // "potion_hp", ...
    };

    public static Card CreateRandomCard(CardRarity rarity)
    {
        // (나중에 rarity에 맞는 ID만 추려서 뽑는 로직 추가 가능)
        string randomID = allCardIDs[Random.Range(0, allCardIDs.Length)];

        // 상점용 생성 (주인 없음)
        return CreateCard(randomID, null, -1);
    }

    //카드 ID, 주인, 슬롯을 받아 카드 생성
    public static Card CreateCard(string cardID, object owner, int index)
    {
        PlayerController playerOwner = owner as PlayerController;
        MonsterController monsterOwner = owner as MonsterController;

        switch (cardID)
        {
            // --- '재료' 카드들 --- (상점에 등장 X)
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

            // (나중에 추가...)
            // case "":
            //    if (playerOwner != null) return new ~~~ (playerOwner, index);
            //    break;


            // --- '제국의 성주' 카드들 ---


            // --- '진보의 성주' 카드들


            // --- '자연의 성주' 카드들 ---



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
            case "witch": // [신규!]
                if (monsterOwner != null)
                    return new Card_Witch(monsterOwner, index);
                break;
        }

        string ownerType = (owner != null) ? owner.GetType().Name : "null (Inventory/Shop)";
        Debug.LogError($"[CardFactory] ID를 찾을 수 없거나 생성 실패: {cardID} (요청자: {ownerType})");
        return null;
    }
}
