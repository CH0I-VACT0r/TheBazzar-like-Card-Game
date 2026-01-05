using UnityEngine;

public class Card_SlimeJelly : Card
{
    public Card_SlimeJelly(object owner, int index) : base(owner, index, 0f, "card_slime_jelly")
    {
        this.CardNameKey = "card_slime_jelly_name"; // 슬라임
        this.ItemType = CardType.Material;
        this.CardImage = Resources.Load<Sprite>("CardImages/Materials/Slime_Jelly");
        this.TagKeys.Add("tag_material");
        SetInitPrice(2);
        this.Rarity = CardRarity.Bronze;
        this.CardSkillDescriptionKey = "card_slime_jelly_desc";
        this.FlavorTextKey = "card_slime_jelly_flavor";
    }

    public override void ExecuteSkill()
    {
        // 재료는 전투 중 사용 안 함
    }
}