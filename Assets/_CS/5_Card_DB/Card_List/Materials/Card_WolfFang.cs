using UnityEngine;

public class Card_WolfFang : Card
{
    public Card_WolfFang(object owner, int index) : base(owner, index, 0f) // 쿨타임 0 (사용 안함)
    {
        this.CardNameKey = "card_wolffang_name"; // 흉포한 늑대 이빨
        this.ItemType = CardType.Material;
        this.CardImage = Resources.Load<Sprite>("CardImages/Materials/WolfFang");
        this.TagKeys.Add("tag_material");
        SetInitPrice(2);
        this.Rarity = CardRarity.Bronze;
        this.CardSkillDescriptionKey = "card_wolffang_desc";
        this.FlavorTextKey = "card_wolffang_flavor";
    }

    public override void ExecuteSkill()
    {
        // 재료는 전투 중 직접 사용하지 않으므로 빈칸
        Debug.Log("이것은 재료 아이템입니다.");
    }
}
