using System.Collections.Generic;
using UnityEngine;

// (������Ÿ�Կ� ������ ������)
/// �ؽ�Ʈ Ű(Key)�� �޾Ƽ�, ���� �� �´� ���� �ؽ�Ʈ ��ȯ

public static class LocalizationManager
{
    // TODO: ���߿� �� ���� "en", "jp" ������ �ٲٸ� �� �ٲ�ϴ�.
    private static string m_CurrentLanguage = "ko";

    // �ѱ��� ���� (ko)
    private static Dictionary<string, string> m_KoreanDict = new Dictionary<string, string>()
    {
        // TODO: ���⿡ ��� ī�� �ؽ�Ʈ Ű �߰�
        // --- �������� �̸� ---
        { "stage_name_01", "������ ����" },
        { "stage_name_02", "��â�� ��" },
        { "msg_new_region_unlock", "�� ���� ����� �뺴�� ��� ����� �� �ֽ��ϴ�."},

        // --- �κ��丮 ---
        { "ui_inventory_title", "�κ��丮" },
        { "ui_tab_mercenary", "�뺴" },
        { "ui_tab_consumable", "�Ҹ�ǰ" },
        { "ui_tab_material", "���" },

        // --- ���� UI ---
        { "quest_status_complete", "�Ϸ�" },
        { "quest_status_incomplete", "���� ��" },
        { "stat_cooldown", "��Ÿ��: {0}��" },
        { "stat_crit_chance", "ġ��Ÿ Ȯ��: {0}%" },
        { "stat_durability", "������: {0}" },
        { "stat_damage", "���ط�: {0}" },
        { "stat_shield", "���� ȹ��: {0}" },
        { "stat_heal", "ü�� ȸ��: {0}" },
        { "stat_apply_bleed", "���� : {0}" },
        { "stat_apply_poison", "�ߵ� : {0}" },
        { "stat_apply_burn", "ȭ�� : {0}" },
        { "stat_apply_heal_dot", "���� ȸ�� : {0}" },
        { "stat_apply_freeze", "���� : {0}��" },
        { "stat_apply_haste", "���� : {0}��" },
        { "stat_apply_slow", "���� : {0}��" },
        { "stat_apply_cooldown_reduction", "���� : {0}��" },
        { "stat_apply_cooldown_increase", "���� : {0}��" },
        { "stat_apply_echo", "�޾Ƹ� : {0}ȸ" },
        { "stat_apply_shock", "��� : {0}��" },
        { "stat_apply_sturdy", "�߰� : {0}��" },
        { "stat_summon", "��ȯ: {0} x {1}" },
        { "stat_deathrattle", "����: {0}" },
        { "stat_apply_price_inflate", "��ġ �λ� : {0}" },
        { "stat_apply_price_extort", "��ġ ���� : {0}" },
        { "stat_apply_polymorph", "���� : {0} {1}��" },
        { "stat_triggers_shuffle", "���� : {0}ĭ" },
        { "stat_triggers_chain", "����" },

        // --- �±� ---
        { "tag_mercenary", "�뺴" },
        { "tag_dealer", "���" },
        { "tag_tanker", "��Ŀ" },
        { "tag_healer", "����" },
        { "tag_regen", "����" },
        { "tag_bleed", "����" },
        { "tag_burn", "ȭ��" },
        { "tag_poison", "�ߵ�" },
        { "tag_freeze", "����" },
        { "tag_summon", "��ȯ" },
        { "tag_deathrattle", "����" },
        { "tag_barbarian", "�߸�����" },
        { "tag_knight", "���" },
        { "tag_beast", "�߼�" },
        { "tag_monster", "����" },
        { "tag_slime", "������" },
        { "tag_goblin", "����" },
        { "tag_book",  "å" },
        { "tag_armor", "��" },
        { "tag_weapon", "����" },
        { "tag_potion", "����" },
        { "tag_material", "���" },
        { "tag_wood", "����" },


        // --- [ī��] ---
        // --- ������ ---
        { "card_potion_heal_name", "ȸ�� ����" },
        { "card_potion_heal_desc", "�÷��̾��� ü���� 30 ȸ���մϴ�." },
        { "card_potion_heal_flavor", "���� ���� ���� ��� ���� �����Դϴ�." },

        // --- ��� ---
        { "card_torn_book_name", "������ å" },
        { "card_torn_book_desc", "�ʹ� ���Ƽ� �� �̻� ���� �� �����ϴ�." },
        { "card_torn_book_flavor", "������ ġ���ϰ� ������ ������ �����ֽ��ϴ�." },

        { "card_slime_jelly_name", "������ ����" },
        { "card_slime_jelly_desc", "�����Ÿ��� ���������� �����Դϴ�." },
        { "card_slime_jelly_flavor", "������ ����� ���ϰ� ����������, �ݹ� ���� ���������ϴ�." },

        { "card_branch_name", "��������" },
        { "card_branch_desc", "�濡�� ���� ���̴� ���������Դϴ�." },
        { "card_branch_flavor", "���� ����" },

        { "card_wolffang_name", "������ ���� �̻�" },
        { "card_wolffang_desc", "������ ������ ��ī�ο� �۰����Դϴ�." },
        { "card_wolffang_flavor", "�ָӴϿ� �ٷ� ������ ������ �� ������ �����ϼ���" },


        // --- �߸� ---
        // [å]
        { "card_manual_beginner_name", "�ϱ� ���� ����" },
        { "card_manual_beginner_desc", "���ʿ� �ִ� [���] ī���� ���ݷ��� ���������� 10 ������ŵ�ϴ�." },
        { "card_manual_beginner_flavor", "�������� �˼� ������ �׷��� �ֽ��ϴ�." },

        // [����]
        { "card_wolf_dagger_name", "���� �̻� �ܰ�" },
        { "card_wolfdagger_skill_desc", "������ ���� 5�� �ο��մϴ�." },
        { "card_wolfdagger_flavor", "��ī�ο� ������ �̻��� �����Ͽ� ���� �ܰ��Դϴ�." },


        // ��
        { "card_sheep_name", "��"},
        { "card_sheep_desc", "���Դϴ�." },
        
        // --- [ �뺴 ] ---
        // �߸����� ������
        { "card_barbarian_warrior_name", "�߸����� ������" },
        { "card_barbarian_warrior_skill_desc", "������ ��븦 �����մϴ�." },
        { "card_barbarian_warrior_quest_title", "[�̱���!]" },
        { "card_barbarian_warrior_quest_desc", "�������� 3ȸ �¸�" },
        { "card_barbarian_warrior_flavor", "\"�������!!\"" },

        // �߸����� ���к�
        { "card_barbarianshield_name", "�߸����� ���к�" },
        { "card_barbarianshield_skill_desc", "���и� ��� ���带 ����ϴ�." },
        { "card_barbarianshield_flavor", "\"�� �� ��������!\"" },

        //���� ����
        { "card_icewolf_name", "���� ����" },
        { "card_icewolf_skill_desc", "�������� �����ϴ�. ������ �����Դ� ������ �����ϴ�." },
        { "card_icewolf_flavor", "\"������ �������� �����ϴ� �����Դϴ�.\"" },

        //������ ���
        { "card_frozenknight_name", "������ ���" },
        { "card_frozenknight_desc", "������ ��󿡰� ������ �ο��մϴ�." },
        { "card_frozenknight_flavor", "\"���� �� ������, ������ ħ���� �����϶�.\"" },

        // --- [ ���� ] ---
        // ������
        { "card_slime_green_name", "�ʷ� ������" },
        { "card_slime_green_skill_desc", "��¦ �پ�ö� ü���� ȸ���մϴ�." },
        { "card_slime_green_flavor", "���� ��ó���� ���� �� �� �ִ� �ʷϻ� �������Դϴ�." },

        { "card_slime_yellow_name", "��� ������" },
        { "card_slime_yellow_skill_desc", "��¦ �پ�ö� ���� ȹ���մϴ�." },
        { "card_slime_yellow_flavor", "���� ��ó���� ���� �� �� �ִ� ����� �������Դϴ�." },

        { "card_slime_red_name", "���� ������" },
        { "card_slime_red_skill_desc", "��¦ �پ�ö� ��븦 �����մϴ�." },
        { "card_slime_red_flavor", "���� ��ó���� ���� �� �� �ִ� ������ �������Դϴ�." },

        { "card_slime_purple_name", "���� ������" },
        { "card_slime_purple_skill_desc", "��¦ �پ�ö� ��뿡�� �� ��ø�� �ο��մϴ�." },
        { "card_slime_purple_flavor", "���� ��ó���� ���� �� �� �ִ� ����� �������Դϴ�." },

        // ����
        { "card_goblin_name", "����" },
        { "card_goblin_skill_desc", "��븦 �����մϴ�." },
        { "card_goblin_flavor", "�������� �����Դϴ�." },

        // ����
        { "card_witch_name", "����" },
        { "card_witch_skill_desc", "��� ī�带 ������ ���̽�ŵ�ϴ�." },

        // --- ���� �̺�Ʈ ���� & ���� ---
        { "evt_shop_bronze_title", "�㸧�� �뺴 ���" },
        { "evt_shop_bronze_desc", "�ʺ� ���谡���� ���̴� ���Դϴ�. �����ϰ� �뺴�� ����� �� �ֽ��ϴ�." },

        { "evt_shop_barbarian_title", "�߸����� �߿���" },
        { "evt_shop_barbarian_desc", "��ģ ���Ҹ��� ����ɴϴ�. �߸�������� ����� ��ٸ��ϴ�." },

        { "evt_shop_potion_title", "��ȭ ����" },
        { "evt_shop_potion_desc", "������ �߽ɿ� ��ġ�� ��ȭ �����Դϴ�. �پ��� ������ �Ȱ� �ֽ��ϴ�." },

        { "evt_reinforce_title", "���� ���尣" },
        { "evt_reinforce_desc", "������ ���� ��ġ�� ���Դϴ�. ��� �������� �������� ���� �ø� �� �ֽ��ϴ�." },

        { "evt_repair_book_title", "����� ������" },
        { "evt_repair_book_desc", "������ ���� ������ ��ũ ���� �����մϴ�. ���ƺ��� å�̳� ������ �������� ����ó�� �����ݴϴ�." },

        { "evt_repair_consumable_title", "������ ���� ����" },
        { "evt_repair_consumable_desc", "��ں� ���� ū ��Ʃ ���� ���� �ֽ��ϴ�. ��ģ ���谡���� ���� �ķ��� ������ ������ �ִ� ���Դϴ�." },

        // --- ���� ���� �̺�Ʈ ---
        // 1. ���� (Damage)
        { "evt_train_damage_title", "����ƺ� ġ��" },
        { "evt_train_damage_desc", "���� �������� �Ʒ��Դϴ�. [���]�� �⺻ ���ݷ��� 10��ŭ ��ȭ�մϴ�." },

        // 2. ��� (Shield)
        { "evt_train_shield_title", "������ ����" },
        { "evt_train_shield_desc", "������� ���ٱ⸦ �ߵ����ϴ�.[��Ŀ]�� �⺻ ���� 10��ŭ ��ȭ�մϴ�." },

        // 3. ȸ�� (Heal)
        { "evt_train_heal_title", "������ �⵵" },
        { "evt_train_heal_desc", "���࿡�� �⵵�� �޽��ϴ�. [����]�� ȸ������ 10��ŭ ��ȭ�մϴ�." },

        // ������
        { "evt_train_healdot_title", "���� ������ ����" },
        { "evt_train_healdot_desc", "�ֺ����� ���� ���� ���ʸ� �����մϴ�. [���� ȸ��] �ο� ��ġ�� 3��ŭ ��ȭ�մϴ�." },
        
        // 4. ���� (Bleed)
        { "evt_train_bleed_title", "��ī�ο� ����" },
        { "evt_train_bleed_desc", "���⸦ �����ϰ� �ٵ���ϴ�. [����] �ο� ��ġ�� 2��ŭ ��ȭ�մϴ�." },

        // 5. ȭ�� (Burn)
        { "evt_train_burn_title", "���� ȭ�� ����" },
        { "evt_train_burn_desc", "���� �ٷ�� ���� �����ϴ�. [ȭ��] �ο� ��ġ�� 3��ŭ ��ȭ�մϴ�." },

        // 6. �ߵ� (Poison)
        { "evt_train_poison_title", "���� �ֻ�" },
        { "evt_train_poison_desc", "�͵� �ֻ縦 ó���մϴ�. [�ߵ�] �ο� ��ġ�� 1��ŭ ��ȭ�մϴ�." },

        // 7. ���� (Freeze)
        { "evt_train_freeze_title", "������ �Լ�" },
        { "evt_train_freeze_desc", "������ ������ �����մϴ�. [����] ���� �ð��� 0.1�� ��ȭ�մϴ�." },

        // 8. ��Ÿ�� (Cooldown)
        { "evt_train_cooldown_title", "������ ��Ʈ��Ī" },
        { "evt_train_cooldown_desc", "���� Ǯ�� �������� �⸨�ϴ�. ��ų ��Ÿ���� 2% �����ŵ�ϴ�." },

        // 9. ��ġ (Price)
        { "evt_train_price_title", "���ؾ� ����" },
        { "evt_train_price_desc", "�ڽ��� ��ġ�� �����ϱ� ���� ������ �մϴ�. ���� �Ǹ� ������ 1G ���Դϴ�." },
        // ---------------------------------------------//

        // --- ũ������ ���� �̺�Ʈ ---
        { "evt_crafting_bronze_title", "��� ����" },
        { "evt_crafting_bronze_desc", "���� ���� ��Ḧ �����Ͽ� ��� ������ �� �ִ� ���Դϴ�." },

        // --- ���� �̺�Ʈ ---
        { "title_evt_bt_s1_w1_1" , "������ ������" },
        { "desc_evt_bt_s1_w1_1", "���� �����ӵ��� �� �ִ� ���Դϴ�." },

    };

