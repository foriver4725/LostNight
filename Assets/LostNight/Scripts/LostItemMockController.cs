using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LostNight
{
    public sealed class LostItemMockController : MonoBehaviour
    {
        [Serializable]
        private sealed class CaseData
        {
            public string itemName;
            public string[] clues;
            public string[] claimantNames;
            public string[] claims;
            public int ownerIndex;
            public string successReason;
            public string failureReason;
            public string memory;
        }

        [SerializeField] private Transform itemRoot;
        [SerializeField] private Text clockText;
        [SerializeField] private Text caseText;
        [SerializeField] private Text memoText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text itemText;
        [SerializeField] private Text claimantText;
        [SerializeField] private Text progressText;
        [SerializeField] private Button claimantAButton;
        [SerializeField] private Button claimantBButton;
        [SerializeField] private Button[] clueButtons;
        [SerializeField] private Button recordButton;
        [SerializeField] private Button observeButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Button nextButton;

        private readonly ReactiveProperty<int> foundClues = new(0);
        private readonly ReactiveProperty<int> selectedClaimant = new(-1);
        private readonly CompositeDisposable disposables = new();
        private Vector3 lastPointerPosition;
        private bool dragging;
        private bool resolved;
        private int caseIndex;
        private int correctCount;
        private int mistakeCount;
        private readonly bool[] discoveredClues = new bool[3];
        private readonly bool[] recordedClues = new bool[3];
        private int selectedClue = -1;

        private readonly CaseData[] cases =
        {
            new()
            {
                itemName = "内側に星空が降る透明傘",
                clues = new[] { "濡れていない", "柄に小さな歯形", "内側に夜空が見える" },
                claimantNames = new[] { "A　会社員", "B　子どもの影" },
                claims = new[] { "『透明です。普通の傘でした』", "『持ち手に、かんだあとがある』" },
                ownerIndex = 1,
                successReason = "歯形を知っていたのは子どもの影だけだった。",
                failureReason = "会社員は色しか一致せず、歯形を説明できない。",
                memory = "駅の時計が、1分だけ戻った。"
            },
            new()
            {
                itemName = "片方だけ温かい右手袋",
                clues = new[] { "右手用", "乾いた砂が付着", "微かな拍手音がする" },
                claimantNames = new[] { "A　旅の楽団員", "B　清掃員" },
                claims = new[] { "『右手をなくした。砂浜で演奏した』", "『左手用だ。砂には触れていない』" },
                ownerIndex = 0,
                successReason = "右手・砂・拍手音のすべてが楽団員の証言と一致した。",
                failureReason = "清掃員の証言は左右と砂の両方で矛盾している。",
                memory = "誰もいないホームから、短い拍手が聞こえた。"
            },
            new()
            {
                itemName = "行先が消え続ける定期券",
                clues = new[] { "日付は明日", "顔写真が瞬く", "券面に駅員の名前" },
                claimantNames = new[] { "A　学生", "B　会社員" },
                claims = new[] { "『今日まで有効。写真は私です』", "『名前は私のものです』" },
                ownerIndex = -1,
                successReason = "申告者の誰とも一致しない。保管が正しい判断だった。",
                failureReason = "券面は申告者ではなく、窓口にいる駅員の名前を示している。",
                memory = "保管棚の奥で、明日の発車ベルが鳴った。"
            }
        };

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Text itemLabel, Text claimLabel, Text progressLabel, Button claimantA, Button claimantB,
            Button[] clues, Button record, Button observe, Button returnAction, Button store, Button next)
        {
            itemRoot = item; clockText = clock; caseText = caseLabel; memoText = memo; messageText = message;
            itemText = itemLabel; claimantText = claimLabel; progressText = progressLabel;
            claimantAButton = claimantA; claimantBButton = claimantB; clueButtons = clues;
            recordButton = record; observeButton = observe;
            returnButton = returnAction; storeButton = store; nextButton = next;
        }

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Button record, Button observe, Button returnAction, Button store)
        {
            Initialize(item, clock, caseLabel, memo, message, null, null, null, null, null, null,
                record, observe, returnAction, store, null);
        }

        private void Start()
        {
            recordButton.onClick.AddListener(RecordClue);
            observeButton.onClick.AddListener(Observe);
            returnButton.onClick.AddListener(() => Resolve(true));
            storeButton.onClick.AddListener(() => Resolve(false));
            claimantAButton?.onClick.AddListener(() => SelectClaimant(0));
            claimantBButton?.onClick.AddListener(() => SelectClaimant(1));
            if (clueButtons != null)
                for (var i = 0; i < clueButtons.Length; i++)
                {
                    var clueIndex = i;
                    clueButtons[i]?.onClick.AddListener(() => SelectClue(clueIndex));
                }
            nextButton?.onClick.AddListener(NextCase);

            foundClues.Subscribe(UpdateMemo).AddTo(disposables);
            selectedClaimant.Subscribe(_ => UpdateDecisionState()).AddTo(disposables);
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => PulseClock()).AddTo(disposables);
            LoadCase();
        }

        private void Update()
        {
            if (resolved) return;
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                dragging = true;
                lastPointerPosition = mouse.position.ReadValue();
            }
            if (mouse.leftButton.wasReleasedThisFrame) dragging = false;
            if (dragging && itemRoot != null)
            {
                var pointerPosition = mouse.position.ReadValue();
                var delta = (Vector3)pointerPosition - lastPointerPosition;
                itemRoot.Rotate(Vector3.up, -delta.x * .35f, Space.World);
                itemRoot.Rotate(Vector3.right, delta.y * .2f, Space.World);
                lastPointerPosition = pointerPosition;
            }
            if (itemRoot != null)
            {
                var zoom = mouse.scroll.ReadValue().y / 120f;
                itemRoot.localScale = Vector3.one * Mathf.Clamp(itemRoot.localScale.x + zoom * .08f, .75f, 1.35f);
            }
        }

        private void LoadCase()
        {
            resolved = false;
            foundClues.Value = 0;
            selectedClaimant.Value = -1;
            selectedClue = -1;
            Array.Clear(discoveredClues, 0, discoveredClues.Length);
            Array.Clear(recordedClues, 0, recordedClues.Length);
            var data = cases[caseIndex];
            if (itemText != null) itemText.text = $"本日の忘れ物\n{data.itemName}\n\n特徴を2つ以上記録して判断する。";
            if (claimantText != null)
                claimantText.text = $"{data.claimantNames[0]}\n{data.claims[0]}\n\n{data.claimantNames[1]}\n{data.claims[1]}";
            SetButtonLabel(claimantAButton, data.claimantNames[0]);
            SetButtonLabel(claimantBButton, data.claimantNames[1]);
            messageText.text = "①観察 → ②特徴を記録 → ③申告者を選択 → ④返却 / 保管";
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            SetActionsInteractable(true);
            UpdateClueButtons();
            UpdateMemo(0);
            UpdateDecisionState();
        }

        private void RecordClue()
        {
            if (selectedClue < 0 || !discoveredClues[selectedClue] || recordedClues[selectedClue])
            {
                messageText.text = "観察で見つけた特徴を選択してから記録してください。";
                return;
            }
            recordedClues[selectedClue] = true;
            foundClues.Value++;
            selectedClue = -1;
            UpdateClueButtons();
            messageText.text = foundClues.Value >= 2
                ? "判断可能です。申告者を選んで返却するか、安全に保管してください。"
                : "もう1つ特徴を記録すると判断できます。";
        }

        private void Observe()
        {
            itemRoot.Rotate(0f, 35f, 0f, Space.World);
            var discoveredIndex = Array.FindIndex(discoveredClues, discovered => !discovered);
            if (discoveredIndex < 0)
            {
                messageText.text = "すべての特徴を観察しました。記録する特徴を選んでください。";
                return;
            }
            discoveredClues[discoveredIndex] = true;
            selectedClue = discoveredIndex;
            UpdateClueButtons();
            messageText.text = $"特徴『{cases[caseIndex].clues[discoveredIndex]}』を発見。選択中の特徴を記録できます。";
        }

        private void SelectClue(int index)
        {
            if (resolved || !discoveredClues[index] || recordedClues[index]) return;
            selectedClue = index;
            UpdateClueButtons();
            messageText.text = $"『{cases[caseIndex].clues[index]}』を選択中。「記録」で調査メモに残します。";
        }

        private void SelectClaimant(int index)
        {
            if (resolved) return;
            selectedClaimant.Value = index;
            messageText.text = $"申告者 {cases[caseIndex].claimantNames[index]} を選択中。返却で確定します。";
            TintClaimantButtons();
        }

        private void Resolve(bool returned)
        {
            if (resolved || foundClues.Value < 2) return;
            if (returned && selectedClaimant.Value < 0)
            {
                messageText.text = "返却先の申告者を選択してください。";
                return;
            }

            var data = cases[caseIndex];
            var correct = returned ? selectedClaimant.Value == data.ownerIndex : data.ownerIndex < 0;
            resolved = true;
            if (correct) correctCount++; else mistakeCount++;
            messageText.text = $"{(correct ? "正しい判断" : "誤った判断")} — {(correct ? data.successReason : data.failureReason)}\n{data.memory}";
            clockText.text = $"0:{Mathf.Max(10, 13 - caseIndex - 1):00}";
            SetActionsInteractable(false);
            UpdateProgress();
            if (nextButton != null)
            {
                SetButtonLabel(nextButton, caseIndex == cases.Length - 1 ? "結果を見る" : "次の案件");
                nextButton.gameObject.SetActive(true);
            }
        }

        private void NextCase()
        {
            if (caseIndex < cases.Length - 1)
            {
                caseIndex++;
                LoadCase();
                return;
            }

            resolved = true;
            caseText.text = "一夜の業務終了";
            memoText.text = $"業務報告\n\n正しい判断　{correctCount} / {cases.Length}\n誤った判断　{mistakeCount}\n\n{(correctCount == cases.Length ? "結末コード：星傘-013" : "もう一度、証言をよく照合しよう。")}";
            messageText.text = correctCount == cases.Length
                ? "全案件を正しく処理しました。忘れ物は、記憶を少しだけ残していった。"
                : "一夜が終了しました。再挑戦して全案件の正解を目指せます。";
            SetButtonLabel(nextButton, "もう一度");
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(RestartRun);
        }

        private void RestartRun()
        {
            caseIndex = 0; correctCount = 0; mistakeCount = 0; clockText.text = "0:13";
            nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(NextCase);
            LoadCase();
        }

        private void UpdateMemo(int count)
        {
            var data = cases[caseIndex];
            memoText.text = "調査メモ\n\n";
            for (var i = 0; i < data.clues.Length; i++)
                memoText.text += recordedClues[i] ? $"■ {data.clues[i]}\n" : "□ 未記録\n";
            caseText.text = $"案件 {caseIndex + 1:00} / {cases.Length:00}　記録 {count}/3";
            UpdateProgress();
            UpdateDecisionState();
        }

        private void UpdateDecisionState()
        {
            if (returnButton == null || storeButton == null) return;
            var enoughEvidence = foundClues.Value >= 2 && !resolved;
            recordButton.interactable = selectedClue >= 0 && !recordedClues[selectedClue] && !resolved;
            returnButton.interactable = enoughEvidence && selectedClaimant.Value >= 0;
            storeButton.interactable = enoughEvidence;
            TintClaimantButtons();
        }

        private void UpdateProgress()
        {
            if (progressText != null) progressText.text = $"正解 {correctCount}　誤判断 {mistakeCount}　残り {cases.Length - caseIndex}";
        }

        private void SetActionsInteractable(bool value)
        {
            recordButton.interactable = value; observeButton.interactable = value;
            if (claimantAButton != null) claimantAButton.interactable = value;
            if (claimantBButton != null) claimantBButton.interactable = value;
            if (clueButtons != null)
                foreach (var clueButton in clueButtons)
                    if (clueButton != null) clueButton.interactable = value;
            if (!value) { returnButton.interactable = false; storeButton.interactable = false; }
        }

        private void UpdateClueButtons()
        {
            if (clueButtons == null) return;
            for (var i = 0; i < clueButtons.Length; i++)
            {
                var button = clueButtons[i];
                if (button == null) continue;
                SetButtonLabel(button, discoveredClues[i] ? cases[caseIndex].clues[i] : $"未発見 {i + 1}");
                button.interactable = discoveredClues[i] && !recordedClues[i] && !resolved;
                button.image.color = recordedClues[i] ? new Color(.18f, .38f, .25f)
                    : selectedClue == i ? new Color(.55f, .4f, .16f) : new Color(.12f, .2f, .24f);
            }
            observeButton.interactable = !resolved && Array.Exists(discoveredClues, discovered => !discovered);
            recordButton.interactable = selectedClue >= 0 && !recordedClues[selectedClue] && !resolved;
        }

        private void TintClaimantButtons()
        {
            if (claimantAButton == null || claimantBButton == null) return;
            claimantAButton.image.color = selectedClaimant.Value == 0 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
            claimantBButton.image.color = selectedClaimant.Value == 1 ? new Color(.55f, .4f, .16f) : new Color(.14f, .22f, .24f);
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button != null && button.GetComponentInChildren<Text>() is { } label) label.text = value;
        }

        private void PulseClock()
        {
            if (clockText != null) clockText.color = clockText.color == Color.white ? new Color(1f, .42f, .16f) : Color.white;
        }

        private void OnDestroy()
        {
            disposables.Dispose(); foundClues.Dispose(); selectedClaimant.Dispose();
        }
    }
}
