using UnityEngine;

public class Card_Manual_Beginner : Card
{
    // 생성자: (주인, 슬롯인덱스) -> 쿨타임 10초 설정
    public Card_Manual_Beginner(object owner, int index) : base(owner, index, 10.0f)
    {
        // 1. 기본 정보
        this.CardNameKey = "card_manual_beginner_name";
        this.CardSkillDescriptionKey = "card_manual_beginner_desc";
        this.FlavorTextKey = "card_manual_beginner_flavor";

        // 2. 이미지 로드 (파일명: manual_beginner)
        this.CardImage = Resources.Load<Sprite>("CardImages/Neutral/Book/Manual_Beginner");

        // 3. 타입 및 밸런스
        this.ItemType = CardType.Consumable; // 소모품
        this.Rarity = CardRarity.Bronze;     // 브론즈
        this.SetInitPrice(2);                // 가격 2G
        this.OwnerLord = LordType.Common;    // 중립


        // 4. 내구도 설정 (3회 사용 가능)
        this.Durability = 3;
    }

    // 스킬 로직
    public override void ExecuteSkill()
    {
        // 주인이 플레이어인지 확인
        if (Owner is PlayerController player)
        {
            // 나의 '왼쪽'에 있는 카드 가져오기
            Card leftNeighbor = player.GetLeftNeighbor(this.SlotIndex);

            // 카드가 존재하고 && '딜러(tag_dealer)' 태그가 있는지 확인
            if (leftNeighbor != null && leftNeighbor.HasTagKey("tag_dealer"))
            {
                // 공격력 영구 증가
                // (BaseDamage를 직접 수정하면 해당 카드 객체가 파괴되기 전까지 유지됨)
                leftNeighbor.IncreaseBaseDamage(10);

                Debug.Log($"[무술교본] {leftNeighbor.CardNameKey}의 공격력이 10 증가했습니다! (현재: {leftNeighbor.BaseDamage})");

                // (선택사항) 버프 이펙트나 UI 갱신이 필요하면 여기서 호출
                // player.UpdateCardSlotUI(leftNeighbor.SlotIndex);
            }
            else
            {
                Debug.Log("[무술교본] 왼쪽에 '딜러' 카드가 없습니다.");
            }
        }
    }

    protected override void ConsumeDurability()
    {
        // 무한 내구도 체크
        if (this.Durability == -1) return;

        this.Durability--;

        // 내구도가 다 되었을 때
        if (this.Durability <= 0)
        {
            if (Owner is PlayerController player)
            {
                Debug.Log($"[{CardNameKey}] 낡아서 찢어졌습니다! ('찢어진 책'으로 변환)");

                // 1. '찢어진 책' 카드 생성 (ID: card_torn_book 가정)
                // (주의: CardFactory와 Localization에 'card_torn_book'이 등록되어 있어야 함)
                Card tornBook = CardFactory.CreateCard("card_torn_book", player, this.SlotIndex);

                if (tornBook != null)
                {
                    // 2. 현재 슬롯을 찢어진 책으로 즉시 교체
                    player.EquipCardDirectly(tornBook, this.SlotIndex);
                }
                else
                {
                    // 생성 실패 시(아직 구현 안 됨 등) 기본 파괴 로직 수행
                    player.DestroyCard(this.SlotIndex);
                }
            }
            else if (Owner is MonsterController monster)
            {
                // 몬스터가 사용 시 파괴
                monster.DestroyCard(this.SlotIndex);
            }
        }
    }
}