    // ���� ���� (en)
    private static Dictionary<string, string> m_EnglishDict = new Dictionary<string, string>()
    {
        // TODO: ���⿡ ��� ī�� �ؽ�Ʈ Ű �߰�
         // --- �������� �̸� ---
        { "stage_name_01", "Starting Village" },
        { "stage_name_02", "Dense Forest" },
        { "msg_new_region_unlock", "Higher tier mercenaries and gear are now available."},

        // --- �κ��丮 ---
        { "ui_inventory_title", "INVENTORY" },
        { "ui_tab_mercenary", "Mercenary" },
        { "ui_tab_consumable", "Items" },
        { "ui_tab_material", "Material" },

        // --- ���� UI ---
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
        
        // --- �±� ---
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

        // --- [ī��] ---
        // --- ������ ---
        { "card_potion_heal_name", "Healing Potion" },
        { "card_potion_heal_desc", "Recover the player's HP by 30." },
        { "card_potion_heal_flavor", "It's a pleasant potion with a strawberry scent." },

        // --- ��� ---
        { "card_torn_book_name", "Torn Book" },
        { "card_torn_book_desc", "It is too worn out to read." },
        { "card_torn_book_flavor", "Traces of someone's intense study remain." },
        // --- �߸� ---
        // �ϱ� ���� ����
        { "card_manual_beginner_name", "Lesser Martial Arts Manual" },
        { "card_manual_beginner_desc", "Permanently increases the Attack Damage of the [Dealer] card to the left by 10." },
        { "card_manual_beginner_flavor", "It depicts basic swordsmanship movements." },

        // ��
        { "card_sheep_name", "Sheep"},
        { "card_sheep_desc", "It's a sheep." },

        // �߸����� ������
        { "card_barbarian_warrior_name", "Barbarian Warrior" },
        { "card_barbarian_warrior_skill_desc", "Attack the opponent with an ax." },
        { "card_barbarian_warrior_quest_title", "[Let's Win!]" },
        { "card_barbarian_warrior_quest_desc", "Win 3 battles" },
        { "card_barbarian_warrior_flavor", "\"Waaaaaaaaagh!!\"" },

        // �߸����� ���к�
        { "card_barbarianshield_name", "Barbarian Shieldbearer" },
        { "card_barbarianshield_skill_desc", "Lift the shield" },
        { "card_barbarianshield_flavor", "\"You shall not pass!\"" },

        //���� ����
        { "card_icewolf_name", "Ice Wolf" },
        { "card_icewolf_skill_desc", "Deals damage. Inflicts bleeding on frozen enemies." },
        { "card_icewolf_flavor", "A wolf that attacks with frozen claws." },

        //������ ���
        { "card_frozenknight_name", "Frozen Knight" },
        { "card_frozenknight_desc", "Inflicts frozen on enemies." },
        { "card_frozenknight_flavor", "\"Meet eternal silence at the edge of my blade.\"" },

        // ����
        { "card_goblin_name", "Goblin" },
        { "card_goblin_skill_desc", "Attack the opponent" },
        { "card_goblin_flavor", "A typical goblin." },

        // ����
        { "card_witch_name", "Witch" },
        { "card_witch_skill_desc", "Polymorph the opponent card into a sheep." },

        // --- ���� �̺�Ʈ ���� & ���� ---
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

        // 1. Damage (����ƺ� ġ��)
        { "evt_train_damage_title", "Scarecrow Practice" },
        { "evt_train_damage_desc", "The most basic form of training. Permanently increases the Base Attack of [Dealer] by 10." },

        // 2. Shield (������ ����)
        { "evt_train_shield_title", "Waterfall Training" },
        { "evt_train_shield_desc", "Endure the pouring streams of water. Permanently increases the Base Shield of [Tanker] by 10." },

        // 3. Heal (������ �⵵)
        { "evt_train_heal_title", "Saint's Prayer" },
        { "evt_train_heal_desc", "Receive a prayer from the Saintess. Permanently increases the Heal Amount of [Healer] by 10." },

        // 10. Heal Dot (���� ������ ����)
        { "evt_train_healdot_title", "Basic Herbalism" },
        { "evt_train_healdot_desc", "Learn to distinguish common herbs. Permanently increases the [Heal over Time] amount by 3." },

        // 4. Bleed (��ī�ο� ����)
        { "evt_train_bleed_title", "Sharp Whetstone" },
        { "evt_train_bleed_desc", "Sharpen your weapons keenly. Permanently increases the [Bleed] stack application by 2." },

        // 5. Burn (���� ȭ�� ����)
        { "evt_train_burn_title", "Basic Pyromancy" },
        { "evt_train_burn_desc", "Learn how to handle fire. Permanently increases the [Burn] stack application by 3." },

        // 6. Poison (���� �ֻ�)
        { "evt_train_poison_title", "Venom Injection" },
        { "evt_train_poison_desc", "Prescribe a deadly venom. Permanently increases the [Poison] stack application by 1." },

        // 7. Freeze (������ �Լ�)
        { "evt_train_freeze_title", "Ice Plunge" },
        { "evt_train_freeze_desc", "Adapt to the extreme cold. Permanently increases the [Freeze] duration by 0.1s." },

        // 8. Cooldown (������ ��Ʈ��Ī)
        { "evt_train_cooldown_title", "Light Stretching" },
        { "evt_train_cooldown_desc", "Loosen up the body to increase flexibility. Permanently reduces Skill Cooldown by 2%." },

        // 9. Price (���ؾ� ����)
        { "evt_train_price_title", "Eloquence Practice" },
        { "evt_train_price_desc", "Practice speech to prove your worth. Permanently increases the Sell Price by 1G." },
    };

