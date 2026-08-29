using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LostNight
{
    public sealed class LostItemMockController : MonoBehaviour
    {
        private const float CaseDuration = 45f;
        private readonly ReactiveProperty<GameFlowState> state = new(GameFlowState.AudioPrompt);
        private readonly CompositeDisposable disposables = new();
        private readonly GameSession session = new();
        private readonly CaseDeck caseDeck = new();
        private IReadOnlyList<LostItemCaseDefinition> catalog;
        private LostItemCaseDefinition currentCase;
        private LostNightScreenView view;
        private LostItemModelPresenter modelPresenter;
        private LostNightAudio audioService;
        private Transform itemRoot;
        private Transform[] clueHotspots;
        private readonly bool[] recordedClues = new bool[3];
        private int recordedCount;
        private int selectedClaimant = -1;
        private float timeRemaining;
        private Vector3 lastPointerPosition;
        private Vector3 pointerDownPosition;
        private bool dragging;

        public void Initialize(LostItemModelPresenter presenter, LostNightScreenView screenView, LostNightAudio audio)
        {
            modelPresenter = presenter; view = screenView; audioService = audio;
        }

        public void Initialize(Transform item, Text clock, Text caseLabel, Text memo, Text message,
            Button record, Button observe, Button returnAction, Button store) => itemRoot = item;

        private void Start()
        {
            if (view == null) return;
            catalog = LostItemCaseCatalog.CreateDefault();
            view.AudioPromptButton.onClick.AddListener(UnlockAudio);
            view.VolumeSlider.onValueChanged.AddListener(audioService.SetVolume);
            view.StartButton.onClick.AddListener(StartGame);
            view.ClaimantAButton.onClick.AddListener(() => SelectClaimant(0));
            view.ClaimantBButton.onClick.AddListener(() => SelectClaimant(1));
            view.ReturnButton.onClick.AddListener(() => ResolveCase(true));
            view.StoreButton.onClick.AddListener(() => ResolveCase(false));
            view.ContinueButton.onClick.AddListener(ContinueFlow);
            view.RetryButton.onClick.AddListener(StartGame);
            view.TitleButton.onClick.AddListener(ShowTitle);
            state.Subscribe(view.Show).AddTo(disposables);
            audioService.SetVolume(view.VolumeSlider.value);
            state.Value = GameFlowState.AudioPrompt;
        }

        private void UnlockAudio() { audioService.Unlock(); ShowTitle(); }

        private void Update()
        {
            if (state.Value != GameFlowState.Playing) return;
            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            view.SetClock(timeRemaining);
            if (timeRemaining <= 0f) { ResolveTimeout(); return; }

            var mouse = Mouse.current;
            if (mouse == null) return;
            var overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (mouse.leftButton.wasPressedThisFrame && !overUi)
            {
                dragging = true; pointerDownPosition = mouse.position.ReadValue(); lastPointerPosition = pointerDownPosition;
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                var pointerPosition = mouse.position.ReadValue();
                if (!overUi && Vector2.Distance(pointerDownPosition, pointerPosition) < 12f) TryRecordHotspot(pointerPosition);
            }
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
            PulseHotspots();
        }

        private void StartGame() { audioService.PlayConfirm(); session.Reset(); caseDeck.Reset(catalog); LoadCase(); }

        private void ShowTitle()
        {
            state.Value = GameFlowState.Title;
            if (modelPresenter == null) return;
            var titleModel = modelPresenter.Show(LostItemModelKind.Umbrella);
            itemRoot = titleModel.Root; clueHotspots = titleModel.Hotspots; SetHotspotsVisible(false);
        }

        private void LoadCase()
        {
            currentCase = caseDeck.Draw();
            var model = modelPresenter.Show(currentCase.ModelKind);
            itemRoot = model.Root; clueHotspots = model.Hotspots;
            Array.Clear(recordedClues, 0, recordedClues.Length);
            recordedCount = 0; selectedClaimant = -1; timeRemaining = CaseDuration;
            if (itemRoot != null) { itemRoot.rotation = Quaternion.identity; itemRoot.localScale = Vector3.one; }
            SetHotspotsVisible(true);
            view.ShowCase(currentCase, session.CaseNumber + 1, session, recordedClues);
            view.SetClock(timeRemaining); view.SetDecisionEnabled(false, selectedClaimant);
            state.Value = GameFlowState.Playing;
        }

        private void SelectClaimant(int index)
        {
            if (state.Value != GameFlowState.Playing) return;
            selectedClaimant = index; view.TintClaimants(index);
            audioService.PlaySelect();
            view.SetDecisionEnabled(recordedCount >= 2, selectedClaimant);
            view.SetMessage($"<color=#E9B85F>{currentCase.ClaimantNames[index]}</color>を返却先として選択中。<color=#7ED6E6>返却</color>で確定します。");
        }

        private void TryRecordHotspot(Vector2 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null || clueHotspots == null) return;
            var hits = Physics.RaycastAll(camera.ScreenPointToRay(screenPosition), 100f);
            for (var h = 0; h < hits.Length; h++)
            for (var i = 0; i < clueHotspots.Length; i++)
            {
                if (hits[h].transform != clueHotspots[i] || recordedClues[i]) continue;
                recordedClues[i] = true; recordedCount++; clueHotspots[i].gameObject.SetActive(false);
                audioService.PlayDiscover();
                view.UpdateMemo(currentCase, session.CaseNumber + 1, recordedClues);
                view.SetDecisionEnabled(recordedCount >= 2, selectedClaimant);
                view.SetMessage(recordedCount >= 2 ? $"<color=#7ED6E6>『{currentCase.Clues[i]}』</color>を記録。<color=#72D89A>判断可能</color>です。"
                    : $"<color=#7ED6E6>『{currentCase.Clues[i]}』</color>を記録。もう1箇所探してください。");
                return;
            }
        }

        private void ResolveCase(bool returned)
        {
            if (state.Value != GameFlowState.Playing || recordedCount < 2) return;
            if (returned && selectedClaimant < 0) { view.SetMessage("返却先の申告者を選択してください。"); return; }
            PresentResolution(session.Resolve(currentCase, returned, selectedClaimant, recordedCount, timeRemaining));
        }

        private void ResolveTimeout() => PresentResolution(session.RegisterTimeout());

        private void PresentResolution(CaseResolution resolution)
        {
            if (resolution.IsCorrect) audioService.PlaySuccess(); else audioService.PlayFailure();
            SetHotspotsVisible(false); view.UpdateProgress(session); view.ShowResolution(resolution, currentCase);
            state.Value = GameFlowState.CaseResult;
        }

        private void ContinueFlow()
        {
            audioService.PlayConfirm();
            if (session.IsClear) ShowEnding(true);
            else if (session.IsGameOver) ShowEnding(false);
            else LoadCase();
        }

        private void ShowEnding(bool clear)
        {
            view.ShowEnding(clear, session); state.Value = clear ? GameFlowState.Clear : GameFlowState.GameOver;
        }

        private void SetHotspotsVisible(bool visible)
        {
            if (clueHotspots == null) return;
            for (var i = 0; i < clueHotspots.Length; i++)
                if (clueHotspots[i] != null) clueHotspots[i].gameObject.SetActive(visible && !recordedClues[i]);
        }

        private void PulseHotspots()
        {
            if (clueHotspots == null) return;
            var pulse = .18f + Mathf.Sin(Time.time * 4f) * .035f;
            foreach (var hotspot in clueHotspots)
                if (hotspot != null && hotspot.gameObject.activeSelf) hotspot.localScale = Vector3.one * pulse;
        }

        private void OnDestroy() { disposables.Dispose(); state.Dispose(); }
    }
}
