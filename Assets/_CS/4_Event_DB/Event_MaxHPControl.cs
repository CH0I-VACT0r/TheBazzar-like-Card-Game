using UnityEngine;

[CreateAssetMenu(fileName = "Event_MaxHPControl", menuName = "Events/Instant/MaxHPControl")]
public class Event_MaxHPControl : GameEvent
{
    public enum HPChangeMode { Flat, Percent, RandomRange }

    [Header("설정 모드")]
    public HPChangeMode changeMode;

    [Header("수치 설정")]
    [Tooltip("Flat: 고정 수치 증가 / Percent: 퍼센트 증가율(%) / RandomRange: 최소/최대 범위 지정")]
    public float value;

    [Header("RandomRange 전용 설정")]
    public float minValue = -100f;
    public float maxValue = 100f;

    public override void Execute(PlayerController player)
    {
        float finalAmount = 0;

        switch (changeMode)
        {
            case HPChangeMode.Flat:
                finalAmount = value;
                break;

            case HPChangeMode.Percent:
                finalAmount = Mathf.FloorToInt(player.MaxHP * (value / 100f));
                break;

            case HPChangeMode.RandomRange:
                int minStep = Mathf.CeilToInt(minValue / 5f); 
                int maxStep = Mathf.FloorToInt(maxValue / 5f);

                int randomStep = Random.Range(minStep, maxStep + 1);

                finalAmount = randomStep * 5f;

                Debug.Log($"[Event] 랜덤 단계: {randomStep}, 최종 수치: {finalAmount}");
                break;
        }

        // 실제 적용
        if (finalAmount > 0)
        {
            player.IncreaseMaxHP(finalAmount);
        }
        else if (finalAmount < 0)
        {
            // 감소 시에는 양수값으로 전달해야 함
            player.DecreaseMaxHP(Mathf.Abs(finalAmount));
        }

        // 결과 확인용 로그
        Debug.Log($"[Event_MaxHP] 결과: {finalAmount} (Mode: {changeMode})");

        GameManager.Instance.StartNextDay();
    }
}