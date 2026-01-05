using UnityEngine;

[CardConfig(
    "card_wolf_dagger",
    NameEn = "Wolf Dagger",
    NameKo = "늑대 단검",
    DescEn = "Deals damage and applies bleed.",
    DescKo = "적에게 출혈을 부여합니다.",
    FlavorEn = "Sharp dagger made of wolf fang.",
    FlavorKo = "늑대 이빨로 만든 날카로운 단검입니다."
)]
public class Card_WolfDagger : Card
{
    // 효과: 출혈 5 스택 부여
    public Card_WolfDagger(object owner, int index) : base(owner, index, 5.0f)
    {
        // 1. NameKey / DescKey / FlavorKey are now auto-set by the base Card class using the Attribute above.
        
        this.ItemType = CardType.Consumable;   // 소모품 및 재료 분류 (전투 중 소모됨)
        this.CardImage = Resources.Load<Sprite>("CardImages/Consumable/Weapon/WolfDagger");
        this.Durability = 5;       // 내구도 5
        this.BleedStacksToApply = 5; // 출혈 5스택 (스킬에서 사용)
        
        SetInitPrice(2);
        this.Rarity = CardRarity.Bronze;
    }

    public override void ExecuteSkill()
    {
        // 1. 플레이어가 사용 시
        if (Owner is PlayerController player)
        {
            BattleManager bm = Object.FindFirstObjectByType<BattleManager>();
            if (bm != null && bm.monsterController != null)
            {
                // 적에게 출혈 5 부여
                bm.monsterController.ApplyLordStatus(StatusEffectType.Bleed, this.BleedStacksToApply);

                Debug.Log($"[WolfDagger] 적에게 출혈 {this.BleedStacksToApply} 부여!");
            }
        }
    }
}