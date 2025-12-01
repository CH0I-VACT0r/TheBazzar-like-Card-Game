using System.Collections.Generic;
using UnityEngine;

// (프로토타입용 간단한 번역기)
/// 텍스트 키(Key)를 받아서, 현재 언어에 맞는 실제 텍스트 반환

public static class LocalizationManager
{
    // TODO: 나중에 이 값을 "en", "jp" 등으로 바꾸면 언어가 바뀝니다.
    private static string m_CurrentLanguage = "ko";

    // 한국어 사전 (ko)
    private static Dictionary<string, string> m_KoreanDict = new Dictionary<string, string>()
    {
        // TODO: 여기에 모든 카드 텍스트 키 추가
        // --- 인벤토리 ---
        { "ui_inventory_title", "인벤토리" },
        { "ui_tab_mercenary", "용병" },
        { "ui_tab_consumable", "소모품" },
        { "ui_tab_material", "재료" },

        // --- 툴팁 UI ---
        { "quest_status_complete", "완료" },
        { "quest_status_incomplete", "진행 중" },
        { "stat_cooldown", "쿨타임: {0}초" },
        { "stat_crit_chance", "치명타 확률: {0}%" },
        { "stat_durability", "내구도: {0}" },
        { "stat_damage", "피해량: {0}" },
        { "stat_shield", "쉴드 획득: {0}" },
        { "stat_heal", "체력 회복: {0}" },
        { "stat_apply_bleed", "출혈 : {0}" },
        { "stat_apply_poison", "중독 : {0}" },
        { "stat_apply_burn", "화상 : {0}" },
        { "stat_apply_heal_dot", "지속 회복 : {0}" },
        { "stat_apply_freeze", "빙결 : {0}초" },
        { "stat_apply_haste", "가속 : {0}초" },
        { "stat_apply_slow", "감속 : {0}초" },
        { "stat_apply_cooldown_reduction", "촉진 : {0}초" },
        { "stat_apply_cooldown_increase", "방해 : {0}초" },
        { "stat_apply_echo", "메아리 : {0}회" },
        { "stat_apply_shock", "충격 : {0}초" },
        { "stat_apply_sturdy", "견고 : {0}초" },
        { "stat_summon", "소환: {0} x {1}" },
        { "stat_deathrattle", "유언: {0}" },
        { "stat_apply_price_inflate", "가치 인상 : {0}" },
        { "stat_apply_price_extort", "가치 인하 : {0}" },
        { "stat_apply_polymorph", "변이 : {0} {1}초" },
        { "stat_triggers_shuffle", "교란 : {0}칸" },
        { "stat_triggers_chain", "연쇄" },

        // --- 태그 ---
        { "tag_mercenary", "용병" },
        { "tag_dealer", "딜러" },
        { "tag_tanker", "탱커" },
        { "tag_healer", "힐러" },
        { "tag_regen", "리젠" },
        { "tag_bleed", "출혈" },
        { "tag_burn", "화상" },
        { "tag_poison", "중독" },
        { "tag_freeze", "빙결" },
        { "tag_summon", "소환" },
        { "tag_deathrattle", "유언" },
        { "tag_barbarian", "야만전사" },
        { "tag_beast", "야수" },
        { "tag_monster", "몬스터" },
        { "tag_goblin", "고블린" },
        { "tag_book",  "책" },
        { "tag_armor", "방어구" },
        { "tag_weapon", "무기" },
        { "tag_potion", "포션" },


        // --- [카드] ---
        // --- 재료 ---
        { "card_torn_book_name", "찢어진 책" },
        { "card_torn_book_desc", "너무 낡아서 더 이상 읽을 수 없습니다." },
        { "card_torn_book_flavor", "누군가 치열하게 공부한 흔적이 남아있습니다." },

        // --- 중립 ---
        { "card_manual_beginner_name", "하급 무술 교본" },
        { "card_manual_beginner_desc", "왼쪽에 있는 [딜러] 카드의 공격력을 영구적으로 10 증가시킵니다." },
        { "card_manual_beginner_flavor", "기초적인 검술 동작이 그려져 있습니다." },

        // 양
        { "card_sheep_name", "양"},
        { "card_sheep_desc", "양입니다." },

        // 야만전사 전투병
        { "card_barbarian_warrior_name", "야만전사 전투병" },
        { "card_barbarian_warrior_skill_desc", "도끼로 상대를 공격합니다." },
        { "card_barbarian_warrior_quest_title", "[이기자!]" },
        { "card_barbarian_warrior_quest_desc", "전투에서 3회 승리" },
        { "card_barbarian_warrior_flavor", "\"우어어어어어!!\"" },

        // 야만전사 방패병
        { "card_barbarianshield_name", "야만전사 방패병" },
        { "card_barbarianshield_skill_desc", "방패를 들어 쉴드를 얻습니다." },
        { "card_barbarianshield_flavor", "\"넌 못 지나간다!\"" },

        // 고블린
        { "card_goblin_name", "고블린" },
        { "card_goblin_skill_desc", "상대를 공격합니다." },
        { "card_goblin_flavor", "전형적인 고블린입니다." },

        // 마녀
        { "card_witch_name", "마녀" },
        { "card_witch_skill_desc", "상대 카드를 양으로 변이시킵니다." },

        // --- 상점 이벤트 제목 & 설명 ---
        { "evt_shop_bronze_title", "허름한 용병 길드" },
        { "evt_shop_bronze_desc", "초보 모험가들이 모이는 곳입니다. 저렴하게 용병을 고용할 수 있습니다." },

        { "evt_shop_barbarian_title", "야만전사 야영지" },
        { "evt_shop_barbarian_desc", "거친 숨소리가 들려옵니다. 야만전사들이 당신을 기다립니다." },

        { "evt_shop_potion_title", "잡화 상점" },
        { "evt_shop_potion_desc", "마을에 중심에 위치한 잡화 상점입니다. 다양한 물약을 팔고 있습니다." },

        { "evt_reinforce_title", "낡은 대장간" },
        { "evt_reinforce_desc", "오래된 모루와 망치가 보입니다. 장비 아이템의 내구도를 소폭 늘릴 수 있습니다." },

        { "evt_repair_book_title", "기억의 제본소" },
        { "evt_repair_book_desc", "오래된 종이 냄새와 잉크 향이 가득합니다. 낡아빠진 책이나 교본을 가져가면 새것처럼 고쳐줍니다." },

        { "evt_repair_consumable_title", "떠돌이 보급 마차" },
        { "evt_repair_consumable_desc", "모닥불 위에 큰 스튜 냄비가 끓고 있습니다. 지친 모험가들을 위해 식량과 상비약을 보충해 주는 곳입니다." },

        // 1. 공격 (Damage)
        { "evt_train_damage_title", "허수아비 치기" },
        { "evt_train_damage_desc", "가장 기초적인 훈련입니다. [딜러]의 기본 공격력을 10만큼 강화합니다." },

        // 2. 방어 (Shield)
        { "evt_train_shield_title", "폭포수 수련" },
        { "evt_train_shield_desc", "쏟아지는 물줄기를 견뎌냅니다.[탱커]의 기본 방어도를 10만큼 강화합니다." },

        // 3. 회복 (Heal)
        { "evt_train_heal_title", "성녀의 기도" },
        { "evt_train_heal_desc", "성녀에게 기도를 받습니다. [힐러]의 회복량을 10만큼 강화합니다." },

        // 지속힐
        { "evt_train_healdot_title", "기초 약초학 수련" },
        { "evt_train_healdot_desc", "주변에서 흔히 보는 약초를 구분합니다. [지속 회복] 부여 수치를 3만큼 강화합니다." },
        
        // 4. 출혈 (Bleed)
        { "evt_train_bleed_title", "날카로운 숫돌" },
        { "evt_train_bleed_desc", "무기를 예리하게 다듬습니다. [출혈] 부여 수치를 2만큼 강화합니다." },

        // 5. 화상 (Burn)
        { "evt_train_burn_title", "기초 화염 수련" },
        { "evt_train_burn_desc", "불을 다루는 법을 익힙니다. [화상] 부여 수치를 3만큼 강화합니다." },

        // 6. 중독 (Poison)
        { "evt_train_poison_title", "독약 주사" },
        { "evt_train_poison_desc", "맹독 주사를 처방합니다. [중독] 부여 수치를 1만큼 강화합니다." },

        // 7. 빙결 (Freeze)
        { "evt_train_freeze_title", "얼음물 입수" },
        { "evt_train_freeze_desc", "극한의 추위에 적응합니다. [빙결] 지속 시간을 0.1초 강화합니다." },

        // 8. 쿨타임 (Cooldown)
        { "evt_train_cooldown_title", "가벼운 스트레칭" },
        { "evt_train_cooldown_desc", "몸을 풀어 유연성을 기릅니다. 스킬 쿨타임을 2% 단축시킵니다." },

        // 9. 가치 (Price)
        { "evt_train_price_title", "말솜씨 연습" },
        { "evt_train_price_desc", "자신의 가치를 증명하기 위한 연습을 합니다. 상점 판매 가격을 1G 높입니다." },

    };

