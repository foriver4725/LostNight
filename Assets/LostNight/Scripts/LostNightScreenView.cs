using UnityEngine;
using UnityEngine.UI;

namespace LostNight
{
    public sealed class LostNightScreenView : MonoBehaviour
    {
        public Button StartButton { get; private set; }
        public Button TutorialStartButton { get; private set; }
        public Button AudioPromptButton { get; private set; }
        public Slider VolumeSlider { get; private set; }
        public Button ClaimantAButton { get; private set; }
        public Button ClaimantBButton { get; private set; }
        public Button ReturnButton { get; private set; }
        public Button StoreButton { get; private set; }
        public Button ContinueButton { get; private set; }
        public Button RetryButton { get; private set; }
        public Button TitleButton { get; private set; }

        private GameObject titleScreen;
        private GameObject tutorialScreen;
        private GameObject audioPromptScreen;
        private GameObject volumePanel;
        private GameObject gameplayScreen;
        private GameObject resultScreen;
        private GameObject endingScreen;
        private Text clockText;
        private Text caseText;
        private Text memoText;
        private Text messageText;
        private Text itemText;
        private Text claimantText;
        private Text progressText;
        private Text resultTitleText;
        private Text resultBodyText;
        private Text endingTitleText;
        private Text endingBodyText;

        public void Initialize(GameObject audioPrompt, GameObject volume, GameObject title, GameObject tutorial, GameObject gameplay, GameObject result, GameObject ending,
            Text clock, Text caseLabel, Text memo, Text message, Text item, Text claimant, Text progress,
            Text resultTitle, Text resultBody, Text endingTitle, Text endingBody,
            Button promptButton, Slider volumeSlider, Button start, Button tutorialStart, Button claimantA, Button claimantB, Button returnAction, Button store,
            Button continueAction, Button retry, Button titleAction)
        {
            audioPromptScreen = audioPrompt; volumePanel = volume; titleScreen = title; tutorialScreen = tutorial;
            gameplayScreen = gameplay; resultScreen = result; endingScreen = ending;
            clockText = clock; caseText = caseLabel; memoText = memo; messageText = message;
            itemText = item; claimantText = claimant; progressText = progress;
            resultTitleText = resultTitle; resultBodyText = resultBody; endingTitleText = endingTitle; endingBodyText = endingBody;
            AudioPromptButton = promptButton; VolumeSlider = volumeSlider; StartButton = start; TutorialStartButton = tutorialStart;
            ClaimantAButton = claimantA; ClaimantBButton = claimantB;
            ReturnButton = returnAction; StoreButton = store; ContinueButton = continueAction;
            RetryButton = retry; TitleButton = titleAction;
        }

        public void Show(GameFlowState state)
        {
            audioPromptScreen.SetActive(state == GameFlowState.AudioPrompt);
            volumePanel.SetActive(state != GameFlowState.AudioPrompt);
            titleScreen.SetActive(state == GameFlowState.Title);
            tutorialScreen.SetActive(state == GameFlowState.Tutorial);
            gameplayScreen.SetActive(state == GameFlowState.Playing || state == GameFlowState.CaseResult);
            resultScreen.SetActive(state == GameFlowState.CaseResult);
            endingScreen.SetActive(state == GameFlowState.GameOver || state == GameFlowState.Clear);
        }

        public void ShowCase(LostItemCaseDefinition data, int caseNumber, GameSession session, bool[] recorded)
        {
            itemText.text = $"本日の忘れ物\n<color=#8A4F12>{data.ItemName}</color>\n\n<color=#075968>光る箇所を2つ以上</color>調べて判断する。";
            claimantText.text = $"<color=#7ED6E6>{data.ClaimantNames[0]}</color>\n{data.Claims[0]}\n\n<color=#E9B85F>{data.ClaimantNames[1]}</color>\n{data.Claims[1]}";
            SetLabel(ClaimantAButton, data.ClaimantNames[0]); SetLabel(ClaimantBButton, data.ClaimantNames[1]);
            messageText.text = "忘れ物を回して光る箇所をクリック → 申告者を選択 → 返却 / 保管";
            UpdateMemo(data, caseNumber, recorded);
            UpdateProgress(session);
            TintClaimants(-1);
        }

        public void UpdateMemo(LostItemCaseDefinition data, int caseNumber, bool[] recorded)
        {
            memoText.text = "調査メモ\n\n";
            var count = 0;
            for (var i = 0; i < data.Clues.Length; i++)
            {
                if (recorded[i]) { memoText.text += $"<color=#17606C>■ {data.Clues[i]}</color>\n"; count++; }
                else memoText.text += "□ 未記録\n";
            }
            caseText.text = $"案件 <color=#E9B85F>{caseNumber:00}</color>　記録 <color=#7ED6E6>{count}/3</color>";
        }

        public void SetMessage(string value) => messageText.text = value;
        public void SetClock(float seconds)
        {
            clockText.text = $"残り 0:{Mathf.CeilToInt(seconds):00}";
            clockText.color = seconds <= 10f && Mathf.Sin(Time.time * 7f) > 0f ? Color.white : new Color(1f, .42f, .16f);
        }

        public void UpdateProgress(GameSession session) =>
            progressText.text = $"得点 <color=#E9B85F>{session.Score}</color>　正解 <color=#72D89A>{session.CorrectCount}/{GameSession.ClearTarget}</color>　ミス <color=#E57668>{session.MistakeCount}/{GameSession.MaxMistakes}</color>";

        public void SetDecisionEnabled(bool enoughEvidence, int claimantIndex)
        {
            ReturnButton.interactable = enoughEvidence && claimantIndex >= 0;
            StoreButton.interactable = enoughEvidence;
        }

        public void TintClaimants(int selected)
        {
            ClaimantAButton.image.color = selected == 0 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
            ClaimantBButton.image.color = selected == 1 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
        }

        public void ShowResolution(CaseResolution resolution, LostItemCaseDefinition data)
        {
            resultTitleText.text = resolution.IsCorrect ? "正しい判断" : "誤った判断";
            resultTitleText.color = resolution.IsCorrect ? new Color(.45f, .86f, .61f) : new Color(.9f, .38f, .32f);
            var bonus = resolution.EfficiencyBonus > 0
                ? $"迅速判定 +{resolution.EfficiencyBonus}　時間 +{resolution.TimeBonus}"
                : resolution.IsCorrect ? $"時間ボーナス +{resolution.TimeBonus}" : "得点なし";
            resultBodyText.text = $"{resolution.Reason}\n\n<color=#E9B85F>{bonus}</color>\n\n<color=#7ED6E6>{data.Memory}</color>";
        }

        public void ShowEnding(bool clear, GameSession session)
        {
            endingTitleText.text = clear ? "業務完了" : "業務停止";
            endingTitleText.color = clear ? new Color(.92f, .75f, .46f) : new Color(.9f, .35f, .3f);
            endingBodyText.text = clear
                ? $"5件の記憶を正しく持ち主へ繋いだ。\n\n最終得点　{session.Score}\n誤判断　{session.MistakeCount}\n\n結末コード：星傘-013"
                : $"誤判断が3件に達し、窓口は閉鎖された。\n\n最終得点　{session.Score}\n正解　{session.CorrectCount}\n\n証言と特徴をもう一度照合しよう。";
        }

        private static void SetLabel(Button button, string value)
        {
            if (button.GetComponentInChildren<Text>() is { } label) label.text = value;
        }
    }
}
