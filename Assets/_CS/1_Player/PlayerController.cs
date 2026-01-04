using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용
using System.Collections.Generic; // List 사용

// 모든 플레이어 영주(Lord)의 공통 부모가 되는 기본 클래스 : 체력, 슬롯, 카드 덱 등 공통 기능만 관리
// 영주만의 특수한 로직은 포함하지 않고, 따로 관리

public class PlayerController
{
    // --- 1. 참조 변수 ---
    protected BattleManager m_BattleManager; // 전투 규칙 관리
    protected MonsterController m_Target; // 공격 타겟 : 몬스터 컨트롤러

    // --- 2. UI 요소 ---
    protected VisualElement m_LordPortrait; // 초상화 패널
    protected VisualElement m_PlayerParty; // 7칸 카드 슬롯 패널
    protected VisualElement m_StatusPanel; // 상태 패널 UI
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7); // 7개의 카드 슬롯 UI 요소 리스트
    public int Gold { get; private set; } = 15;
    public int CurrentLife { get; private set; } = 3; // 현재 라이프
    public int MaxLife { get; private set; } = 3;    // 최대 라이프

    // 툴팁 UI 요소
    protected VisualElement m_Root;
    private VisualElement m_TooltipRoot;
    private Label m_TooltipName;
    private VisualElement m_TooltipTagContainer;
    private Label m_TooltipCooldown;
    private Label m_TooltipSkillDesc;
    private VisualElement m_TooltipStatContainer;
    private VisualElement m_TooltipQuestContainer;
    private Label m_TooltipQuestTitle;
    private Label m_TooltipQuestDesc;
    private Label m_TooltipQuestStatus;

    private VisualElement m_TooltipCritContainer;
    private Label m_TooltipCritChance;
    private VisualElement m_TooltipDurabilityContainer;
    private Label m_TooltipDurability;
    private Label m_TooltipFlavorText;
    private VisualElement m_TooltipDivider1;
    private VisualElement m_TooltipDivider2;

    // 툴팁 스케줄러 변수
    private IVisualElementScheduledItem m_TooltipScheduler;
    private const long TOOLTIP_DELAY_MS = 300; // 0.3초

    // 상태 패널 요소
    private VisualElement m_HealthBarFill;
    private VisualElement m_ShieldBarFill;
    private Label m_HealthLabel;
    private Label m_ShieldLabel;
    private Label m_LevelLabel;
    private List<VisualElement> m_XPTicks = new List<VisualElement>(10); // 10칸 네모 XP 바를 위한 리스트
    private VisualElement m_LifeContainer;
    // DoT 도트 대미지 아이콘 UI 라벨
    private Label m_BleedStatusLabel;
    private Label m_PoisonStatusLabel;
    private Label m_BurnStatusLabel;
    private Label m_HealStatusLabel;

    // 역할 UI 컨테이너 리스트
    private List<VisualElement> m_RoleUIContainers = new List<VisualElement>(7);
    private List<VisualElement> m_CooldownOverlays = new List<VisualElement>(7);
    private List<VisualElement> m_CardImageLayers = new List<VisualElement>(7);
    private List<VisualElement> m_CostContainers = new List<VisualElement>(7);
    private List<Label> m_CostLabels = new List<Label>(7);

    // --- 3. 핵심 상태 (공통) ---
    public float CurrentHP { get; protected set; }  // 영주의 현재 체력
    public float MaxHP { get; protected set; } // 영주의 현재 체력
    public float CurrentShield { get; protected set; } // 영주의 현재 쉴드
    
    public int CurrentLevel { get; protected set; } = 1; // 현재 레벨
    public int CurrentXP { get; protected set; } = 0; // 현재 경험치 
    public int MaxXP { get; protected set; } = 10; // 최대 경험치

    // DoT 중첩 변수
    public int BleedStacks { get; protected set; } = 0;
    public int PoisonStacks { get; protected set; } = 0;
    public int BurnStacks { get; protected set; } = 0;
    public int HealStacks { get; protected set; } = 0;

    // DoT 데미지 타이머 (개별 관리)
    private float m_BleedTickTimer = 1.5f;   // 출혈 : 1.5초 
    private float m_PoisonTickTimer = 3.0f;  // 중독 : 3초 
    private float m_BurnTickTimer = 0.5f;    // 화상 : 0.5초
    private float m_HealTickTimer = 2.0f; // 회복 : 2초

    // 충격/견고 상태 변수
    private bool m_IsShocked = false;
    private float m_ShockTimer = 0f;
    private bool m_IsSturdy = false;
    private float m_SturdyTimer = 0f;

    // 충격/견고 배율 (상수)
    private const float SHOCK_MULTIPLIER = 1.2f; // 20% 추가 피해
    private const float STURDY_MULTIPLIER = 0.8f; // 20% 피해 감소

    // --- 4. 카드 덱 관리 ---
    /// 이 영주가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// PlayerController가 처음 생성될 때 호출
    public PlayerController(BattleManager manager, float maxHP)
    {
        this.m_BattleManager = manager;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;
        this.CurrentShield = 0;

        // 2.리스트 초기화(Null 방지)
        Slots = new List<VisualElement>();
        m_RoleUIContainers = new List<VisualElement>();
        m_CooldownOverlays = new List<VisualElement>();
        m_CardImageLayers = new List<VisualElement>();
        m_CostContainers = new List<VisualElement>();
        m_CostLabels = new List<Label>();
        m_XPTicks = new List<VisualElement>();
    }

    // --- 6. 핵심 함수 ---
    public void InitializeUI(VisualElement playerUiRoot, VisualElement mainRoot)
    {
       m_Root = playerUiRoot; // Fixed_Player.uxml의 루트

        // 플레이어 상태 패널 연결 
        m_StatusPanel = playerUiRoot.Q<VisualElement>("PlayerStatus"); 
        
        if (m_StatusPanel != null)
        {
            m_LordPortrait = m_StatusPanel.Q<VisualElement>("Portrait");
            m_HealthBarFill = m_StatusPanel.Q<VisualElement>("HP-Bar-Fill");
            m_HealthLabel = m_StatusPanel.Q<Label>("HP-label");
            m_ShieldBarFill = m_StatusPanel.Q<VisualElement>("Shield-Bar_Fill");
            m_ShieldLabel = m_StatusPanel.Q<Label>("Shield-label");
            m_LevelLabel = m_StatusPanel.Q<Label>("LV-label");
            m_LifeContainer = m_StatusPanel.Q<VisualElement>("LifeContainer");

            // XP Ticks
            m_XPTicks.Clear();
            for (int i = 0; i < 10; i++)
            {
                m_XPTicks.Add(m_StatusPanel.Q<VisualElement>("XPTick" + i));
            }

            // DoT Status Labels
            m_BleedStatusLabel = m_StatusPanel.Q<Label>("BleedStatus");
            m_PoisonStatusLabel = m_StatusPanel.Q<Label>("PoisonStatus");
            m_BurnStatusLabel = m_StatusPanel.Q<Label>("BurnStatus");
            m_HealStatusLabel = m_StatusPanel.Q<Label>("HealStatus");
        }
        else
        {
            // (디버깅용)
            Debug.LogWarning("[PlayerController] 'PlayerStatus'를 찾지 못했습니다. UI 구성을 확인하세요.");
        }

        // 2. 카드 슬롯 연결 및 D&D / 툴팁 이벤트 등록 (Fixed_Player.uxml)
        Slots.Clear();
        m_CardImageLayers.Clear();
        m_RoleUIContainers.Clear();
        m_CooldownOverlays.Clear();
        m_CostContainers.Clear();
        m_CostLabels.Clear();

        m_PlayerParty = playerUiRoot.Q<VisualElement>("PlayerPartyContainer"); 

        for (int i = 0; i < 7; i++)
        {
            // 이름 규칙: CardSlot1
            VisualElement slot = playerUiRoot.Q<VisualElement>($"CardSlot_{i}");
            Slots.Add(slot);

            if (slot != null)
            {
                // 슬롯 내부 요소 연결
                m_CardImageLayers.Add(slot.Q<VisualElement>("CardImage"));
                m_RoleUIContainers.Add(slot.Q<VisualElement>("RoleUIContatiner"));
                m_CooldownOverlays.Add(slot.Q<VisualElement>("CooldownOverlay"));
                m_CostContainers.Add(slot.Q<VisualElement>("CostContainer"));
                m_CostLabels.Add(slot.Q<Label>("CostLabel"));

                // D&D 핸들러 부착
                DragAndDropHandler manipulator = new DragAndDropHandler(slot, mainRoot, this);
                slot.AddManipulator(manipulator);

                int currentIndex = i;
                slot.RegisterCallback<PointerEnterEvent>(evt => OnPointerEnterSlot(currentIndex, evt));
                slot.RegisterCallback<PointerLeaveEvent>(evt => OnPointerLeaveSlot());
            }
            else
            {

                Debug.LogError($"[PlayerController] 'CardSlot_{i}'를 찾을 수 없습니다!");
                m_CardImageLayers.Add(null);
                m_RoleUIContainers.Add(null);
                m_CooldownOverlays.Add(null);
                m_CostContainers.Add(null);
                m_CostLabels.Add(null);
            }
        }

        // ---------------------------------------------------------
        // 3. 툴팁 UI 요소 연결 (MainLayout.uxml 내부)
        // ---------------------------------------------------------
        m_TooltipRoot = mainRoot.Q<VisualElement>("TooltipRoot"); // MainLayout에 추가한 이름

        if (m_TooltipRoot != null)
        {
            m_TooltipName = m_TooltipRoot.Q<Label>("TooltipName");
            m_TooltipTagContainer = m_TooltipRoot.Q<VisualElement>("TooltipTagContainer");
            m_TooltipSkillDesc = m_TooltipRoot.Q<Label>("TooltipSkillDesc");
            m_TooltipCooldown = m_TooltipRoot.Q<Label>("TooltipCooldown");
            m_TooltipQuestContainer = m_TooltipRoot.Q<VisualElement>("TooltipQuestContainer");
            m_TooltipQuestTitle = m_TooltipRoot.Q<Label>("QuestName");
            m_TooltipQuestDesc = m_TooltipRoot.Q<Label>("TooltipQuestDesc");
            m_TooltipQuestStatus = m_TooltipRoot.Q<Label>("TooltipQuestStatus");
            m_TooltipStatContainer = m_TooltipRoot.Q<VisualElement>("TooltipStatContainer");
            m_TooltipCritContainer = m_TooltipRoot.Q<VisualElement>("TooltipCritContainer");
            m_TooltipCritChance = m_TooltipRoot.Q<Label>("TooltipCritChance");
            m_TooltipDurabilityContainer = m_TooltipRoot.Q<VisualElement>("TooltipDurabilityContainer");
            m_TooltipDurability = m_TooltipRoot.Q<Label>("TooltipDurability");
            m_TooltipFlavorText = m_TooltipRoot.Q<Label>("TooltipFlavorText");
            m_TooltipDivider1 = m_TooltipRoot.Q<VisualElement>("TooltipDivider1");
            m_TooltipDivider2 = m_TooltipRoot.Q<VisualElement>("TooltipDivider2");

            // 초기엔 숨기기
            m_TooltipRoot.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogError("[PlayerController] MainLayout에서 'TooltipRoot'를 찾을 수 없습니다!");
        }

        // ---------------------------------------------------------
        // 4. 초기 화면 갱신 (데이터 반영)
        // ---------------------------------------------------------
        UpdateHealthUI();
        UpdateXPUI();
        UpdateDoTUI();
        UpdateLifeUI();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUDLife(CurrentLife, MaxLife);
        }

        for (int i = 0; i < 7; i++)
        {
            UpdateCardSlotUI(i);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoldUI(Gold);
        }

        Debug.Log("PlayerController UI 초기화 및 연결 완료");
    }

    public virtual void LoseLife()
    {
        CurrentLife--;
        Debug.LogWarning($"[라이프 감소] 남은 라이프: {CurrentLife}");

        // 자신의 UI 갱신 함수 호출
        UpdateLifeUI();

        if (CurrentLife <= 0)
        {
            Debug.LogError("게임 오버!");
        }
    }

    public void UpdateLifeUI()
    {
        if (m_LifeContainer == null) return;

        m_LifeContainer.Clear();

        // MaxLife만큼 하트를 생성 (보통 3개)
        for (int i = 0; i < MaxLife; i++)
        {
            VisualElement heart = new VisualElement();
            heart.AddToClassList("heart-icon");

            // 현재 남은 라이프 수치와 비교하여 클래스 결정
            if (i < CurrentLife)
                heart.AddToClassList("heart-full");
            else
                heart.AddToClassList("heart-empty");

            m_LifeContainer.Add(heart);
        }
    }
    public void UpdatePartyUI()
    {
        for (int i = 0; i < 7; i++)
        {
            UpdateCardSlotUI(i);
        }
    }

    public virtual void BattleUpdate(float deltaTime)
    {
        // 내 카드들의 쿨타임 회전 및 스킬 발동
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null)
            {
                m_Cards[i].UpdateCooldown(deltaTime);

                // 스킬 발동
                if (m_Cards[i].CurrentCooldown <= 0f)
                {
                    m_Cards[i].TriggerSkill();
                }
                UpdateCardSlotUI(i);
            }
        }
        // DoT 데미지 타이머
        ProcessDoTs(deltaTime);
    }

    // 카드 슬롯에 마우스가 들어왔을 때 호출
    private void OnPointerEnterSlot(int slotIndex, PointerEnterEvent evt)
    {
        // 해당 슬롯에 카드가 있는지 확인
        Card card = GetCardAtIndex(slotIndex);
        if (card == null) return; // 카드가 없으면 무시

        // 이전에 예약된 툴팁 스케줄이 있다면 취소
        m_TooltipScheduler?.Pause();

        // 'TOOLTIP_DELAY_MS' 밀리초 후 ShowTooltip 함수 실행
        VisualElement slotElement = evt.currentTarget as VisualElement;
        m_TooltipScheduler = slotElement.schedule
            .Execute(() => ShowTooltip(card, slotElement))
            .StartingIn(TOOLTIP_DELAY_MS);
    }

    // 카드 슬롯에서 마우스가 나갔을 때 호출
    private void OnPointerLeaveSlot()
    {
        // 예약되어 있던 툴팁 스케줄 즉시 취소
        m_TooltipScheduler?.Pause();

        // 2. 툴팁 UI 즉시 숨김
        if (m_TooltipRoot != null)
        {
            m_TooltipRoot.style.display = DisplayStyle.None;
        }
    }

    // 스케줄러에 의해 딜레이 이후 실제 툴팁을 띄우는 함수
    private void ShowTooltip(Card card, VisualElement slotElement)
    {
        // null 체크
        if (m_TooltipRoot == null || card == null || slotElement == null) return;

        // --- 카드 데이터로 툴팁 UI 내용 채우기 ---
        // 이름 (Key -> 번역)
        m_TooltipName.text = LocalizationManager.GetText(card.CardNameKey);
        // 태그 (동적 Label 생성)
        m_TooltipTagContainer.Clear();
        m_TooltipTagContainer.style.display = DisplayStyle.None;
        if (card.TagKeys != null && card.TagKeys.Count > 0)
        {
            foreach (string tagKey in card.TagKeys)
            {
                string translatedTag = LocalizationManager.GetText(tagKey);
                Label tagLabel = new Label(translatedTag);
                tagLabel.AddToClassList("tooltip-tag-label");
                m_TooltipTagContainer.Add(tagLabel);
            }
            m_TooltipTagContainer.style.display = DisplayStyle.Flex;
        }

        // --- 쿨타임 표시 (별도 라벨) ---
        if (card.ShowCooldownUI && card.BaseCooldownTime > 0)
        {
            m_TooltipCooldown.text = LocalizationManager.GetText("stat_cooldown", card.BaseCooldownTime.ToString());
            m_TooltipCooldown.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipCooldown.style.display = DisplayStyle.None;
        }
        //  메인 스킬 설명
        m_TooltipSkillDesc.text = LocalizationManager.GetText(card.CardSkillDescriptionKey);
        // 부가 스탯/효과 목록
        m_TooltipStatContainer.Clear();

        System.Action<string, float> AddStatLine = (string key, float value) =>
        {
            if (value > 0)
            {
                Label line = new Label(LocalizationManager.GetText(key, value.ToString()));
                line.AddToClassList("tooltip-stat-line");
                m_TooltipStatContainer.Add(line);
            }
        };

        System.Action<string> AddTextLine = (string text) =>
        {
            Label line = new Label(text);
            line.AddToClassList("tooltip-stat-line");
            m_TooltipStatContainer.Add(line);
        };

        // 모든 스탯 추가
        AddStatLine("stat_damage", card.GetCurrentDamage());
        AddStatLine("stat_shield", card.GetCurrentShield());
        AddStatLine("stat_heal", card.GetCurrentHeal());
        AddStatLine("stat_crit_chance", card.GetCurrentCritChance() * 100f);
        AddStatLine("stat_apply_bleed", card.GetCurrentBleedStacks());
        AddStatLine("stat_apply_poison", card.GetCurrentPoisonStacks());
        AddStatLine("stat_apply_burn", card.GetCurrentBurnStacks());
        AddStatLine("stat_apply_heal_dot", card.GetCurrentHealStacks());
        AddStatLine("stat_apply_freeze", card.GetCurrentFreezeDuration());
        AddStatLine("stat_apply_haste", card.GetCurrentHasteDuration());
        AddStatLine("stat_apply_slow", card.GetCurrentSlowDuration());
        AddStatLine("stat_apply_cooldown_reduction", card.GetCurrentCooldownReduction());
        AddStatLine("stat_apply_cooldown_increase", card.GetCurrentCooldownIncrease());
        AddStatLine("stat_apply_echo", card.GetCurrentEchoStacks());
        AddStatLine("stat_apply_shock", card.GetCurrentShockDuration());
        AddStatLine("stat_apply_sturdy", card.GetCurrentSturdyDuration());
        AddStatLine("stat_price_inflate", card.GetCurrentPriceInflate());
        AddStatLine("stat_price_extort", card.GetCurrentPriceExtort());
        AddStatLine("stat_triggers_shuffle", card.TriggersTargetShuffle);
        AddStatLine("stat_triggers_chain", card.TriggersChainCount);

        // 소환
        if (card.SummonCount > 0 && !string.IsNullOrEmpty(card.SummonCardNameKey))
        {
            // 소환할 카드 이름 번역
            string summonName = LocalizationManager.GetText(card.SummonCardNameKey);
            // 최종 문장 완성 ("소환: "카드 이름" x 2")
            string text = LocalizationManager.GetText("stat_summon", summonName, card.SummonCount);
            AddTextLine(text);
        }

        // 유언
        if (!string.IsNullOrEmpty(card.DeathrattleDescKey))
        {
            // 유언 효과 설명 번역 
            string effectDesc = LocalizationManager.GetText(card.DeathrattleDescKey);
            // 최종 문장 완성 ("유언: ~~~")
            string text = LocalizationManager.GetText("stat_deathrattle", effectDesc);
            AddTextLine(text);
        }

        // 변이 
        if (card.PolymorphDurationToApply > 0 && !string.IsNullOrEmpty(card.PolymorphTargetNameKey))
        {
            // 변이 대상 이름
            string targetName = LocalizationManager.GetText(card.PolymorphTargetNameKey);

            // 최종 문장 완성
            string text = LocalizationManager.GetText("stat_apply_polymorph", targetName, card.PolymorphDurationToApply);

            // 텍스트 라인 추가
            Label line = new Label(text);
            line.AddToClassList("tooltip-stat-line");
            m_TooltipStatContainer.Add(line);
        }

        // 컨테이너 보이기/숨기기
        if (m_TooltipStatContainer.childCount > 0)
        {
            m_TooltipStatContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipStatContainer.style.display = DisplayStyle.None;
        }

        // --- [이하 동일] 퀘스트, 내구도, 플레이버 텍스트 ---

        // 퀘스트 (조건부 표시)
        if (card.HasQuest)
        {
            m_TooltipQuestTitle.text = LocalizationManager.GetText(card.QuestTitleKey);
            m_TooltipQuestDesc.text = LocalizationManager.GetText(card.QuestDescriptionKey);
            bool isDone = card.IsQuestComplete;
            string statusKey = isDone ? "quest_status_complete" : "quest_status_incomplete";
            m_TooltipQuestStatus.text = LocalizationManager.GetText(statusKey);
            m_TooltipQuestStatus.EnableInClassList("quest-status-complete", isDone);
            m_TooltipQuestStatus.EnableInClassList("quest-status-incomplete", !isDone);
            m_TooltipQuestContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipQuestContainer.style.display = DisplayStyle.None;
        }

        // 내구도 (조건부 표시)
        if (card.Durability > -1)
        {
            m_TooltipDurability.text = LocalizationManager.GetText("stat_durability", card.Durability.ToString());
            m_TooltipDurabilityContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipDurabilityContainer.style.display = DisplayStyle.None;
        }

        // 플레이버 텍스트 (Key -> 번역)
        string flavor = LocalizationManager.GetText(card.FlavorTextKey);
        if (!string.IsNullOrEmpty(flavor) && !flavor.StartsWith("["))
        {
            m_TooltipFlavorText.text = $"\"{flavor}\"";
            m_TooltipFlavorText.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipFlavorText.style.display = DisplayStyle.None;
        }

        //Divider1 (태그/쿨타임과 스킬설명 사이)
        bool isTopSectionVisible = (m_TooltipTagContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipCooldown.style.display == DisplayStyle.Flex);

        m_TooltipDivider1.style.display = isTopSectionVisible ? DisplayStyle.Flex : DisplayStyle.None;

        // Divider2 (부가 스탯/퀘스트/내구도와 플레이버 텍스트 사이)
        bool isMidSectionVisible = (
                                    m_TooltipQuestContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipDurabilityContainer.style.display == DisplayStyle.Flex);

        bool isFlavorTextVisible = (m_TooltipFlavorText.style.display == DisplayStyle.Flex);

        m_TooltipDivider2.style.display = (isMidSectionVisible && isFlavorTextVisible) ? DisplayStyle.Flex : DisplayStyle.None;

        // --- 툴팁 위치 조절 (슬롯의 오른쪽 상단) ---
        Rect slotRect = slotElement.worldBound;
        float offsetX = 10f;
        m_TooltipRoot.style.left = slotRect.xMax + offsetX;
        m_TooltipRoot.style.top = slotRect.yMin;

        // ---  툴팁 UI---
        m_TooltipRoot.style.display = DisplayStyle.Flex;
    }

    /// (공통) 몬스터가 나를 공격할 때 호출하는 함수
    public virtual void TakeDamage(float amount)
    {
        // 최종 피해량 계산
        float finalDamage = amount;
        if (m_IsShocked)
        {
            finalDamage *= SHOCK_MULTIPLIER; // 20% 증폭
        }
        else if (m_IsSturdy) // (중첩 안 됨)
        {
            finalDamage *= STURDY_MULTIPLIER; // 20% 감소
        }
        finalDamage = Mathf.Round(finalDamage);
        float damageRemaining = finalDamage;

        // 쉴드 깎음
        if (CurrentShield > 0)
        {
            if (damageRemaining >= CurrentShield)
            {
                damageRemaining -= CurrentShield;
                CurrentShield = 0;
            }
            else
            {
                CurrentShield -= damageRemaining;
                damageRemaining = 0;
            }
        }

        // 체력 깎음
        if (damageRemaining > 0)
        {
            CurrentHP -= damageRemaining;
        }

        Debug.Log($"[플레이어] {finalDamage} 피해 받음! (쉴드 {CurrentShield} 남음, 체력 {CurrentHP} 남음)");

        UpdateHealthUI();
        m_BattleManager.CheckBattleStatus();
    }

    public virtual void IncreaseMaxHP(float amount)
    {
        if (amount <= 0) return;

        MaxHP += amount;
        CurrentHP = MaxHP; // 현재 체력 최대로 회복

        Debug.Log($"[플레이어] 최대 체력 영구 증가! (새로운 최대 HP: {MaxHP})");

        // UI 즉시 업데이트
        UpdateHealthUI();
    }

    public virtual void DecreaseMaxHP(float amount)
    {
        if (amount <= 0) return;

        MaxHP -= amount;

        // 최대 체력이 1 미만 방지
        if (MaxHP < 1)
        {
            MaxHP = 1;
        }

        if (CurrentHP > MaxHP)
        {
            CurrentHP = MaxHP;
        }

        Debug.LogWarning($"[플레이어] 최대 체력을 담보로 지불! (새로운 최대 HP: {MaxHP})");

        // UI 즉시 업데이트
        UpdateHealthUI();
    }

    // 쉴드 추가
    public virtual void AddShield(float amount)
    {
        CurrentShield += amount;
        Debug.Log($"[플레이어] 쉴드 {amount} 획득! (총 쉴드: {CurrentShield})");
        UpdateHealthUI();
    }

    // 명성/경험치 획득 함수
    public virtual void AddExperience(int amount)
    {
        CurrentXP += amount;
        Debug.Log($"[플레이어] 경험치 {amount} 획득! (총 XP: {CurrentXP}/{MaxXP})");

        //10칸이 다 차면 레벨업 
        while (CurrentXP >= MaxXP)
        {
            CurrentLevel++;
            CurrentXP -= MaxXP; // 초과분

            MaxHP += 10; // 레벨업 시 최대 체력 증가 (추후 조정)
            CurrentHP = MaxHP; // 레벨업 시 체력 회복)

            Debug.LogWarning($"[플레이어] 레벨 업! {CurrentLevel} 레벨 달성! (남은 XP: {CurrentXP})");
        }

        UpdateHealthUI(); // 체력 회복 UI 업데이트
        UpdateXPUI();
    }

    // (공통) 나의 '타겟'(몬스터)이 누구인지 알려주는 함수.
    public MonsterController GetTarget()
    {
        return m_Target;
    }

    // (공통) BattleManager가 나의 '타겟'을 지정해주는 함수.
    public void SetTarget(MonsterController target)
    {
        this.m_Target = target;
    }

    // --- 7. 위치 호출 함수 ---
    /// 내 덱(m_Cards)의 특정 인덱스에 있는 카드 반환 : (범위를 벗어나면 null을 반환)
    public Card GetCardAtIndex(int index)
    {
        if (index >= 0 && index < 7 && m_Cards[index] != null)
        {
            return m_Cards[index];
        }
        return null;
    }

    /// [인접-왼쪽] 왼쪽에 있는 카드를 반환
    public Card GetLeftNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex - 1);
    }

    /// [인접-오른쪽] 오른쪽에 있는 카드를 반환
    public Card GetRightNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex + 1);
    }

    /// [상대 위치] 맞은편에 있는 몬스터 카드 반환
    public Card GetOppositeCard(int myIndex)
    {
        if (m_Target != null)
        {
            return m_Target.GetCardAtIndex(myIndex);
        }
        return null;
    }

    // --- 8. 상태 이상 헬퍼 함수 ---
    /// 내 카드 중 '면역이 아닌' 무작위 카드 N개에 상태 이상을 적용
    public void ApplyStatusToRandomCards(int count, StatusEffectType effectType, float duration, string extraData = "")
    {
        if (m_BattleManager.IsBattleEnded) return;
        // 0~6번 슬롯 인덱스가 담긴 리스트 생성
        List<int> slotIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 리스트 무작위 셔플
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 적용에 성공한 횟수를 카운팅
        int successCount = 0;

        // 무작위로 섞인 슬롯 순서대로 확인
        foreach (int index in slotIndices)
        {
            Card card = GetCardAtIndex(index); // 

            // 슬롯이 비어있지 않은지 확인
            if (card != null)
            {
                // 카드에게 효과 적용 시도
                // (이 코드가 작동하려면 Card.cs에 ApplyStatusEffect 함수가 있어야 됨)
                if (card.ApplyStatusEffect(effectType, duration, extraData))
                {
                    // 적용에 성공했으면, 카운팅
                    successCount++;
                }
            }

            // 목표한 횟수(count)만큼 성공했으면, 즉시 종료
            if (successCount >= count)
            {
                break;
            }
        }
    }

    /// 이 영주에게 스탯 중첩 추가
    public virtual void ApplyLordStatus(StatusEffectType effectType, int stacks)
    {
        if (m_BattleManager.IsBattleEnded) return;
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks += stacks;
                break;
            case StatusEffectType.Poison:
                PoisonStacks += stacks;
                break;
            case StatusEffectType.Burn:
                BurnStacks += stacks;
                break;
            case StatusEffectType.Heal:
                HealStacks += stacks;
                break;

            // --- Buff/Debuff (지속시간) ---
            case StatusEffectType.Shock: // 충격
                m_IsShocked = true;
                m_ShockTimer = Mathf.Max(m_ShockTimer, stacks); // 더 긴 시간으로 갱신
                m_IsSturdy = false; // 견고 해제
                m_SturdyTimer = 0f;
                Debug.Log($"[플레이어] '감전' 효과! ({stacks}초)");
                break;

            case StatusEffectType.Sturdy: // 견고
                m_IsSturdy = true;
                m_SturdyTimer = Mathf.Max(m_SturdyTimer, stacks); // 더 긴 시간으로 갱신
                m_IsShocked = false; // 감전 해제
                m_ShockTimer = 0f;
                Debug.Log($"[플레이어] '견고' 효과! ({stacks}초)");
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }


    /// 특정 상태 이상 중첩을 '일정 수치(정수)'만큼 감소
    public virtual void ReduceLordStatus(StatusEffectType effectType, int amount)
    {
        if (m_BattleManager.IsBattleEnded) return;
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks = Mathf.Max(0, BleedStacks - amount);
                break;
            case StatusEffectType.Poison:
                PoisonStacks = Mathf.Max(0, PoisonStacks - amount);
                break;
            case StatusEffectType.Burn:
                BurnStacks = Mathf.Max(0, BurnStacks - amount);
                break;
            case StatusEffectType.Heal:
                HealStacks = Mathf.Max(0, HealStacks - amount);
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    /// 특정 상태 이상 중첩을 '일정 퍼센트(%)'만큼 감소
    public virtual void ReduceLordStatusPercent(StatusEffectType effectType, float percent)
    {
        if (m_BattleManager.IsBattleEnded) return;
        float clampedPercent = Mathf.Clamp01(percent);
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks = Mathf.FloorToInt(BleedStacks * (1.0f - clampedPercent));
                break;
            case StatusEffectType.Poison:
                PoisonStacks = Mathf.FloorToInt(PoisonStacks * (1.0f - clampedPercent));
                break;
            case StatusEffectType.Burn:
                BurnStacks = Mathf.FloorToInt(BurnStacks * (1.0f - clampedPercent));
                break;
            case StatusEffectType.Heal:
                HealStacks = Mathf.FloorToInt(HealStacks * (1.0f - clampedPercent));
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    // 변이 
    public void MutateCard(int slotIndex, string newCardID, float duration)
    {
        if (slotIndex < 0 || slotIndex >= 7) return;
        if (m_Cards[slotIndex] == null) return;

        // 1. 원본 카드 백업
        Card originalCard = m_Cards[slotIndex];

        // 2. 새 카드 생성 (팩토리 이용)
        Card newCard = CardFactory.CreateCard(newCardID, this, slotIndex);
        if (newCard == null) return; // 생성 실패 시 중단

        // 3. [핵심!] 변이 정보 설정 (원본 주입 + 시간 설정)
        newCard.OriginalForm = originalCard;
        newCard.PolymorphTimer = duration;

        // 4. 배열 교체 및 UI 갱신
        m_Cards[slotIndex] = newCard;
        UpdateCardSlotUI(slotIndex);

        Debug.Log($"[{originalCard.CardNameKey}] -> [{newCard.CardNameKey}] 변이됨! ({duration}초)");
    }

    // 변이 해제 및 복구
    public void RevertMutation(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 7) return;
        Card currentCard = m_Cards[slotIndex];

        // 변이된 카드가 맞는지 확인 (OriginalForm을 가지고 있는지)
        if (currentCard != null && currentCard.OriginalForm != null)
        {
            // 1. 품고 있던 원본 카드 꺼내기
            Card originalCard = currentCard.OriginalForm;

            // 2. 덱에 원본 복귀
            m_Cards[slotIndex] = originalCard;

            // 3. 연결 끊기 (중요: 참조 제거)
            currentCard.OriginalForm = null;

            // 4. UI 갱신
            UpdateCardSlotUI(slotIndex);
            Debug.Log($"변이 해제! [{originalCard.CardNameKey}] 복귀 완료.");
        }
    }

    // 셔플
    public void ShuffleDeck()
    {
        // 피셔-예이츠 셔플 (Fisher-Yates Shuffle) 알고리즘
        for (int i = 0; i < 7; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, 7);

            // 배열의 요소(Card) 위치 교환 (Swap)
            Card temp = m_Cards[i];
            m_Cards[i] = m_Cards[randomIndex];
            m_Cards[randomIndex] = temp;
        }

        // 내부 데이터 업데이트 및 UI 갱신
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null)
            {
                // 카드가 자신의 바뀐 슬롯 번호를 기억하게 함
                m_Cards[i].SetSlotIndex(i);
            }

            // UI를 바뀐 카드 정보로 덮어씌움
            UpdateCardSlotUI(i);
        }

        UnityEngine.Debug.LogWarning("진영의 카드가 '재배열' 되었습니다!");
    }

    // 덱을 원래 순서대로 복구
    public void RevertShuffle()
    {
        // 임시 리스트 생성
        List<Card> allCards = new List<Card>();
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) allCards.Add(m_Cards[i]);
            m_Cards[i] = null; // 일단 배열 비우기
        }

        // 원래 위치 기준 카드 원위치
        foreach (Card card in allCards)
        {
            int originalIndex = card.OriginalSlotIndex;

            if (originalIndex >= 0 && originalIndex < 7 && m_Cards[originalIndex] == null)
            {
                m_Cards[originalIndex] = card;
                card.SetSlotIndex(originalIndex); // 현재 위치 정보 갱신
            }
            else
            {
                // (예외 처리: 원래 자리에 누가 들어갔거나 유효하지 않으면 빈 곳 찾기)
                for (int i = 0; i < 7; i++)
                {
                    if (m_Cards[i] == null) { m_Cards[i] = card; card.SetSlotIndex(i); break; }
                }
            }
        }

        // UI 전체 갱신
        for (int i = 0; i < 7; i++) UpdateCardSlotUI(i);

        UnityEngine.Debug.Log("진영 재배열! 원래 대형으로 복귀했습니다.");
    }

    // 교란
    public void PerformDisruption(int exchangeCount)
    {
        if (exchangeCount <= 0) return;
        int actualCount = Mathf.Min(exchangeCount, 7);

        for (int i = 0; i < actualCount; i++)
        {
            // 무작위 인덱스 두 개를 선택
            int indexA = UnityEngine.Random.Range(0, 7);
            int indexB = UnityEngine.Random.Range(0, 7);

            // 같은 위치면 다시 고름
            if (indexA == indexB)
            {
                i--; // 카운트를 줄여서 한 번 더 시도
                continue;
            }

            // 카드 위치 교환
            Card cardA = m_Cards[indexA];
            Card cardB = m_Cards[indexB];

            m_Cards[indexA] = cardB;
            m_Cards[indexB] = cardA;

            // 카드 내부의 슬롯 인덱스 업데이트 (OriginalSlotIndex는 그대로 둡니다)
            if (cardA != null) cardA.SetSlotIndex(indexB);
            if (cardB != null) cardB.SetSlotIndex(indexA);

            // 교환된 두 슬롯 UI 갱신
            UpdateCardSlotUI(indexA);
            UpdateCardSlotUI(indexB);
        }

        UnityEngine.Debug.LogWarning($"진영에 [{actualCount}회] 교란(Disruption)이 발생했습니다.");
    }

    // --- 9. DoT 데미지 처리---
    // '개별' 타이머로 DoT 데미지를 계산 및 적용
    private void ProcessDoTs(float deltaTime)
    {
        if (m_BattleManager.IsBattleEnded) return;
        // (1) 출혈 (1.5초 틱, 쉴드부터 깎음)
        if (BleedStacks > 0)
        {
            m_BleedTickTimer -= deltaTime;
            if (m_BleedTickTimer <= 0f)
            {
                float damage = BleedStacks * 1; // (스택당 1데미지)
                TakeDamage(damage);
                Debug.Log($"[DoT] 출혈로 {damage} 피해!");
                m_BleedTickTimer = 1.5f; // 1.5초 타이머 초기화
            }
        }

        // (2) 중독 (3초 틱, 쉴드 무시)
        if (PoisonStacks > 0)
        {
            m_PoisonTickTimer -= deltaTime;
            if (m_PoisonTickTimer <= 0f)
            {
                float damage = PoisonStacks * 1;
                CurrentHP -= damage; // 쉴드를 무시하고 체력에 직접 피해
                Debug.Log($"[DoT] 중독으로 {damage} 피해! (쉴드 무시)");
                UpdateHealthUI(); // 체력이 바로 바뀌었으니 UI 업데이트
                m_PoisonTickTimer = 3.0f; // 3초 타이머 초기화
            }
        }

        // (3) 화상 (0.5초 틱, 쉴드부터 깎음, 1스택 '고갈')
        if (BurnStacks > 0)
        {
            m_BurnTickTimer -= deltaTime;
            if (m_BurnTickTimer <= 0f)
            {
                float damage = BurnStacks * 1; // (예: 틱당 1데미지)
                TakeDamage(damage);
                Debug.Log($"[DoT] 화상으로 {damage} 피해!");

                // 피해를 입힌 후 1스택 감소
                BurnStacks -= 1;
                UpdateDoTUI(); // 스택이 바뀌었으니 UI 업데이트

                m_BurnTickTimer = 0.5f; // 0.5초 타이머 초기화
            }
        }

        // (4) 지속 회복 (2초 틱, 체력 회복)
        if (HealStacks > 0)
        {
            m_HealTickTimer -= deltaTime;
            if (m_HealTickTimer <= 0f)
            {
                float healAmount = HealStacks * 1; // (스택당 1 회복)
                AddHealth(healAmount); // 체력 추가 함수 호출
                Debug.Log($"[HoT] 지속 회복으로 {healAmount} 회복!");
                m_HealTickTimer = 2.0f; // 2초 타이머 초기화
            }
        }

        // (5) 충격 타이머
        if (m_IsShocked)
        {
            m_ShockTimer -= deltaTime;
            if (m_ShockTimer <= 0f)
            {
                m_IsShocked = false;
                Debug.Log("[플레이어] '감전' 상태 해제!");
                // (나중에 UI가 있다면 UpdateStatusEffectUI() 호출)
            }
        }

        // (6) 견고 타이머
        if (m_IsSturdy)
        {
            m_SturdyTimer -= deltaTime;
            if (m_SturdyTimer <= 0f)
            {
                m_IsSturdy = false;
                Debug.Log("[플레이어] '견고' 상태 해제!");
                // (나중에 UI가 있다면 UpdateStatusEffectUI() 호출)
            }
        }
    }

    //체력 회복
    public virtual void AddHealth(float amount)
    {
        if (m_BattleManager.IsBattleEnded) return;

        CurrentHP += amount;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        UpdateHealthUI();
    }

    // --- 10. UI 업데이트 함수 ---
    protected virtual void UpdateHealthUI()
    {
        // 체력 바
        if (m_HealthBarFill != null)
        {
            float healthPercent = (MaxHP > 0) ? (CurrentHP / MaxHP) : 0f;
            healthPercent = Mathf.Clamp01(healthPercent);
            m_HealthBarFill.style.width = Length.Percent(healthPercent * 100f);
        }

        // 체력 텍스트(Label)
        if (m_HealthLabel != null)
        {
            m_HealthLabel.text = $"{Mathf.CeilToInt(CurrentHP)}";
        }

        // 쉴드 텍스트(Label)
        if (m_ShieldLabel != null)
        {
            m_ShieldLabel.text = $"{Mathf.CeilToInt(CurrentShield)}";
        }

        // 쉴드 바(Fill)
        if (m_ShieldBarFill != null)
        {
            if (CurrentShield > 0)
            {
                m_ShieldBarFill.style.display = DisplayStyle.Flex;
                float shieldPercent = Mathf.Clamp(CurrentShield / MaxHP, 0, 1f);
                m_ShieldBarFill.style.width = Length.Percent(shieldPercent * 100f);
            }
            else
            {
                m_ShieldBarFill.style.display = DisplayStyle.None;
            }
        }
    }

    // 레벨/경험치(10칸 네모) UI를 업데이트
    protected virtual void UpdateXPUI()
    {
        // 레벨 텍스트(Label)
        if (m_LevelLabel != null)
        {
            m_LevelLabel.text = $"LV. {CurrentLevel}";
        }

        // 10칸 네모(Ticks) UI 업데이트
        int filledTicks = CurrentXP;

        for (int i = 0; i < 10; i++)
        {
            if (m_XPTicks[i] != null)
            {
                if (i < filledTicks)
                {
                    // (i)번 칸이 채워져야 함 (예: 0, 1, 2)
                    m_XPTicks[i].AddToClassList("xp-tick-filled");
                }
                else
                {
                    // (i)번 칸이 비어있어야 함
                    m_XPTicks[i].RemoveFromClassList("xp-tick-filled");
                }
            }
        }
    }

    // DoT 중첩을 UI 라벨에 업데이트하고, 0이면 숨김.
    protected virtual void UpdateDoTUI()
    {
        // 출혈 UI
        if (m_BleedStatusLabel != null)
        {
            if (BleedStacks > 0)
            {
                m_BleedStatusLabel.text = $"{BleedStacks}";
                m_BleedStatusLabel.style.display = DisplayStyle.Flex; // 보이기
            }
            else
            {
                m_BleedStatusLabel.style.display = DisplayStyle.None; // 숨기기
            }
        }

        // 중독 UI
        if (m_PoisonStatusLabel != null)
        {
            if (PoisonStacks > 0)
            {
                m_PoisonStatusLabel.text = $"{PoisonStacks}";
                m_PoisonStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_PoisonStatusLabel.style.display = DisplayStyle.None;
            }
        }

        // 화상 UI
        if (m_BurnStatusLabel != null)
        {
            if (BurnStacks > 0)
            {
                m_BurnStatusLabel.text = $"{BurnStacks}";
                m_BurnStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_BurnStatusLabel.style.display = DisplayStyle.None;
            }
        }

        // 회복 UI
        if (m_HealStatusLabel != null)
        {
            if (HealStacks > 0)
            {
                m_HealStatusLabel.text = $"{HealStacks}";
                m_HealStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_HealStatusLabel.style.display = DisplayStyle.None;
            }
        }
    }

    public virtual void UpdateCardSlotUI(int index)
    {
        // 인덱스 유효 여부 확인
        if (index < 0 || index >= 7) return;
        if (Slots.Count <= index) return;
        // C# 데이터 & UI 슬롯 호출
        Card cardData = m_Cards[index];
        VisualElement slotUI = Slots[index];

        if (slotUI == null) return;

        // UI 내부 요소 호출
        VisualElement cooldownOverlay = m_CooldownOverlays[index];
        VisualElement roleUIContainer = m_RoleUIContainers[index];
        VisualElement cardImageLayer = m_CardImageLayers[index];
        VisualElement costContainer = m_CostContainers[index];
        Label costLabel = m_CostLabels[index];

        // 카드 데이터에 따라 UI를 업데이트
        if (cardData != null) 
        {
            // 카드 이미지 적용
            if (cardImageLayer != null)
            {
                cardImageLayer.style.backgroundImage = new StyleBackground(cardData.CardImage);
            }
            if (costContainer != null && costLabel != null)
            {
                // Card.cs의 CardPrice가 0보다 클 때만 UI를 켭니다.
                if (cardData.CardPrice > 0)
                {
                    costContainer.style.display = DisplayStyle.Flex;
                    costLabel.text = cardData.GetSellPrice().ToString();
                }
                else
                {
                    costContainer.style.display = DisplayStyle.None;
                }
            }

            // 쿨타임 UI 업데이트
            if (cooldownOverlay != null)
            {
                // 패시브 카드인지 확인
                if (cardData.ShowCooldownUI)
                {
                    //카드가 '빙결' 상태인지 먼저 확인
                    if (cardData.IsFrozen())
                    {
                        cooldownOverlay.style.display = DisplayStyle.Flex;
                        cooldownOverlay.style.height = Length.Percent(100f);
                        cooldownOverlay.AddToClassList("cooldown-overlay-frozen");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                    }
                    // 일반 쿨타임 상태인지 확인
                    
                    else if (cardData.CurrentCooldown > 0.01f)
                    {
                        cooldownOverlay.style.display = DisplayStyle.Flex; // 오버레이
                        float maxCooldown = cardData.GetCurrentCooldownTime(); // 
                        float percent = (maxCooldown > 0) ? (cardData.CurrentCooldown / maxCooldown) : 1f; 
                        cooldownOverlay.style.height = Length.Percent(percent * 100f); // '남은 %'만큼 높이를 조절
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-frozen"); // 빙결 제거

                        // 가속 상태 확인
                        if (cardData.IsHasted())
                        {
                            cooldownOverlay.AddToClassList("cooldown-overlay-hasted");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        }
                        // 감속 상태 확인
                        else if (cardData.IsSlowed())
                        {
                            cooldownOverlay.AddToClassList("cooldown-overlay-slowed");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        }
                        // 일반' 쿨타임 상태입니다.
                        else
                        {
                            // 모든 특수 색상 제거
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        }
                    }
                    // 기본 준비 상태
                    else
                    {
                        cooldownOverlay.style.display = DisplayStyle.None;
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-frozen");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        cooldownOverlay.style.height = Length.Percent(100f);
                    }
                }
                else
                {
                    cooldownOverlay.style.display = DisplayStyle.None;
                }
            }
                // 역할 UI 업데이트
            if (roleUIContainer != null)
            {   
                roleUIContainer.Clear();
                // 우선순위에 따라 아이콘을 동적 생성
                // 우선순위 1: 대미지
                float currentDamage = cardData.GetCurrentDamage();
                if (currentDamage > 0) { CreateRoleIcon(roleUIContainer, "role-attacker", currentDamage.ToString()); }

                // 우선순위 2: 상태이상
                // 출혈
                int currentBleed = cardData.GetCurrentBleedStacks();
                if (currentBleed > 0) { CreateRoleIcon(roleUIContainer, "role-bleed", currentBleed.ToString()); }
                //화상
                int currentBurn = cardData.GetCurrentBurnStacks();
                if (currentBurn > 0) { CreateRoleIcon(roleUIContainer, "role-burn", currentBurn.ToString()); }
                // 중독
                int currentPoison = cardData.GetCurrentPoisonStacks();
                if (currentPoison > 0) { CreateRoleIcon(roleUIContainer, "role-poison", currentPoison.ToString()); }
                // 빙결
                float currentFreeze = cardData.GetCurrentFreezeDuration();
                if (currentFreeze > 0) { CreateRoleIcon(roleUIContainer, "role-freeze", currentFreeze.ToString()); }

                // 우선순위 3: 쉴드
                float currentShield = cardData.GetCurrentShield();
                if (currentShield > 0) { CreateRoleIcon(roleUIContainer, "role-tanker", currentShield.ToString()); }

                // 우선순위 4: 힐
                float currentHeal = cardData.GetCurrentHeal();
                if (currentHeal > 0) { CreateRoleIcon(roleUIContainer, "role-healer", currentHeal.ToString()); }

                // 우선순위 5: 지속 힐
                int currentHealStacks = cardData.GetCurrentHealStacks();
                if (currentHealStacks > 0) { CreateRoleIcon(roleUIContainer, "role-heal-dot", currentHealStacks.ToString()); }
            }
        }
        else // 슬롯이 비어있다면
        {
            // 배경 이미지를 null로 설정
            if (cardImageLayer != null)
            {
                cardImageLayer.style.backgroundImage = null;
            }

            // 쿨타임/역할 UI도 모두 숨김
            if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.None;
            if (roleUIContainer != null) roleUIContainer.Clear();
            if (costContainer != null) costContainer.style.display = DisplayStyle.None;
            if (costLabel != null) costLabel.text = "";
        }
    }

    // 역할 아이콘 동적 생성
    private void CreateRoleIcon(VisualElement container, string roleClass, string valueText)
    {
        // 1) 아이콘 생성
        VisualElement icon = new VisualElement();
        icon.AddToClassList("card-role-icon"); // 공통 스타일 
        icon.AddToClassList(roleClass); // 개별 색상 스타일

        // 2) 텍스트 라벨(Label) 생성
        Label label = new Label(valueText);
        label.AddToClassList("card-role-label"); 

        // 3) 네모 안에 텍스트를 넣고, 컨테이너에 네모를 추가
        icon.Add(label); // 텍스트 추가
        container.Add(icon); // 아이콘 추가
    }

    public virtual void CleanupBattleUI()
    {
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) // 슬롯에 카드가 있다면
            {
                m_Cards[i].ClearBattleStatBuffs(); // 스탯 초기화
                m_Cards[i].ClearBattleFrozen(); // 빙결 초기화
                m_Cards[i].CurrentCooldown = 0f; // 쿨타임 초기화
                m_Cards[i].SetSlotIndex(i);
                UpdateCardSlotUI(i); // UI 갱신
            }
        }
    }

    // --- 11. 카드 파괴 함수 ---
    // 이 함수는 전투 중 파괴만 담당 : 다음 획득 전까지 영구적으로 사라지는 로직은 이곳이 아닌 메인 덱 리스트에서 이 카드를 제거함으로써 구현
    public virtual void DestroyCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 7) return;

        Card cardToDestroy = m_Cards[slotIndex];
        if (cardToDestroy != null)
        {
            Debug.Log($"[{cardToDestroy.CardNameKey}] (이)가 파괴되었습니다!");
            cardToDestroy.OnDestroyed();
            // 1. C# 배열에서 카드를 제거 (null로 만듦)
            m_Cards[slotIndex] = null;

            // 2. UI를 빈 슬롯 상태로 즉시 업데이트 (이미지/UI 모두 지움)
            UpdateCardSlotUI(slotIndex);
        }
    }

    // --- 12. 카드 소환 함수 ---
    public virtual bool SpawnCardToRandomEmptySlot(string cardID)
    {
        // 비어있는 슬롯 찾기
        int emptySlotIndex = -1;
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex == -1)
        {
            Debug.Log($"[{cardID}] 소환 실패: 빈 슬롯이 없습니다.");
            return false;
        }

        // CardFactory 카드 생성 요청
        Card newCard = CardFactory.CreateCard(cardID, this, emptySlotIndex);

        if (newCard == null)
        {
            Debug.LogError($"[SpawnCard] CardFactory가 {cardID} 카드 생성을 실패했습니다.");
            return false;
        }

        //카드 배치
        m_Cards[emptySlotIndex] = newCard;

        //UI 업데이트
        UpdateCardSlotUI(emptySlotIndex);

        Debug.Log($"[{newCard.CardNameKey}] (이)가 {emptySlotIndex}번 슬롯에 성공적으로 소환되었습니다!");
        return true;
    }

    // --- 13. 드래그 앤 드랍 ---
    public BattleManager GetBattleManager()
    {
        return m_BattleManager;
    }
    //툴팁 표시 예약 취소
    public void ClearTooltipScheduler()
    {
        m_TooltipScheduler?.Pause();
    }

    // UI 요소 인덱스 반환
    public int GetSlotIndexFromTarget(VisualElement target)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] == target)
            {
                return i; // 인덱스 (0~6) 반환
            }
        }
        return -1;
    }

    // 배열에서 카드를 제거하고, 이후의 요소들을 왼쪽으로 당겨 빈 공간을 메우는 함수
    public void RemoveCard(int index)
    {
        if (index < 0 || index >= m_Cards.Length) return;

        // 왼쪽으로 시프트
        for (int i = index; i < m_Cards.Length - 1; i++)
        {
            m_Cards[i] = m_Cards[i + 1];
            // New SlotIndex
            if (m_Cards[i] != null) m_Cards[i].SetSlotIndex(i);
        }

        //마지막 칸을 null로 변환
        m_Cards[m_Cards.Length - 1] = null;

        //  UI 갱신
        for (int i = index; i < m_Cards.Length; i++)
        {
            UpdateCardSlotUI(i);
        }
    }

    //지정된 인덱스에 카드를 삽입하고, 기존 요소들을 오른쪽으로 미는 함수
    public void InsertCard(int index, Card cardToInsert)
    {
        if (index < 0 || index >= m_Cards.Length) return;
        if (cardToInsert == null) return;

        // 오른쪽으로 시프트
        for (int i = m_Cards.Length - 1; i > index; i--)
        {
            m_Cards[i] = m_Cards[i - 1];
            // New SlotIndex
            if (m_Cards[i] != null) m_Cards[i].SetSlotIndex(i);
        }

        // 지정된 인덱스에 카드 삽입
        m_Cards[index] = cardToInsert;
        cardToInsert.SetSlotIndex(index);

        // UI 갱신
        for (int i = index; i < m_Cards.Length; i++)
        {
            UpdateCardSlotUI(i);
        }
    }

    public void MoveCard(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex) return;
        if (oldIndex < 0 || oldIndex >= 7 || newIndex < 0 || newIndex >= 7) return;

        Card targetCard = m_Cards[oldIndex];
        if (targetCard == null) return;

        // 목적지가 비어있는지 확인
        if (m_Cards[newIndex] == null)
        {
            // 빈 자리로 이동 
            m_Cards[newIndex] = targetCard;
            m_Cards[oldIndex] = null;

            Debug.Log($"[이동] {oldIndex} -> {newIndex} (단순 이동)");
        }
        else
        {
            // 이미 카드가 있는 자리로 이동 
            List<Card> cardList = new List<Card>(m_Cards);

            cardList.RemoveAt(oldIndex); // 원래 자리에서 빼고 (뒤쪽이 당겨짐)
            cardList.Insert(newIndex, targetCard); // 새 자리에 끼워넣음 (뒤쪽이 밀림)

            m_Cards = cardList.ToArray();

            Debug.Log($"[이동] {oldIndex} -> {newIndex} (끼어들기)");
        }

        // 3. 데이터 및 UI 갱신
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) m_Cards[i].SetSlotIndex(i);
            UpdateCardSlotUI(i);
        }
    }

    // 14. 인벤토리 관련 함수

    // 장착 (인벤토리 -> 파티 슬롯)
    public void EquipCard(int inventoryIndex, int partySlotIndex)
    {
        // 현재 보고 있는 탭의 리스트에서 카드를 가져옴
        CardType currentTab = UIManager.Instance.CurrentTab;
        Card cardToEquip = InventoryManager.Instance.GetCardAtIndex(currentTab, inventoryIndex);

        if (cardToEquip == null) return;

        // 만약 해당 파티 슬롯에 이미 카드가 있다면? -> 교체(Swap)
        if (m_Cards[partySlotIndex] != null)
        {
            Card oldCard = m_Cards[partySlotIndex];
            Debug.Log($"[Equip] 교체됨: {oldCard.CardNameKey} -> 인벤토리로 이동");

            // 기존 카드를 인벤토리로 반환
            InventoryManager.Instance.AddCardObject(oldCard);
        }

        // 새 카드 장착
        m_Cards[partySlotIndex] = cardToEquip;

        // 카드 정보 업데이트 (주인: 나, 위치: 여기)
        cardToEquip.Owner = this; // (setter가 없다면 필드 직접 할당 or 메서드 사용)
        cardToEquip.SetSlotIndex(partySlotIndex);

        // 인벤토리 리스트에서는 삭제
        InventoryManager.Instance.RemoveCard(cardToEquip);

        // UI 갱신 (파티창 & 인벤토리 둘 다)
        UpdateAllUI();
    }

    // 해제 (파티 슬롯 -> 인벤토리)
    public void UnequipCard(int partySlotIndex)
    {
        if (InventoryManager.Instance == null) return;
        if (partySlotIndex < 0 || partySlotIndex >= m_Cards.Length) return;

        Card cardToUnequip = m_Cards[partySlotIndex];
        if (cardToUnequip == null) return;

        // 인벤토리로 보내기
        InventoryManager.Instance.AddCardObject(cardToUnequip);

        // 파티 슬롯 비우기
        m_Cards[partySlotIndex] = null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SwitchTab(cardToUnequip.ItemType);

            // 만약 인벤토리가 닫혀있다면, 자동으로 열어주는 센스
            if (!UIManager.Instance.IsInventoryOpen)
            {
                UIManager.Instance.OpenInventory();
            }
        }
        // UI 갱신
        UpdateAllUI();
    }

    // 판매 (어디서든 -> 판매 존)
    public void SellCard(int slotIndex, bool fromInventory)
    {
        Card cardToSell = null;

        // 인벤토리에서 판매
        if (fromInventory)
        {
            CardType currentTab = UIManager.Instance.CurrentTab;
            cardToSell = InventoryManager.Instance.GetCardAtIndex(currentTab, slotIndex);

            if (cardToSell != null)
            {
                InventoryManager.Instance.RemoveCard(cardToSell);
            }
        }
        // 파티 창에서 바로 판매
        else
        {
            cardToSell = m_Cards[slotIndex];
            if (cardToSell != null)
            {
                m_Cards[slotIndex] = null; // 슬롯 비우기
            }
        }

        if (cardToSell == null) return;

        // 골드 획득 로직
        int price = cardToSell.GetSellPrice(); // Card 클래스에 있는 함수
        Gold += price;
        Debug.Log($"[Sell] {cardToSell.CardNameKey} 판매 완료! (+{price} G)");

        // UI 갱신
        UpdateAllUI();
    }

    // 모든 UI 갱신
    private void UpdateAllUI()
    {
        // 파티 UI 갱신
        // (PlayerController의 InitializeUI에서 등록한 refresh 메서드나 UIManager 호출)
        // 예: UpdatePartyUI(); 
        // 만약 UIManager를 통해 갱신한다면:
        UIManager.Instance.RefreshPlayerUI();

        // 인벤토리 UI 갱신
        if (UIManager.Instance.IsInventoryOpen)
        {
            UIManager.Instance.RefreshInventoryGrid(UIManager.Instance.CurrentTab);
        }

        // 골드 UI 갱신 
        UIManager.Instance.UpdateGoldUI(Gold);
    }

    public void SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;

            // UI 갱신
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGoldUI(Gold);
            }
        }
    }
    public void ModifyGold(int amount)
    {
        // 돈이 음수가 되지 않게 방지
        if (Gold + amount < 0)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return;
        }

        Gold += amount;
        Debug.Log($"[Player] 골드 변동: {amount}G (현재: {Gold}G)");

        // UI 즉시 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoldUI(Gold);
        }
    }

    // 빈 파티 슬롯 찾기 (없으면 -1 반환)
    public int GetFirstEmptyPartySlot()
    {
        for (int i = 0; i < m_Cards.Length; i++)
        {
            if (m_Cards[i] == null) return i;
        }
        return -1;
    }

    // 카드 즉시 장착 (상점에서 바로 파티로)
    public void EquipCardDirectly(Card card, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= m_Cards.Length) return;

        m_Cards[slotIndex] = card;
        card.Owner = this;
        card.SetSlotIndex(slotIndex);

        // 파티 UI 갱신
        UpdateCardSlotUI(slotIndex);
    }

    // 특정 슬롯의 카드를 제거하고 반환 (데이터는 유지, 슬롯만 비움)
    public Card ExtractCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 7) return null;

        Card card = m_Cards[slotIndex];
        m_Cards[slotIndex] = null; // 슬롯 비우기

        UpdateCardSlotUI(slotIndex); // 빈 슬롯으로 갱신
        return card;
    }

    // 카드를 빈자리에 자동으로 넣기 (파티 -> 인벤토리 순)
    public void ReturnCardToBestSlot(Card card)
    {
        if (card == null) return;

        // 1. 파티 창 빈자리 확인
        int emptyPartySlot = GetFirstEmptyPartySlot();
        if (emptyPartySlot != -1)
        {
            EquipCardDirectly(card, emptyPartySlot);
            Debug.Log($"[System] {card.CardNameKey} -> 파티 슬롯 {emptyPartySlot}으로 복귀");
            return;
        }

        // 2. 인벤토리로 보내기
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddCardObject(card);
            Debug.Log($"[System] {card.CardNameKey} -> 인벤토리로 복귀");
        }
        else
        {
            Debug.LogError("[System] 인벤토리 매니저가 없어서 카드가 증발했습니다!");
        }
    }

    // -------------------------- 프로토타입용 덱 설정 함수 ---------------------------------
    // --------------------------------------------------------------------------------------

    /// <param name="cardNames">덱에 넣을 카드 이름 목록 (예: "이름")</param>

    public virtual void SetupDeck(string[] cardNames)
    {
        // 기본 PlayerController는 덱이 비어있습니다.
        // 자식 클래스에서 이 부분을 채워야 합니다.
        Debug.Log("기본 PlayerController는 덱을 설정할 수 없습니다.");
    }
}