    // 영어 사전 (en)
    private static Dictionary<string, string> m_EnglishDict = new Dictionary<string, string>()
    {
        // TODO: 여기에 모든 카드 텍스트 키 추가
        // --- 인벤토리 ---
        { "ui_inventory_title", "INVENTORY" },
        { "ui_tab_mercenary", "Mercenary" },
        { "ui_tab_consumable", "Items" },
        { "ui_tab_material", "Material" },

        // --- 툴팁 UI ---
        { "quest_status_complete", "Complete" },
        { "quest_status_incomplete", "In Progress" },
        { "stat_cooldown", "Cooldown: {0}s" },
        { "stat_crit_chance", "Crit Chance: {0}%" },
        { "stat_durability", "Durability: {0}" },
        { "stat_damage", "Damage: {0}" },
        { "stat_shield", "Shield: {0}" },
        { "stat_heal", "Heal: {0}" },
        { "stat_apply_bleed", "Bleed: {0}" },
        { "stat_apply_poison", "Poison: {0}" },
        { "stat_apply_burn", "Burn: {0}" },
        { "stat_apply_heal_dot", "Regen: {0}" },
        { "stat_apply_freeze", "Freeze: {0}s" },
        { "stat_apply_haste", "Haste: {0}s" },
        { "stat_apply_slow", "Slow: {0}s" },
        { "stat_apply_cooldown_reduction", "Stimulate : {0}s" },
        { "stat_apply_cooldown_increase", "Hider : {0}s" },
        { "stat_apply_echo", "Echo: {0}" },
        { "stat_apply_shock", "Shock: {0}s" },
        { "stat_apply_sturdy", "Sturdy: {0}s" },
        { "stat_summon", "Summon" },
        { "stat_deathrattle", "Deathrattle" },
        { "stat_apply_price_inflate", "Price Inflate : {0}" },
        { "stat_apply_price_extort", "Price Extort : {0}" },
        { "stat_apply_polymorph", "Polymorph : {0} {1}s" },
        { "stat_triggers_shuffle", "Disruption : {0} slots" },
        { "stat_triggers_chain", "Chain" },
        
        // --- 태그 ---
        { "tag_mercenary", "Mercenary" },
        { "tag_barbarian", "Barbarian" },
        { "tag_dealer", "Dealer" },
        { "tag_tanker", "Tanker" },
        { "tag_healer", "Healer" },
        { "tag_regen", "Regen" },
        { "tag_bleed", "Bleed" },
        { "tag_burn", "Burn" },
        { "tag_poison", "Poison" },
        { "tag_freeze", "Freeze" },
        { "tag_summon", "Summon" },
        { "tag_deathrattle", "Deathrattle" },
        { "tag_monster", "Monster" },
        { "tag_beast", "Beast" },
        { "tag_goblin", "Goblin" },
        { "tag_book",  "Book" },
        { "tag_armor", "Armor" },
        { "tag_weapon", "Weapon" },
        { "tag_potion", "Potion" },

        // --- [카드] ---
        // --- 재료 ---
        { "card_torn_book_name", "Torn Book" },
        { "card_torn_book_desc", "It is too worn out to read." },
        { "card_torn_book_flavor", "Traces of someone's intense study remain." },
        // --- 중립 ---
        // 하급 무술 교본
        { "card_manual_beginner_name", "Lesser Martial Arts Manual" },
        { "card_manual_beginner_desc", "Permanently increases the Attack Damage of the [Dealer] card to the left by 10." },
        { "card_manual_beginner_flavor", "It depicts basic swordsmanship movements." },

        // 양
        { "card_sheep_name", "Sheep"},
        { "card_sheep_desc", "It's a sheep." },

        // 야만전사 전투병
        { "card_barbarian_warrior_name", "Barbarian Warrior" },
        { "card_barbarian_warrior_skill_desc", "Attack the opponent with an ax." },
        { "card_barbarian_warrior_quest_title", "[Let's Win!]" },
        { "card_barbarian_warrior_quest_desc", "Win 3 battles" },
        { "card_barbarian_warrior_flavor", "\"Waaaaaaaaagh!!\"" },

        // 야만전사 방패병
        { "card_barbarianshield_name", "Barbarian Shieldbearer" },
        { "card_barbarianshield_skill_desc", "Lift the shield" },
        { "card_barbarianshield_flavor", "\"You shall not pass!\"" },

        // 고블린
        { "card_goblin_name", "Goblin" },
        { "card_goblin_skill_desc", "Attack the opponent" },
        { "card_goblin_flavor", "A typical goblin." },

        // 마녀
        { "card_witch_name", "Witch" },
        { "card_witch_skill_desc", "Polymorph the opponent card into a sheep." },

        // --- 상점 이벤트 제목 & 설명 ---
        { "evt_shop_bronze_title", "Run-down Mercenary Guild" },
        { "evt_shop_bronze_desc", "A gathering place for novice adventurers. You can hire mercenaries at a low price." },

        { "evt_shop_barbarian_title", "Barbarian Encampment" },
        { "evt_shop_barbarian_desc", "The sound of heavy breathing fills the air. Barbarians represent strength." }, 

        { "evt_shop_potion_title", "General Store" },
        { "evt_shop_potion_desc", "A general store located in the town center. They sell a variety of potions." },

        { "evt_reinforce_title", "Old Smithy" },
        { "evt_reinforce_desc", "You see an old anvil and hammer. You can tend to your equipment to extend their durability." },

        { "evt_repair_book_title", "The Memory Bindery" },
        { "evt_repair_book_desc", "The air is filled with the scent of old paper and ink. They can restore worn-out books and manuals as if they were new." },

        { "evt_repair_consumable_title", "Wandering Supply Wagon" },
        { "evt_repair_consumable_desc", "A large stew pot is boiling over a campfire. It's a place to replenish food and medicine for weary adventurers." },

        // 1. Damage (허수아비 치기)
        { "evt_train_damage_title", "Scarecrow Practice" },
        { "evt_train_damage_desc", "The most basic form of training. Permanently increases the Base Attack of [Dealer] by 10." },

        // 2. Shield (폭포수 수련)
        { "evt_train_shield_title", "Waterfall Training" },
        { "evt_train_shield_desc", "Endure the pouring streams of water. Permanently increases the Base Shield of [Tanker] by 10." },

        // 3. Heal (성녀의 기도)
        { "evt_train_heal_title", "Saint's Prayer" },
        { "evt_train_heal_desc", "Receive a prayer from the Saintess. Permanently increases the Heal Amount of [Healer] by 10." },

        // 10. Heal Dot (기초 약초학 수련)
        { "evt_train_healdot_title", "Basic Herbalism" },
        { "evt_train_healdot_desc", "Learn to distinguish common herbs. Permanently increases the [Heal over Time] amount by 3." },

        // 4. Bleed (날카로운 숫돌)
        { "evt_train_bleed_title", "Sharp Whetstone" },
        { "evt_train_bleed_desc", "Sharpen your weapons keenly. Permanently increases the [Bleed] stack application by 2." },

        // 5. Burn (기초 화염 수련)
        { "evt_train_burn_title", "Basic Pyromancy" },
        { "evt_train_burn_desc", "Learn how to handle fire. Permanently increases the [Burn] stack application by 3." },

        // 6. Poison (독약 주사)
        { "evt_train_poison_title", "Venom Injection" },
        { "evt_train_poison_desc", "Prescribe a deadly venom. Permanently increases the [Poison] stack application by 1." },

        // 7. Freeze (얼음물 입수)
        { "evt_train_freeze_title", "Ice Plunge" },
        { "evt_train_freeze_desc", "Adapt to the extreme cold. Permanently increases the [Freeze] duration by 0.1s." },

        // 8. Cooldown (가벼운 스트레칭)
        { "evt_train_cooldown_title", "Light Stretching" },
        { "evt_train_cooldown_desc", "Loosen up the body to increase flexibility. Permanently reduces Skill Cooldown by 2%." },

        // 9. Price (말솜씨 연습)
        { "evt_train_price_title", "Eloquence Practice" },
        { "evt_train_price_desc", "Practice speech to prove your worth. Permanently increases the Sell Price by 1G." },
    };

