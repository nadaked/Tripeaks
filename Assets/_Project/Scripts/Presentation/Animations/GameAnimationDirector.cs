using System;
using System.Collections;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;
using _Project.Scripts.Core.Undo;
using _Project.Scripts.Presentation.Views.Board;
using _Project.Scripts.Presentation.Views.Card;
using _Project.Scripts.Presentation.Views.Deck;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Presentation.Animations
{
    public class GameAnimationDirector : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private BoardView boardView;
        [SerializeField] private WasteView wasteView;
        [SerializeField] private DeckView deckView;
        [SerializeField] private CardView ghostCardPrefab;
        [SerializeField] private Transform ghostRoot;

        [Header("Board to Waste")]
        [SerializeField] private float flyDuration = 0.35f;
        [SerializeField] private float arcHeight = 1.2f;
        [SerializeField] private float rotateDegrees = 360f;
        [SerializeField] private float landSqueezeTime = 0.08f;
        [SerializeField] private float landBounceHeight = 0.08f;
        [SerializeField] private float landBounceTime = 0.13f;
        [SerializeField] private float landBounceScale = 1.035f;

        [Header("Deck to Waste")]
        [SerializeField] private float deckToWasteDuration = 0.28f;
        [SerializeField] private float flipDuration = 0.18f;

        [Header("Initial Waste Deal")]
        [SerializeField] private bool playInitialWasteDeal = true;

        [Header("Reward to Deck")]
        [SerializeField] private float rewardSpawnInterval = 0.07f;
        [SerializeField] private float rewardFlyDuration = 0.22f;

        [Header("Deck Draw Hint")]
        [SerializeField] private bool shakePlayableCardsOnDeckDraw = true;
        [SerializeField] private float playableCardShakeDuration = 0.28f;
        [SerializeField] private float playableCardShakeStrength = 0.16f;
        [SerializeField] private float playableCardShakeRotation = 9f;
        [SerializeField] private float playableCardShakeScale = 1.06f;
        [SerializeField] private int playableCardShakeVibrato = 22;

        [Header("Initial Board Deal")]
        [SerializeField] private bool playInitialBoardDeal = true;
        [SerializeField] private Transform initialBoardDealStart;
        [SerializeField] private float initialBoardDealDuration = 0.34f;
        [SerializeField] private float initialBoardDealInterval = 0.025f;
        [SerializeField] private float initialBoardDealLift = 0.55f;
        [SerializeField] private float initialBoardDealStartScale = 0.86f;
        [SerializeField] private float initialBoardDealSettleScale = 1.06f;
        [SerializeField] private float initialBoardDealRotateMin = 80f;
        [SerializeField] private float initialBoardDealRotateMax = 90f;

        [Header("Initial Deck Deal")]
        [SerializeField] private bool playInitialDeckDeal = true;
        [SerializeField] private Transform initialDeckDealStart;
        [SerializeField] private float initialDeckDealDuration = 0.28f;
        [SerializeField] private float initialDeckDealInterval = 0.018f;
        [SerializeField] private float initialDeckDealLift = 0.4f;
        [SerializeField] private float initialDeckAccordionDuration = 0.24f;
        [SerializeField] private float initialDeckAccordionInterval = 0.012f;

        private GamePresenter _presenter;
        private int _activeAnimationCount;
        private CardView _activeWasteFlyingGhost;
        private CardData _activeWasteFlyingCard;
        private int _wasteFlyingVersion;
        private bool _initialWasteDealPlayed;
        private readonly MoveValidator _moveValidator = new();

        private void Awake()
        {
            if (playInitialWasteDeal && wasteView != null)
            {
                wasteView.SuppressSync();
                wasteView.HideCard();
            }

            if (!playInitialBoardDeal || boardView == null)
            {
                if (playInitialDeckDeal && deckView != null)
                    deckView.RequestInitialDeal(GetInitialDeckDealStartPosition());

                return;
            }

            boardView.RequestInitialDeal(GetInitialBoardDealStartPosition());

            if (playInitialDeckDeal && deckView != null)
                deckView.RequestInitialDeal(GetInitialDeckDealStartPosition());
        }

        private void Start()
        {
            _presenter = bootstrapper.Presenter;
            _presenter.MovePerformed += OnMovePerformed;
            _presenter.InvalidBoardCardSelected += OnInvalidBoardCardSelected;

            if (playInitialBoardDeal)
                StartCoroutine(PlayInitialBoardDealWhenReady());

            if (playInitialDeckDeal)
                StartCoroutine(PlayInitialDeckDealWhenReady());
            else if (playInitialWasteDeal)
                StartCoroutine(PlayInitialWasteDealWhenReady());
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.MovePerformed -= OnMovePerformed;
                _presenter.InvalidBoardCardSelected -= OnInvalidBoardCardSelected;
            }
        }

        private IEnumerator PlayInitialBoardDealWhenReady()
        {
            if (boardView == null || _presenter == null)
                yield break;

            while (!boardView.IsReady)
                yield return null;

            if (boardView.CardCount <= 0)
                yield break;

            PlayInitialBoardDeal();
        }

        private void PlayInitialBoardDeal()
        {
            var seq = DOTween.Sequence();
            var from = GetInitialBoardDealStartPosition();

            for (var i = 0; i < boardView.CardCount; i++)
            {
                var slotIndex = i;
                var cardTransform = boardView.GetCardTransform(slotIndex);
                var to = boardView.GetInitialDealTargetWorldPosition(slotIndex);
                var targetRotation = boardView.GetInitialDealTargetLocalRotation(slotIndex);
                var fly = PlayInitialBoardCardDeal(cardTransform, from, to, targetRotation);

                fly.SetDelay(slotIndex * initialBoardDealInterval);
                seq.Join(fly);
            }

            seq.OnComplete(() =>
            {
                boardView.CompleteInitialDeal();
            });
        }

        private IEnumerator PlayInitialDeckDealWhenReady()
        {
            if (_presenter == null)
                yield break;

            if (deckView == null)
            {
                PlayInitialWasteDeal();
                yield break;
            }

            while (!deckView.IsReady)
                yield return null;

            if (deckView.VisibleCardCount <= 0)
            {
                PlayInitialWasteDeal();
                yield break;
            }

            PlayInitialDeckDeal();
        }

        private void PlayInitialDeckDeal()
        {
            var seq = DOTween.Sequence();
            var from = GetInitialDeckDealStartPosition();
            var stackPosition = deckView.GetInitialDealStackWorldPosition();

            for (var i = 0; i < deckView.VisibleCardCount; i++)
            {
                var cardIndex = i;
                var cardTransform = deckView.GetCardTransform(cardIndex);
                var targetRotation = deckView.GetInitialDealTargetLocalRotation(cardIndex);
                var fly = PlayInitialDeckCardDeal(cardTransform, from, stackPosition, targetRotation);

                fly.SetDelay(cardIndex * initialDeckDealInterval);
                seq.Join(fly);
            }

            seq.Append(PlayInitialDeckAccordion());

            seq.OnComplete(() =>
            {
                deckView.CompleteInitialDeal();
                PlayInitialWasteDeal();
            });
        }

        private Tween PlayInitialDeckAccordion()
        {
            var seq = DOTween.Sequence();

            for (var i = 0; i < deckView.VisibleCardCount; i++)
            {
                var cardIndex = i;
                var cardTransform = deckView.GetCardTransform(cardIndex);
                var targetPosition = deckView.GetInitialDealTargetWorldPosition(cardIndex);
                var targetRotation = deckView.GetInitialDealTargetLocalRotation(cardIndex);

                var move = DOTween.Sequence();
                move.SetDelay(cardIndex * initialDeckAccordionInterval);
                move.Append(
                    cardTransform
                        .DOMove(targetPosition, initialDeckAccordionDuration)
                        .SetEase(Ease.OutCubic)
                );
                move.Join(
                    cardTransform
                        .DOLocalRotateQuaternion(targetRotation, initialDeckAccordionDuration)
                        .SetEase(Ease.OutCubic)
                );

                seq.Join(move);
            }

            return seq;
        }

        private IEnumerator PlayInitialWasteDealWhenReady()
        {
            if (_presenter == null)
                yield break;

            if (deckView != null)
            {
                while (!deckView.IsReady)
                    yield return null;
            }
            else
            {
                yield return null;
            }

            PlayInitialWasteDeal();
        }

        private Vector3 GetInitialBoardDealStartPosition()
        {
            if (initialBoardDealStart != null)
                return initialBoardDealStart.position;

            if (deckView != null)
                return deckView.GetWorldPosition();

            return transform.position;
        }

        private Vector3 GetInitialDeckDealStartPosition()
        {
            if (initialDeckDealStart != null)
                return initialDeckDealStart.position;

            if (initialBoardDealStart != null)
                return initialBoardDealStart.position;

            if (deckView != null)
                return deckView.GetWorldPosition() + Vector3.up * 2f;

            return transform.position;
        }

        private Tween PlayInitialBoardCardDeal(
            Transform cardTransform,
            Vector3 from,
            Vector3 to,
            Quaternion targetLocalRotation)
        {
            var seq = DOTween.Sequence();
            var startOffset = new Vector3(
                UnityEngine.Random.Range(-0.12f, 0.12f),
                UnityEngine.Random.Range(-0.08f, 0.08f),
                0f);
            var startPosition = from + startOffset;
            var midPoint = Vector3.Lerp(startPosition, to, 0.58f);
            midPoint.y = Mathf.Max(startPosition.y, to.y) + initialBoardDealLift;
            var targetScale = Vector3.one;
            var startScale = Vector3.one * initialBoardDealStartScale;
            var settleScale = Vector3.one * initialBoardDealSettleScale;
            var rotateSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            var startRotationOffset = UnityEngine.Random.Range(initialBoardDealRotateMin, initialBoardDealRotateMax) * rotateSign;
            var startRotation = targetLocalRotation * Quaternion.Euler(0f, 0f, startRotationOffset);
            var midRotation = targetLocalRotation * Quaternion.Euler(0f, 0f, startRotationOffset * 0.32f);

            seq.AppendCallback(() =>
            {
                cardTransform.position = startPosition;
                cardTransform.localScale = startScale;
                cardTransform.localRotation = startRotation;
            });

            seq.Append(
                cardTransform
                    .DOMove(midPoint, initialBoardDealDuration * 0.42f)
                    .SetEase(Ease.OutSine)
            );

            seq.Join(
                cardTransform
                    .DOScale(Vector3.one, initialBoardDealDuration * 0.42f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Join(
                cardTransform
                    .DOLocalRotateQuaternion(midRotation, initialBoardDealDuration * 0.42f)
                    .SetEase(Ease.OutSine)
            );

            seq.Append(
                cardTransform
                    .DOMove(to, initialBoardDealDuration * 0.58f)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(
                cardTransform
                    .DOLocalRotateQuaternion(targetLocalRotation, initialBoardDealDuration * 0.58f)
                    .SetEase(Ease.OutCubic)
            );

            seq.Append(
                cardTransform
                    .DOScale(settleScale, landSqueezeTime)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                cardTransform
                    .DOScale(targetScale, landSqueezeTime * 1.15f)
                    .SetEase(Ease.OutBack)
            );

            seq.AppendCallback(() =>
            {
                cardTransform.position = to;
                cardTransform.localRotation = targetLocalRotation;
                cardTransform.localScale = targetScale;
            });

            return seq;
        }

        private Tween PlayInitialDeckCardDeal(
            Transform cardTransform,
            Vector3 from,
            Vector3 to,
            Quaternion targetLocalRotation)
        {
            var seq = DOTween.Sequence();
            var startOffset = new Vector3(
                UnityEngine.Random.Range(-0.08f, 0.08f),
                UnityEngine.Random.Range(-0.05f, 0.05f),
                0f);
            var startPosition = from + startOffset;
            var midPoint = Vector3.Lerp(startPosition, to, 0.58f);
            midPoint.y = Mathf.Max(startPosition.y, to.y) + initialDeckDealLift;
            var targetScale = Vector3.one;
            var startScale = Vector3.one * initialBoardDealStartScale;
            var settleScale = Vector3.one * initialBoardDealSettleScale;
            var rotateSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            var startRotationOffset = UnityEngine.Random.Range(initialBoardDealRotateMin, initialBoardDealRotateMax) * rotateSign;
            var startRotation = targetLocalRotation * Quaternion.Euler(0f, 0f, startRotationOffset);
            var midRotation = targetLocalRotation * Quaternion.Euler(0f, 0f, startRotationOffset * 0.32f);

            seq.AppendCallback(() =>
            {
                cardTransform.position = startPosition;
                cardTransform.localScale = startScale;
                cardTransform.localRotation = startRotation;
            });

            seq.Append(
                cardTransform
                    .DOMove(midPoint, initialDeckDealDuration * 0.42f)
                    .SetEase(Ease.OutSine)
            );

            seq.Join(
                cardTransform
                    .DOScale(Vector3.one, initialDeckDealDuration * 0.42f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Join(
                cardTransform
                    .DOLocalRotateQuaternion(midRotation, initialDeckDealDuration * 0.42f)
                    .SetEase(Ease.OutSine)
            );

            seq.Append(
                cardTransform
                    .DOMove(to, initialDeckDealDuration * 0.58f)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(
                cardTransform
                    .DOLocalRotateQuaternion(targetLocalRotation, initialDeckDealDuration * 0.58f)
                    .SetEase(Ease.OutCubic)
            );

            seq.Append(
                cardTransform
                    .DOScale(settleScale, landSqueezeTime)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                cardTransform
                    .DOScale(targetScale, landSqueezeTime * 1.15f)
                    .SetEase(Ease.OutBack)
            );

            seq.AppendCallback(() =>
            {
                cardTransform.position = to;
                cardTransform.localRotation = targetLocalRotation;
                cardTransform.localScale = targetScale;
            });

            return seq;
        }

        private void OnMovePerformed(GameMoveResult result)
        {
            if (!result.Success) return;

            _activeAnimationCount++;
            if (ShouldLockInputForAnimation(result))
                _presenter.SetInputLocked(true);

            switch (result.MoveType)
            {
                case GameMoveType.PlayFromBoard:
                    PlayBoardMoveSequence(result);
                    break;

                case GameMoveType.DrawFromDeck:
                    ShakePlayableCardsBeforeDeckDraw(result);
                    PlayDeckToWaste(result);
                    break;

                case GameMoveType.StartGame:
                    PlayDeckToWaste(result);
                    break;

                case GameMoveType.Undo:
                    PlayUndo(result);
                    break;

                case GameMoveType.None:
                default:
                    Debug.Log("[Animation] Unknown move");
                    CompleteMove();
                    break;
            }
        }

        private void OnInvalidBoardCardSelected(int slotIndex)
        {
            ShakeBoardCard(slotIndex);
        }

        private void CompleteMove()
        {
            _activeAnimationCount = Mathf.Max(0, _activeAnimationCount - 1);

            if (_activeAnimationCount > 0)
                return;

            _presenter.PublishStateChanged();
            _presenter.SetInputLocked(false);
        }

        private bool ShouldLockInputForAnimation(GameMoveResult result)
        {
            if (result.Record == null)
                return false;

            if (result.MoveType != GameMoveType.PlayFromBoard && result.MoveType != GameMoveType.Undo)
                return false;

            return result.Record.AddedToDeck.Count > 0 ||
                   result.Record.UnlockResolvedSlots.Count > 0;
        }

        private void PlayUndo(GameMoveResult result)
        {
            var record = result.Record;
            var seq = DOTween.Sequence();

            wasteView.SuppressSync();

            // Deck draw undo ise:
            // Ghost waste'ten deck'e giderken waste altında eski kart görünmeli.
            if (record.PlayedSlotIndex < 0 && record.DrawnFromDeck.HasValue)
            {
                seq.AppendCallback(() =>
                {
                    if (record.HadWaste)
                        wasteView.ShowCard(record.PreviousWaste, true);
                    else
                        wasteView.HideCard();
                });

                seq.Append(PlayUndoWasteToDeck(record));

                seq.OnComplete(() =>
                {
                    wasteView.ReleaseAndSync();
                    CompleteMove();
                });

                return;
            }

            // Board move undo ise:
            // 1) Normal reveal edilen kartları geri kapat
            seq.Append(PlayUndoRevealCards(record));

            // 2) Açılmış özel kartları geri kapat
            seq.AppendCallback(() =>
            {
                foreach (var slotIndex in record.UnlockResolvedSlots)
                {
                    boardView.ShowBackAt(slotIndex);
                }
            });

            // 3) +3/wild ile deck'e eklenen kartlar geri uçsun
            if (record.AddedToDeck.Count > 0)
            {
                seq.Append(PlayUndoRewardCards(record));
            }

            // 4) +3/wild kartları board'da tekrar görünür olsun
            seq.AppendCallback(() =>
            {
                foreach (var slotIndex in record.UnlockResolvedSlots)
                {
                    boardView.ShowBackAt(slotIndex);
                }
            });

            // 5) Waste altında eski waste görünmeli, ghost 5 oradan board'a uçmalı
            seq.AppendCallback(() =>
            {
                if (record.HadWaste)
                    wasteView.ShowCard(record.PreviousWaste, true);
                else
                    wasteView.HideCard();
            });

            if (record.PlayedSlotIndex >= 0)
            {
                seq.Append(PlayUndoWasteToBoard(record));
                seq.AppendCallback(() =>
                {
                    boardView.ShowCardAt(record.PlayedSlotIndex);
                });
            }

            seq.OnComplete(() =>
            {
                wasteView.ReleaseAndSync();
                CompleteMove();
            });
        }

        private Tween PlayUndoRevealCards(MoveRecord record)
        {
            var seq = DOTween.Sequence();

            for (var i = record.RevealedSlots.Count - 1; i >= 0; i--)
            {
                var slotIndex = record.RevealedSlots[i];
                var cardTransform = boardView.GetCardTransform(slotIndex);
                var startScale = cardTransform.localScale;

                seq.Append(
                    cardTransform
                        .DOScaleX(0.08f, flipDuration * 0.5f)
                        .SetEase(Ease.InSine)
                );

                seq.AppendCallback(() =>
                {
                    boardView.ShowBackAt(slotIndex);
                });

                seq.Append(
                    cardTransform
                        .DOScaleX(startScale.x, flipDuration * 0.5f)
                        .SetEase(Ease.OutSine)
                );
            }

            return seq;
        }

        private Tween PlayUndoRewardCards(MoveRecord record)
        {
            var seq = DOTween.Sequence();

            if (record.UnlockResolvedSlots.Count <= 0)
                return seq;

            var from = deckView.GetWorldPosition();
            var cardIndex = record.AddedToDeck.Count - 1;

            for (var sourceIndex = record.UnlockResolvedSlots.Count - 1; sourceIndex >= 0; sourceIndex--)
            {
                var sourceSlotIndex = record.UnlockResolvedSlots[sourceIndex];
                var sourceSlot = _presenter.State.Board.GetSlot(sourceSlotIndex);

                if (!sourceSlot.Card.IsWild && !sourceSlot.Card.IsAddDeckCards)
                    continue;

                var amount = sourceSlot.Card.IsAddDeckCards ? sourceSlot.Card.Value : 1;
                var to = boardView.GetCardWorldPosition(sourceSlotIndex);
                var sourceSeq = DOTween.Sequence();

                for (var i = 0; i < amount && cardIndex >= 0; i++)
                {
                    var ghost = Instantiate(ghostCardPrefab, ghostRoot);

                    ghost.transform.position = from;
                    ghost.transform.rotation = Quaternion.identity;
                    ghost.transform.localScale = Vector3.one;
                    ghost.SetSortingOrder(130 + i);
                    ghost.ShowBack();

                    var delay = i * rewardSpawnInterval;

                    var fly = DOTween.Sequence();
                    fly.SetDelay(delay);
                    fly.Append(PlayGhostArcMove(ghost.transform, from, to, rewardFlyDuration, -rotateDegrees, false));

                    fly.OnComplete(() =>
                    {
                        Destroy(ghost.gameObject);
                    });

                    sourceSeq.Join(fly);
                    cardIndex--;
                }

                seq.Append(sourceSeq);
            }

            return seq;
        }

        private Tween PlayUndoWasteToBoard(MoveRecord record)
        {
            var seq = DOTween.Sequence();

            var from = wasteView.GetWorldPosition();
            var to = boardView.GetCardWorldPosition(record.PlayedSlotIndex);
            var targetCardTransform = boardView.GetCardTransform(record.PlayedSlotIndex);
            var targetRotation = targetCardTransform == null ? Quaternion.identity : targetCardTransform.rotation;

            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowCard(record.NewWaste, true);
            ghost.SetSortingOrder(200);

            seq.Append(PlayGhostArcMove(ghost.transform, from, to, flyDuration, -rotateDegrees, true, targetRotation));

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });

            return seq;
        }

        private Tween PlayUndoWasteToDeck(MoveRecord record)
        {
            var seq = DOTween.Sequence();

            var from = wasteView.GetWorldPosition();
            var to = deckView.GetWorldPosition();

            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowCard(record.DrawnFromDeck.Value, true);
            ghost.SetSortingOrder(200);

            seq.Append(PlayGhostArcMove(ghost.transform, from, to, deckToWasteDuration, -rotateDegrees, false));

            seq.Append(
                ghost.transform
                    .DOScaleX(0f, flipDuration * 0.5f)
                    .SetEase(Ease.InQuad)
            );

            seq.AppendCallback(() =>
            {
                ghost.ShowBack();
            });

            seq.Append(
                ghost.transform
                    .DOScaleX(1f, flipDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });

            return seq;
        }

        private void PlayDeckToWaste(GameMoveResult result)
        {
            var record = result.Record;

            if (!record.DrawnFromDeck.HasValue)
            {
                CompleteMove();
                return;
            }

            var card = record.DrawnFromDeck.Value;

            var from = deckView.GetWorldPosition();
            var to = wasteView.GetWorldPosition();

            var version = BeginWasteFlying(card);
            var ghost = CreateWasteFlyingGhost(card);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowBack();
            ghost.SetSortingOrder(100);

            wasteView.SuppressSync();

            var seq = DOTween.Sequence();

            seq.Append(
                ghost.transform
                    .DOMove(to, deckToWasteDuration)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(FlipToCard(ghost, card));

            seq.Append(PlayGhostLandBounce(ghost.transform, to, Vector3.one));

            seq.OnComplete(() =>
            {
                CompleteWasteFlying(version, ghost, card);
                CompleteMove();
            });
        }

        private void ShakePlayableCardsBeforeDeckDraw(GameMoveResult result)
        {
            if (!shakePlayableCardsOnDeckDraw)
                return;

            var record = result.Record;
            if (record == null || !record.HadWaste)
                return;

            var board = _presenter.State.Board;
            for (var i = 0; i < board.SlotCount; i++)
            {
                if (!board.IsSelectable(i))
                    continue;

                var slot = board.GetSlot(i);
                if (!_moveValidator.CanPlay(slot.Card, record.PreviousWaste))
                    continue;

                ShakeBoardCard(i);
            }
        }

        private void ShakeBoardCard(int slotIndex)
        {
            var cardTransform = boardView.GetCardTransform(slotIndex);
            if (cardTransform == null)
                return;

            var startLocalPosition = cardTransform.localPosition;
            var startLocalRotation = cardTransform.localRotation;
            var startLocalScale = cardTransform.localScale;
            var seq = DOTween.Sequence();

            seq.Join(cardTransform
                .DOShakePosition(
                    playableCardShakeDuration,
                    new Vector3(playableCardShakeStrength, 0f, 0f),
                    playableCardShakeVibrato,
                    0f,
                    false,
                    true));

            seq.Join(cardTransform
                .DOShakeRotation(
                    playableCardShakeDuration,
                    new Vector3(0f, 0f, playableCardShakeRotation),
                    playableCardShakeVibrato,
                    90f,
                    true));

            seq.Join(cardTransform
                .DOScale(startLocalScale * playableCardShakeScale, playableCardShakeDuration * 0.35f)
                .SetEase(Ease.OutQuad));

            seq.Append(cardTransform
                .DOScale(startLocalScale, playableCardShakeDuration * 0.25f)
                .SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                if (cardTransform != null)
                {
                    cardTransform.localPosition = startLocalPosition;
                    cardTransform.localRotation = startLocalRotation;
                    cardTransform.localScale = startLocalScale;
                }
            });
        }

        private void PlayInitialWasteDeal()
        {
            if (!playInitialWasteDeal || _initialWasteDealPlayed)
                return;

            _initialWasteDealPlayed = true;

            if (_presenter == null || !_presenter.State.Waste.HasCard)
            {
                wasteView.ReleaseAndSync();
                return;
            }

            var card = _presenter.State.Waste.Current;
            var from = deckView != null ? deckView.GetWorldPosition() : GetInitialDeckDealStartPosition();
            var to = wasteView.GetWorldPosition();

            var version = BeginWasteFlying(card);
            var ghost = CreateWasteFlyingGhost(card);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowBack();
            ghost.SetSortingOrder(120);

            var seq = DOTween.Sequence();

            seq.Append(
                ghost.transform
                    .DOMove(to, deckToWasteDuration)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(FlipToCard(ghost, card));

            seq.Append(PlayGhostLandBounce(ghost.transform, to, Vector3.one));

            seq.OnComplete(() =>
            {
                CompleteWasteFlying(version, ghost, card);
            });
        }

        private Tween PlayRewardAnimations(GameMoveResult result)
        {
            var seq = DOTween.Sequence();
            var record = result.Record;

            if (record.AddedToDeck.Count <= 0)
                return seq;

            var target = deckView.GetWorldPosition();
            var cardIndex = 0;

            foreach (var sourceSlotIndex in record.UnlockResolvedSlots)
            {
                var sourceSlot = _presenter.State.Board.GetSlot(sourceSlotIndex);

                if (!sourceSlot.Card.IsWild && !sourceSlot.Card.IsAddDeckCards)
                    continue;

                var amount = sourceSlot.Card.IsAddDeckCards ? sourceSlot.Card.Value : 1;
                var from = boardView.GetCardWorldPosition(sourceSlotIndex);
                var sourceSeq = DOTween.Sequence();

                sourceSeq.AppendCallback(() =>
                {
                    boardView.HideAt(sourceSlotIndex);
                });

                sourceSeq.Join(PlayRevealAnimations(result, sourceSlotIndex));

                for (var i = 0; i < amount && cardIndex < record.AddedToDeck.Count; i++)
                {
                    var card = record.AddedToDeck[cardIndex++];
                    sourceSeq.Join(PlayRewardCardFly(card, from, target, i));
                }

                seq.Append(sourceSeq);
            }

            return seq;
        }

        private Tween PlayRewardCardFly(CardData card, Vector3 from, Vector3 target, int index)
        {
            var ghost = Instantiate(ghostCardPrefab, ghostRoot);

            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.SetSortingOrder(100);

            if (card.IsWild)
                ghost.ShowCard(card, true);
            else
                ghost.ShowBack();

            var delay = index * rewardSpawnInterval;

            var seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(PlayGhostArcMove(ghost.transform, from, target, rewardFlyDuration, rotateDegrees, true));

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });

            return seq;
        }

        private void PlayBoardMoveSequence(GameMoveResult result)
        {
            var seq = DOTween.Sequence();

            boardView.SuppressSync();
            seq.Join(PlayBoardToWaste(result));
            seq.Join(PlayRevealAnimations(result, -1));
            seq.Append(PlayRewardAnimations(result));

            seq.OnComplete(() =>
            {
                boardView.ReleaseAndSync();
                CompleteMove();
            });
        }

        private Tween PlayRevealAnimations(GameMoveResult result, int unlockOwnerSlotIndex)
        {
            var seq = DOTween.Sequence();
            var record = result.Record;

            foreach (var slotIndex in record.RevealedSlots)
            {
                if (GetRevealUnlockOwner(record, slotIndex) != unlockOwnerSlotIndex)
                    continue;

                var cardTransform = boardView.GetCardTransform(slotIndex);
                var startScale = cardTransform.localScale;
                var cardSeq = DOTween.Sequence();

                cardSeq.AppendCallback(() =>
                {
                    boardView.ShowBackAt(slotIndex);
                });

                cardSeq.Append(
                    cardTransform
                        .DOScaleX(0.08f, flipDuration * 0.5f)
                        .SetEase(Ease.InSine)
                );

                cardSeq.AppendCallback(() =>
                {
                    boardView.ShowCardAt(slotIndex);
                });

                cardSeq.Append(
                    cardTransform
                        .DOScaleX(startScale.x, flipDuration * 0.5f)
                        .SetEase(Ease.OutSine)
                );

                seq.Join(cardSeq);
            }

            return seq;
        }

        private int GetRevealUnlockOwner(MoveRecord record, int revealedSlotIndex)
        {
            if (record == null || revealedSlotIndex < 0 || revealedSlotIndex >= _presenter.State.Board.SlotCount)
                return -1;

            var slot = _presenter.State.Board.GetSlot(revealedSlotIndex);
            var ownerSlotIndex = -1;
            var ownerOrder = -1;

            foreach (var blockerIndex in slot.BlockedBy)
            {
                var unlockOrder = GetUnlockResolvedOrder(record, blockerIndex);
                if (unlockOrder <= ownerOrder)
                    continue;

                ownerOrder = unlockOrder;
                ownerSlotIndex = blockerIndex;
            }

            return ownerSlotIndex;
        }

        private static int GetUnlockResolvedOrder(MoveRecord record, int slotIndex)
        {
            for (var i = 0; i < record.UnlockResolvedSlots.Count; i++)
            {
                if (record.UnlockResolvedSlots[i] == slotIndex)
                    return i;
            }

            return -1;
        }

        private Tween PlayBoardToWaste(GameMoveResult result)
        {
            var seq = DOTween.Sequence();
            var record = result.Record;

            if (record.PlayedSlotIndex < 0)
                return seq;

            var playedSlot = _presenter.State.Board.GetSlot(record.PlayedSlotIndex);
            var playedCard = playedSlot.Card;

            if (playedCard.IsWild || playedCard.IsAddDeckCards)
                return seq;

            var from = boardView.GetCardWorldPosition(record.PlayedSlotIndex);
            var to = wasteView.GetWorldPosition();

            var version = BeginWasteFlying(record.NewWaste);
            var ghost = CreateWasteFlyingGhost(record.NewWaste);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowCard(record.NewWaste, true);
            ghost.SetSortingOrder(100);

            boardView.HideAt(record.PlayedSlotIndex);
            wasteView.SuppressSync();

            seq.Append(PlayGhostArcMove(ghost.transform, from, to, flyDuration, rotateDegrees, true));

            seq.OnComplete(() =>
            {
                CompleteWasteFlying(version, ghost, record.NewWaste);
            });

            return seq;
        }

        private int BeginWasteFlying(CardData card)
        {
            if (_activeWasteFlyingGhost != null)
            {
                wasteView.ShowCard(_activeWasteFlyingCard, true);
                wasteView.ReleaseWithoutSync();
                _activeWasteFlyingGhost = null;
            }

            return ++_wasteFlyingVersion;
        }

        private CardView CreateWasteFlyingGhost(CardData card)
        {
            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            _activeWasteFlyingGhost = ghost;
            _activeWasteFlyingCard = card;
            return ghost;
        }

        private void CompleteWasteFlying(int version, CardView ghost, CardData card)
        {
            if (version != _wasteFlyingVersion)
            {
                if (ghost != null)
                {
                    ghost.gameObject.SetActive(false);
                    Destroy(ghost.gameObject);
                }

                return;
            }

            wasteView.ShowCard(card, true);
            wasteView.ReleaseAndSync();

            if (_activeWasteFlyingGhost == ghost)
                _activeWasteFlyingGhost = null;

            if (ghost != null)
            {
                ghost.gameObject.SetActive(false);
                Destroy(ghost.gameObject);
            }
        }

        private Tween FlipToCard(CardView cardView, CardData card)
        {
            var seq = DOTween.Sequence();

            seq.Append(
                cardView.transform
                    .DOScaleX(0f, flipDuration * 0.5f)
                    .SetEase(Ease.InQuad)
            );

            seq.AppendCallback(() =>
            {
                cardView.ShowCard(card, true);
            });

            seq.Append(
                cardView.transform
                    .DOScaleX(1f, flipDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );

            return seq;
        }

        private Tween PlayGhostArcMove(
            Transform ghostTransform,
            Vector3 from,
            Vector3 to,
            float duration,
            float rotateAmount,
            bool squeezeOnLand,
            Quaternion? targetWorldRotation = null)
        {
            var seq = DOTween.Sequence();
            var startScale = ghostTransform.localScale;
            var finalRotation = targetWorldRotation ?? Quaternion.identity;
            var moveToPeakTime = duration * 0.58f;
            var moveToTargetTime = duration - moveToPeakTime;
            var peak = new Vector3(
                to.x,
                Mathf.Max(from.y, to.y) + arcHeight,
                to.z
            );

            var rotateDir = from.x < to.x ? -1f : 1f;
            var signedRotation = Mathf.Abs(rotateAmount) * rotateDir * Mathf.Sign(rotateAmount == 0f ? 1f : rotateAmount);

            seq.Append(
                ghostTransform
                    .DOMove(peak, moveToPeakTime)
                    .SetEase(Ease.OutQuad)
            );

            seq.Join(
                ghostTransform
                    .DORotate(new Vector3(0f, 0f, signedRotation), moveToPeakTime, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghostTransform
                    .DOMove(to, moveToTargetTime)
                    .SetEase(Ease.InQuad)
            );

            seq.Join(
                ghostTransform
                    .DORotateQuaternion(finalRotation, moveToTargetTime)
                    .SetEase(Ease.InQuad)
            );

            if (squeezeOnLand)
                seq.Append(PlayGhostLandBounce(ghostTransform, to, startScale));

            seq.AppendCallback(() =>
            {
                ghostTransform.localScale = startScale;
                ghostTransform.rotation = finalRotation;
            });

            return seq;
        }

        private Tween PlayGhostLandBounce(Transform ghostTransform, Vector3 targetPosition, Vector3 targetScale)
        {
            var seq = DOTween.Sequence();
            var bouncePosition = targetPosition + Vector3.up * landBounceHeight;
            var bounceScale = targetScale * landBounceScale;

            seq.Append(
                ghostTransform
                    .DOMove(bouncePosition, landBounceTime * 0.42f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Join(
                ghostTransform
                    .DOScale(bounceScale, landBounceTime * 0.42f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghostTransform
                    .DOMove(targetPosition, landBounceTime * 0.58f)
                    .SetEase(Ease.OutBack)
            );

            seq.Join(
                ghostTransform
                    .DOScale(targetScale, landBounceTime * 0.58f)
                    .SetEase(Ease.OutBack)
            );

            seq.AppendCallback(() =>
            {
                ghostTransform.position = targetPosition;
                ghostTransform.localScale = targetScale;
            });

            return seq;
        }
    }
}
