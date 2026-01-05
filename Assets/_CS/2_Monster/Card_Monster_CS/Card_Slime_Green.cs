using UnityEngine;

public class Card_Slime_Green : Card
{
    public Card_Slime_Green(object owner, int index) : base(owner, index, 8.0f, "card_slime_green")
    {
        this.CardNameKey = "card_slime_green_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Green_Slime");
        SetInitPrice(0);
        this.Rarity = CardRarity.Bronze;

        this.TagKeys.Add("tag_monster");
        this.TagKeys.Add("tag_slime");

        this.CardSkillDescriptionKey = "card_slime_green_skill_desc";
        this.FlavorTextKey = "card_slime_green_flavor";

        this.HasQuest = false;
        this.Durability = -1;

        this.BaseHeal = 5f; // 기본 회복량 5
        this.BaseCritChance = 0.1f; // 치명타 10%
    }

    public override void ExecuteSkill()
    {
        float realHeal = GetCurrentHeal() * CheckForCrit(); ;
        MonsterController monsterOwner = Owner as MonsterController;

        if (monsterOwner != null)
        {
            monsterOwner.AddHealth(realHeal); // 본체 회복
        }
    }

    public override float GetCurrentHeal()
    {
        return this.BaseHeal;
    }
    public override float GetCurrentCritChance()
    {
        return this.BaseCritChance;
    }
}