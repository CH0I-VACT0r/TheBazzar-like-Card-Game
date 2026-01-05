using UnityEngine;

public class Card_Slime_Red : Card
{
    public Card_Slime_Red(object owner, int index) : base(owner, index, 6.0f, "card_slime_red")
    {
        this.CardNameKey = "card_slime_red_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Red_Slime");
        SetInitPrice(0);
        this.Rarity = CardRarity.Bronze;

        this.TagKeys.Add("tag_monster");
        this.TagKeys.Add("tag_slime");

        this.CardSkillDescriptionKey = "card_slime_red_skill_desc";
        this.FlavorTextKey = "card_slime_red_flavor";

        this.HasQuest = false;
        this.Durability = -1;

        this.BaseDamage = 5f;
        this.BaseCritChance = 0.1f;
    }

    public override void ExecuteSkill()
    {
        float realDamage = GetCurrentDamage() * CheckForCrit();
        MonsterController monsterOwner = Owner as MonsterController;

        if (monsterOwner != null)
        {
            monsterOwner.GetTarget()?.TakeDamage(realDamage);
        }
    }

    public override float GetCurrentDamage() => this.BaseDamage;
    public override float GetCurrentCritChance() => this.BaseCritChance;
}