    // "Ű"�� �ָ� ���� �� �´� "�ؽ�Ʈ"�� ��ȯ�մϴ�.
    public static string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return ""; // Ű�� ��������� �� ���ڿ� ��ȯ
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
        // (���߿� �ٸ� ��� �߰�)

        if (targetDict == null) return $"[NO LANG: {key}]";

        // �������� Ű�� ã�� �ؽ�Ʈ�� ��ȯ
        if (targetDict.TryGetValue(key, out string value))
        {
            return value;
        }

        // ������ Ű�� ������ ���� �ؽ�Ʈ ��ȯ
        return $"[MISSING: {key}]";
    }

    // ��� ����
    public static void SetLanguage(string languageCode) // "en", "ko", "jp" ...
    {
        // TODO: ������ �� ��� ������ �����ϴ��� Ȯ���ϴ� ������ ������ �� �����ϴ�.
        m_CurrentLanguage = languageCode;
        Debug.Log($"[Localization] �� {m_CurrentLanguage}(��)�� ����Ǿ����ϴ�.");
    }

    // ���� �� �´� "�ؽ�Ʈ"�� ��ȯ
    public static string GetText(string key, params object[] args)
    {
        // 1. �⺻ ���� �ؽ�Ʈ�� �����ɴϴ�. (��: "...{0}�� ����...")
        string baseText = GetText(key);

        if (string.IsNullOrEmpty(baseText) || baseText.StartsWith("["))
        {
            return baseText; // Ű�� ���ų� ������ �� �ؽ�Ʈ�� ���������� ����
        }

        // 2. C#�� string.Format ����� ����� {0} �ڸ��� args[0] (���ط�)�� �����ֽ��ϴ�.
        try
        {
            return string.Format(baseText, args);
        }
        catch (System.Exception)
        {
            // �����ÿ� �����ϸ� (��: {0}�� �ִµ� args�� ������) ���� �ؽ�Ʈ ��ȯ
            return $"[FORMAT ERR: {key}]";
        }
    }

    public static void RegisterText(string key, string textEn, string textKo)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (m_EnglishDict == null) m_EnglishDict = new Dictionary<string, string>();
        if (m_KoreanDict == null) m_KoreanDict = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(textEn)) m_EnglishDict[key] = textEn;
        if (!string.IsNullOrEmpty(textKo)) m_KoreanDict[key] = textKo;
    }
}
