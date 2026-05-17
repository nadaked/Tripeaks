using System;
using System.Collections;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Game;
using GameServices.GameServices.Runtime.Audio;
using GameServices.GameServices.Runtime.Core;
using UnityEngine;

namespace _Project.Scripts.Presentation.Audio
{
    public sealed class TripeaksAudioDirector : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameBootstrapper gameBootstrapper;
        [SerializeField] private GameServicesProvider servicesProvider;

        [Header("GameServices Sfx Ids")]
        [SerializeField] private string boardCardSfxId = "card_take";
        [SerializeField] private string deckDrawSfxId = "deck_draw";
        [SerializeField] private string revealSfxId = "card_reveal";
        [SerializeField] private string rewardSfxId = "reward_card";
        [SerializeField] private string wildSfxId = "wild";
        [SerializeField] private string undoSfxId = "undo";
        [SerializeField] private string invalidMoveSfxId = "invalid_move";
        [SerializeField] private bool playGameServicesSfx = true;

        [Header("Background Music")]
        [SerializeField] private bool playBackgroundMusic = true;
        [SerializeField] private MusicTrack[] backgroundMusicTracks =
        {
            new("clouds_over_tripeaks_1"),
            new("clouds_over_tripeaks_2")
        };
        [SerializeField] private bool loopBackgroundPlaylist = true;
        [SerializeField, Min(0f)] private float musicTrackGap = 0.15f;
        [SerializeField, Min(1f)] private float fallbackTrackDuration = 120f;

        [Header("Combo Melody")]
        [SerializeField] private string[] comboSfxIds =
        {
            "combo_1",
            "combo_2",
            "combo_3"
        };
        [SerializeField, Range(0f, 1f)] private float comboVolumeScale = 0.9f;
        [SerializeField] private bool usePingPongScale = true;
        [SerializeField] private float phrasePitchDropSemitones = 0.5f;
        [SerializeField] private float minPitch = 0.72f;
        [SerializeField] private float maxPitch = 1.8f;
        [SerializeField] private bool resetComboOnDeckDraw = true;
        [SerializeField] private bool resetComboOnUndo = true;

        private static readonly int[] AscendingScaleSemitones = { 0, 2, 4, 5, 7, 9, 11, 12, 14 };
        private static readonly int[] PingPongScaleSemitones = { 0, 2, 4, 5, 7, 9, 11, 12, 14, 12, 11, 9, 7, 5, 4, 2 };

        private GamePresenter _presenter;
        private IAudioService _audioService;
        private Coroutine _musicRoutine;
        private int _comboStep;

        [Serializable]
        private struct MusicTrack
        {
            public string Id;
            public AudioClip Clip;

            public MusicTrack(string id)
            {
                Id = id;
                Clip = null;
            }
        }

        private void Start()
        {
            if (gameBootstrapper == null)
                gameBootstrapper = FindFirstObjectByType<GameBootstrapper>();

            _presenter = gameBootstrapper != null ? gameBootstrapper.Presenter : null;

            if (_presenter != null)
            {
                _presenter.MovePerformed += OnMovePerformed;
                _presenter.InvalidBoardCardSelected += OnInvalidBoardCardSelected;
            }

            if (servicesProvider == null)
            {
                var servicesBootstrapper = FindFirstObjectByType<GameServicesBootstrapper>();
                if (servicesBootstrapper != null)
                    servicesProvider = servicesBootstrapper.Provider;
            }

            StartCoroutine(ResolveAudioServiceWhenReady());
        }

        private void OnDestroy()
        {
            if (_musicRoutine != null)
            {
                StopCoroutine(_musicRoutine);
                _musicRoutine = null;
            }

            if (_audioService != null && playBackgroundMusic)
                _audioService.StopMusicAsync();

            if (_presenter != null)
            {
                _presenter.MovePerformed -= OnMovePerformed;
                _presenter.InvalidBoardCardSelected -= OnInvalidBoardCardSelected;
            }
        }

