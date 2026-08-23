namespace LostNight
{
    public readonly struct CaseResolution
    {
        public bool IsCorrect { get; }
        public int ScoreGained { get; }
        public int EfficiencyBonus { get; }
        public int TimeBonus { get; }
        public string Reason { get; }

        public CaseResolution(bool isCorrect, int scoreGained, int efficiencyBonus, int timeBonus, string reason)
        {
            IsCorrect = isCorrect; ScoreGained = scoreGained; EfficiencyBonus = efficiencyBonus;
            TimeBonus = timeBonus; Reason = reason;
        }
    }

    public sealed class GameSession
    {
        public const int ClearTarget = 5;
        public const int MaxMistakes = 3;
        public int CorrectCount { get; private set; }
        public int MistakeCount { get; private set; }
        public int Score { get; private set; }
        public int CaseNumber { get; private set; }
        public bool IsClear => CorrectCount >= ClearTarget;
        public bool IsGameOver => MistakeCount >= MaxMistakes;

        public void Reset() { CorrectCount = 0; MistakeCount = 0; Score = 0; CaseNumber = 0; }

        public CaseResolution Resolve(LostItemCaseDefinition data, bool returned, int claimantIndex,
            int evidenceCount, float remainingSeconds)
        {
            var correct = returned ? claimantIndex == data.OwnerIndex : data.OwnerIndex < 0;
            var efficiency = correct && evidenceCount == 2 ? 100 : 0;
            var time = correct ? UnityEngine.Mathf.CeilToInt(remainingSeconds) : 0;
            var gained = correct ? 200 + efficiency + time : 0;
            if (correct) CorrectCount++; else MistakeCount++;
            Score += gained; CaseNumber++;
            return new CaseResolution(correct, gained, efficiency, time,
                correct ? data.SuccessReason : data.FailureReason);
        }

        public CaseResolution RegisterTimeout()
        {
            MistakeCount++; CaseNumber++;
            return new CaseResolution(false, 0, 0, 0, "判断時間を超過し、案件を処理できなかった。");
        }
    }
}
