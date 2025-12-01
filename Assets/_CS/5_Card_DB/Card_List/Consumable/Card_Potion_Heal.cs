using UnityEngine;

public class Card_Potion_Heal : Card
{
    public Card_Potion_Heal(object owner, int index) : base(owner, index, 7.0f)
    {
        // 1. 기본 정보
        this.CardNameKey = "card_potion_heal_name";
        this.CardSkillDescriptionKey = "card_potion_heal_desc";
        this.FlavorTextKey = "card_potion_heal_flavor";

        // 2. 이미지 (없으면 기본 이미지라도)
        this.CardImage = Resources.Load<Sprite>("CardImages/Consumable/Potion_Heal");

        // 3. 타입: 아이템 (Item) / 가격: 0원
        this.ItemType = CardType.Consumable;
        this.Rarity = CardRarity.Bronze;
        this.SetInitPrice(2);
        this.OwnerLord = LordType.Common;    // 중립
        this.TagKeys.Add("tag_potion");

        //내구도 : 3회에 걸쳐서 회복
        this.Durability = 3;
    }

    public override void ExecuteSkill()
    {
        // 2. 주인(Owner)이 플레이어인지 몬스터인지 확인
        PlayerController playerOwner = Owner as PlayerController;
        MonsterController monsterOwner = Owner as MonsterController;

        // 3. 타겟에게 피해 입히기
        if (playerOwner != null)
        {
            // 내가 플레이어 소속이면, 타겟(몬스터)을 공격
            playerOwner.AddHealth(10.0f);
        }
        else if (monsterOwner != null)
        {
            // 내가 몬스터 소속이면, 타겟(플레이어)을 공격
            monsterOwner.AddHealth(10.0f);
        }
    }
}
