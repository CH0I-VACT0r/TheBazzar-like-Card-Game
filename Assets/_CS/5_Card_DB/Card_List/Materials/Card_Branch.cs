using UnityEngine;

public class Card_Branch : Card
{
    public Card_Branch(object owner, int index) : base(owner, index, 0f, "card_branch")
    {
        this.CardNameKey = "card_branch_name"; // 나뭇가지
        this.ItemType = CardType.Material;
        this.CardImage = Resources.Load<Sprite>("CardImages/Materials/Branch");
        this.TagKeys.Add("tag_material");
        this.TagKeys.Add("tag_wood");
        SetInitPrice(2);
        this.Rarity = CardRarity.Bronze;
        this.CardSkillDescriptionKey = "card_branch_desc";
        this.FlavorTextKey = "card_branch_flavor";
    }

    public override void ExecuteSkill()
    {
        // 재료는 전투 중 사용 안 함
    }
}