    // "키"를 주면 현재 언어에 맞는 "텍스트"를 반환합니다.
    public static string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return ""; // 키가 비어있으면 빈 문자열 반환
        }

        Dictionary<string, string> targetDict = null;

        if (m_CurrentLanguage == "ko")
        {
            targetDict = m_KoreanDict;
        }
        else if (m_CurrentLanguage == "en")
        {
            targetDict = m_EnglishDict;
        }
        // (나중에 다른 언어 추가)

        if (targetDict == null) return $"[NO LANG: {key}]";

        // 사전에서 키를 찾아 텍스트를 반환
        if (targetDict.TryGetValue(key, out string value))
        {
            return value;
        }

        // 사전에 키가 없으면 경고용 텍스트 반환
        return $"[MISSING: {key}]";
    }

    // 언어 설정
    public static void SetLanguage(string languageCode) // "en", "ko", "jp" ...
    {
        // TODO: 실제로 그 언어 사전이 존재하는지 확인하는 로직이 있으면 더 좋습니다.
        m_CurrentLanguage = languageCode;
        Debug.Log($"[Localization] 언어가 {m_CurrentLanguage}(으)로 변경되었습니다.");
    }

    // 현재 언어에 맞는 "텍스트"를 반환
    public static string GetText(string key, params object[] args)
    {
        // 1. 기본 번역 텍스트를 가져옵니다. (예: "...{0}의 피해...")
        string baseText = GetText(key);

        if (string.IsNullOrEmpty(baseText) || baseText.StartsWith("["))
        {
            return baseText; // 키가 없거나 에러가 난 텍스트는 포맷팅하지 않음
        }

        // 2. C#의 string.Format 기능을 사용해 {0} 자리에 args[0] (피해량)을 끼워넣습니다.
        try
        {
            return string.Format(baseText, args);
        }
        catch (System.Exception)
        {
            // 포맷팅에 실패하면 (예: {0}은 있는데 args가 없으면) 경고용 텍스트 반환
            return $"[FORMAT ERR: {key}]";
        }
    }
}