        private IEnumerator ResolveAudioServiceWhenReady()
        {
            if (servicesProvider == null)
                yield break;

            while (!servicesProvider.IsInitialized && servicesProvider.IsInitializing)
                yield return null;

            servicesProvider.TryGet(out _audioService);

            if (_audioService != null && playBackgroundMusic)
                _musicRoutine = StartCoroutine(PlayBackgroundMusicPlaylist());
        }

        private void OnMovePerformed(GameMoveResult result)
        {
            if (!result.Success)
                return;

            switch (result.MoveType)
            {
                case GameMoveType.PlayFromBoard:
                    PlaySfx(boardCardSfxId);
                    PlayComboStep();

                    if (result.Record != null && result.Record.RevealedSlots.Count > 0)
                        PlaySfx(revealSfxId);

                    if (result.Record != null && result.Record.AddedToDeck.Count > 0)
                        PlaySfx(rewardSfxId);
                    break;

                case GameMoveType.DrawFromDeck:
                    PlaySfx(deckDrawSfxId);
                    if (resetComboOnDeckDraw)
                        ResetCombo();
                    break;

                case GameMoveType.UseWildButton:
                    PlaySfx(wildSfxId);
                    break;

                case GameMoveType.Undo:
                    PlaySfx(undoSfxId);
                    if (resetComboOnUndo)
                        ResetCombo();
                    break;

                case GameMoveType.StartGame:
                    PlaySfx(deckDrawSfxId);
                    ResetCombo();
                    break;
            }
        }

        private void OnInvalidBoardCardSelected(int _)
        {
            PlaySfx(invalidMoveSfxId);
        }

        private IEnumerator PlayBackgroundMusicPlaylist()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0)
                yield break;

            var index = 0;

            while (_audioService != null)
            {
                var track = backgroundMusicTracks[index];
                if (!string.IsNullOrWhiteSpace(track.Id))
                    _audioService.PlayMusicAsync(track.Id);

                var duration = track.Clip != null ? track.Clip.length : fallbackTrackDuration;
                yield return new WaitForSeconds(duration + musicTrackGap);

                index++;
                if (index < backgroundMusicTracks.Length)
                    continue;

                if (!loopBackgroundPlaylist)
                    yield break;

                index = 0;
            }
        }

        private async void PlaySfx(string sfxId)
        {
            if (!CanPlayAudio(sfxId))
                return;

            await _audioService.PlaySfxAsync(sfxId);
        }

        private async void PlayComboStep()
        {
            var comboSfxId = GetComboSfxId(_comboStep);
            if (!CanPlayAudio(comboSfxId))
            {
                _comboStep++;
                return;
            }

            await _audioService.PlaySfxAsync(comboSfxId, GetComboPitch(_comboStep), comboVolumeScale);
            _comboStep++;
        }

        private bool CanPlayAudio(string sfxId)
        {
            return playGameServicesSfx && _audioService != null && !string.IsNullOrWhiteSpace(sfxId);
        }

        private string GetComboSfxId(int step)
        {
            if (comboSfxIds == null || comboSfxIds.Length == 0)
                return null;

            var startIndex = step % comboSfxIds.Length;
            for (var i = 0; i < comboSfxIds.Length; i++)
            {
                var index = (startIndex + i) % comboSfxIds.Length;
                if (!string.IsNullOrWhiteSpace(comboSfxIds[index]))
                    return comboSfxIds[index];
            }

            return null;
        }

        private float GetComboPitch(int step)
        {
            var scale = usePingPongScale ? PingPongScaleSemitones : AscendingScaleSemitones;
            var phraseLength = scale.Length;
            var phraseIndex = step / phraseLength;
            var noteIndex = step % phraseLength;
            var semitones = scale[noteIndex] - phraseIndex * phrasePitchDropSemitones;
            var pitch = Mathf.Pow(2f, semitones / 12f);
            return Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void ResetCombo()
        {
            _comboStep = 0;
        }
    }
}
