using UnityEngine;

public class Card_Torn_Book : Card
{
    public Card_Torn_Book(object owner, int index) : base(owner, index, 0f, "card_torn_book")
    {
        // 1. 기본 정보
        this.CardNameKey = "card_torn_book_name";
        this.CardSkillDescriptionKey = "card_torn_book_desc";
        this.FlavorTextKey = "card_torn_book_flavor";

        // 2. 이미지 (없으면 기본 이미지라도)
        this.CardImage = Resources.Load<Sprite>("CardImages/Materials/Torn_Book");

        // 3. 타입: 재료 (Material) / 가격: 0원
        this.ItemType = CardType.Material;
        this.Rarity = CardRarity.Bronze;
        this.SetInitPrice(0);

        // 4. 스킬 쿨타임 UI 끄기 (아무 기능 없으므로)
        this.ShowCooldownUI = false;
    }

    public override void ExecuteSkill()
    {
        // 아무 효과 없음
    }
}
