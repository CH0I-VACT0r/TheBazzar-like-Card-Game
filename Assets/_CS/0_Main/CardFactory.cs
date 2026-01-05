using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

// 'Card ID' based Card Factory with Reflection Support
public static class CardFactory
{
    // Retro-compatibility list
    private static List<string> allCardIDs = new List<string>();

    // Cache for creating prototypes
    private static Dictionary<string, Card> _prototypeCache;
    // Cache for types to create new instances
    private static Dictionary<string, Type> _typeCache;

    private static bool _isInitialized = false;

    // Initialize Cache
    private static void InitializeCache()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        _prototypeCache = new Dictionary<string, Card>();
        _typeCache = new Dictionary<string, Type>();
        allCardIDs = new List<string>();

        // 1. Reflection: Find all classes with [CardConfigAttribute]
        var cardTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<CardConfigAttribute>() != null);

        foreach (var type in cardTypes)
        {
            var attr = type.GetCustomAttribute<CardConfigAttribute>();
            string id = attr.Id;

            if (_typeCache.ContainsKey(id))
            {
                Debug.LogError($"[CardFactory] Duplicate Card ID found: {id} in {type.Name}");
                continue;
            }

            // Register Type
            _typeCache[id] = type;
            allCardIDs.Add(id);

            // Register Name
            // Key convention: card_{id}_name
            // If the ID already contains "card_", we might want to avoid double prefix, 
            // but for safety/consistency with existing keys (which vary), we should probably stick to the ID as the base.
            // Existing: "card_wolf_dagger_name" -> ID "wolf_dagger"? or "card_wolf_dagger"?
            // Let's assume ID is "wolf_dagger" -> Key "card_wolf_dagger_name".
            
            string nameKey = $"card_{id}_name";
            if (id.StartsWith("card_")) nameKey = $"{id}_name"; // Handle IDs that already have prefix if user prefers that style

            LocalizationManager.RegisterText(nameKey, attr.NameEn, attr.NameKo);

            // Register Desc
            string descKey = $"card_{id}_desc"; 
            // Some cards use _skill_desc. Let's register both or just standarize.
            // Let's also register _skill_desc as an alias if needed, or just desc.
            string skillDescKey = $"card_{id}_skill_desc";

            LocalizationManager.RegisterText(descKey, attr.DescEn, attr.DescKo);
            LocalizationManager.RegisterText(skillDescKey, attr.DescEn, attr.DescKo); // Fallback

             // Register Flavor
            string flavorKey = $"card_{id}_flavor";
            LocalizationManager.RegisterText(flavorKey, attr.FlavorEn, attr.FlavorKo);

            // Create Prototype for filtering
            try
            {
                // We assume constructor is (object owner, int index)
                Card proto = (Card)Activator.CreateInstance(type, null, -1);
                _prototypeCache[id] = proto;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CardFactory] Failed to create prototype for {id}: {e.Message}");
            }
        }
        
        // Manual Registration for Old Cards (Backwards Compatibility)
        RegisterLegacyCards();

        Debug.Log($"[CardFactory] Initialized {_typeCache.Count} cards via Reflection.");
    }

    private static void RegisterLegacyCards()
    {
        // Add manual IDs if they are not yet in reflection
        string[] legacyIDs = {
            "card_barbarian_warrior", "card_manual_beginner", "card_potion_heal", 
            "card_icewolf", "card_frozenknight", "card_wolf_dagger",
            "card_wolffang", "card_branch", "card_torn_book", 
            "card_barbarian_shieldbearer", "card_sheep", "card_goblin", "card_witch"
        };

        foreach (var id in legacyIDs)
        {
            if (!_typeCache.ContainsKey(id))
            {
                Card proto = CreateLegacyInstance(id, null, -1);
                if (proto != null)
                {
                   _prototypeCache[id] = proto;
                   if (!allCardIDs.Contains(id)) allCardIDs.Add(id);
                }
            }
        }
    }

    public static Card CreateRandomCard(CardRarity rarity)
    {
        InitializeCache();
        if (allCardIDs.Count == 0) return null;

        string randomID = allCardIDs[UnityEngine.Random.Range(0, allCardIDs.Count)];
        return CreateCard(randomID, null, -1);
    }

    public static Card CreateCardByFilter(CardRarity targetRarity, CardType targetType, string requiredTag, LordType lord, object owner)
    {
        InitializeCache();

        List<string> candidateIDs = new List<string>();

        foreach (var kvp in _prototypeCache)
        {
            string id = kvp.Key;
            Card proto = kvp.Value;

            if ((int)targetRarity != 0 && proto.Rarity != targetRarity) continue;
            if ((int)targetType != 0 && proto.ItemType != targetType) continue;
            if (!string.IsNullOrEmpty(requiredTag) && !proto.HasTagKey(requiredTag)) continue;
             if (proto.OwnerLord != LordType.Common && proto.OwnerLord != lord) continue;

            candidateIDs.Add(id);
        }

        if (candidateIDs.Count > 0)
        {
            string pickedID = candidateIDs[UnityEngine.Random.Range(0, candidateIDs.Count)];
            return CreateCard(pickedID, owner, -1);
        }

        return null;
    }

    public static Card CreateCard(string cardID, object owner, int index)
    {
        InitializeCache();

        // 1. Try Reflection Cache
        if (_typeCache.ContainsKey(cardID))
        {
            try
            {
                Card newCard = (Card)Activator.CreateInstance(_typeCache[cardID], owner, index);
                return newCard;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CardFactory] Error creating {cardID}: {e}");
            }
        }

        // 2. Fallback to Legacy
        return CreateLegacyInstance(cardID, owner, index);
    }
    
    private static Card CreateLegacyInstance(string cardID, object owner, int index)
    {
        MonsterController monsterOwner = owner as MonsterController;

        switch (cardID)
        {
            // --- 제작 및 재료 아이템 (상점 미등장) ---
            case "card_slime_jelly": return new Card_SlimeJelly(owner, index);
            case "card_wolffang": return new Card_WolfFang(owner, index);
            case "card_branch": return new Card_Branch(owner, index);
            case "card_wolf_dagger": return new Card_WolfDagger(owner, index);

            // --- '아이템' 카드들 --- 
            case "card_potion_heal": return new Card_Potion_Heal(owner, index);
            case "card_torn_book": return new Card_Torn_Book(owner, index);

            // --- '중립' 카드들 ---
            case "card_manual_beginner": return new Card_Manual_Beginner(owner, index);

            // --- '혹한의 성주' 카드들 ---
            case "card_barbarian_warrior": return new Card_BarbarianWarrior(owner, index);
            case "card_barbarian_shieldbearer": return new Card_BarbarianShieldbearer(owner, index);
            case "card_icewolf": return new Card_IceWolf(owner, index);
            case "card_frozenknight": return new Card_FrozenKnight(owner, index);

            // --- '몬스터' 카드들 ---
            case "card_sheep": return new Card_Sheep(owner, index);

            case "card_slime_green":
                if (monsterOwner != null) return new Card_Slime_Green(monsterOwner, index);
                break;
            case "card_slime_yellow":
                if (monsterOwner != null) return new Card_Slime_Yellow(monsterOwner, index);
                break;
            case "card_slime_red":
                if (monsterOwner != null) return new Card_Slime_Red(monsterOwner, index);
                break;
            case "card_slime_purple":
                if (monsterOwner != null) return new Card_Slime_Purple(monsterOwner, index);
                break;

            case "card_goblin":
                if (monsterOwner != null) return new Card_Goblin(monsterOwner, index);
                break;
            case "card_witch":
                if (monsterOwner != null) return new Card_Witch(monsterOwner, index);
                break;
            
        }
        return null;
    }
}