using UnityEngine;

public class Card_WolfDagger : Card
{
    // 생성자: 쿨타임 5초 설정
    public Card_WolfDagger(object owner, int index) : base(owner, index, 5.0f)
    {
        this.CardNameKey = "card_wolf_dagger_name"; // 늑대 이빨 단검
        this.ItemType = CardType.Consumable;   // 소비 아이템으로 분류 (장비처럼 쓰지만 소모성)
        this.CardImage = Resources.Load<Sprite>("CardImages/Consumable/Weapon/WolfDagger");
        this.Durability = 5;       // 내구도 5
        this.BleedStacksToApply = 5; // 툴팁 표기용 (실제 적용은 ExecuteSkill에서)
        this.CardSkillDescriptionKey = "card_wolfdagger_skill_desc"; // "적에게 출혈 5를 부여합니다."
        this.FlavorTextKey = "card_wolfdagger_flavor";
        SetInitPrice(2);
        this.Rarity = CardRarity.Bronze;
    }

    public override void ExecuteSkill()
    {
        // 1. 플레이어가 사용한 경우
        if (Owner is PlayerController player)
        {
            BattleManager bm = Object.FindFirstObjectByType<BattleManager>();
            if (bm != null && bm.monsterController != null)
            {
                // 몬스터에게 출혈 5 부여 (5초 지속 or 5스택)
                // CardData의 BleedStacksToApply 값을 사용하거나 하드코딩
                bm.monsterController.ApplyLordStatus(StatusEffectType.Bleed, this.BleedStacksToApply);

                Debug.Log($"[WolfDagger] 몬스터에게 출혈 {this.BleedStacksToApply} 부여!");
            }
        }
    }
}