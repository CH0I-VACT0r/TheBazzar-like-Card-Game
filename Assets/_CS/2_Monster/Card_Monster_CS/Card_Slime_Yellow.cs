using UnityEngine;

public class Card_Slime_Yellow : Card
{
    public Card_Slime_Yellow(object owner, int index) : base(owner, index, 8.0f, "card_slime_yellow")
    {
        this.CardNameKey = "card_slime_yellow_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Yellow_Slime");
        SetInitPrice(0);
        this.Rarity = CardRarity.Bronze;

        this.TagKeys.Add("tag_monster");
        this.TagKeys.Add("tag_slime");

        this.CardSkillDescriptionKey = "card_slime_yellow_skill_desc";
        this.FlavorTextKey = "card_slime_yellow_flavor";

        this.HasQuest = false;
        this.Durability = -1;

        this.BaseShield = 10f; // 기본 보호막 5
        this.BaseCritChance = 0.1f;
    }

    public override void ExecuteSkill()
    {
        // 보호막도 치명타가 터지면 더 두꺼워짐!
        float realShield = GetCurrentShield() * CheckForCrit();
        MonsterController monsterOwner = Owner as MonsterController;

        if (monsterOwner != null)
        {
            monsterOwner.AddShield(realShield);
        }
    }

    public override float GetCurrentShield() => this.BaseShield;
    public override float GetCurrentCritChance() => this.BaseCritChance;
}