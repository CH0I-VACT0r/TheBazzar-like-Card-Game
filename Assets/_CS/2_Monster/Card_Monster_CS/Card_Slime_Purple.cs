using UnityEngine;

public class Card_Slime_Purple : Card
{
    public Card_Slime_Purple(object owner, int index) : base(owner, index, 10.0f, "card_slime_purple")
    {
        this.CardNameKey = "card_slime_purple_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Purple_Slime");
        SetInitPrice(0);
        this.Rarity = CardRarity.Bronze;

        this.TagKeys.Add("tag_monster");
        this.TagKeys.Add("tag_slime");

        this.CardSkillDescriptionKey = "card_slime_purple_skill_desc";
        this.FlavorTextKey = "card_slime_purple_flavor";

        this.HasQuest = false;
        this.Durability = -1;

        this.PoisonStacksToApply = 3;
        this.BaseCritChance = 0.0f;
    }

    public override void ExecuteSkill()
    {
        // 독 중첩수 가져오기
        int stacks = GetCurrentPoisonStacks();
        MonsterController monsterOwner = Owner as MonsterController;

        if (monsterOwner != null)
        {
            // 상대방(영주)에게 독 상태 이상 부여
            monsterOwner.GetTarget()?.ApplyLordStatus(StatusEffectType.Poison, stacks);
        }
    }

    public override int GetCurrentPoisonStacks() => this.PoisonStacksToApply;
    public override float GetCurrentCritChance() => this.BaseCritChance;
}