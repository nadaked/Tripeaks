using System;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;
using _Project.Scripts.Presentation.Views.Board;
using _Project.Scripts.Presentation.Views.Card;
using _Project.Scripts.Presentation.Views.Deck;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Presentation.Animations
{
    public class GameAnimationDirector : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private BoardView boardView;
        [SerializeField] private WasteView wasteView;
        [SerializeField] private DeckView deckView;
        [SerializeField] private CardView ghostCardPrefab;
        [SerializeField] private Transform ghostRoot;

        [Header("Board to Waste")] 
        [SerializeField] private float flyDuration = 0.35f;
        [SerializeField] private float arcHeight = 1.2f;
        [SerializeField] private float rotateDegrees = 360f;
        
        [Header("Deck to Waste")] 
        [SerializeField] private float deckToWasteDuration = 0.28f;
        [SerializeField] private float flipDuration = 0.18f;

        [Header("Reward to Deck")]
        [SerializeField] private float rewardSpawnInterval = 0.07f;
        [SerializeField] private float rewardFlyDuration = 0.22f;
        
        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;
            _presenter.MovePerformed += OnMovePerformed;
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.MovePerformed -= OnMovePerformed;
        }

        private void OnMovePerformed(GameMoveResult result)
        {
            if (!result.Success) return;

            switch (result.MoveType)
            {
                case GameMoveType.PlayFromBoard:
                    PlayBoardToWaste(result);
                    PlayRewardAnimations(result);
                    break;
                case GameMoveType.DrawFromDeck:
                case GameMoveType.StartGame:
                    PlayDeckToWaste(result);
                    break;
                case GameMoveType.Undo:
                    PlayUndo(result);
                    break;
                case GameMoveType.None:
                default:
                    Debug.Log("[Animation] Unknown move");
                    break;
            }
        }

        private void PlayUndo(GameMoveResult result)
        {
            Debug.Log("Animation: Undo reverse animation later");
        }

        private void PlayDeckToWaste(GameMoveResult result)
        {
            var record = result.Record;

            if (!record.DrawnFromDeck.HasValue)
                return;

            var card = record.DrawnFromDeck.Value;

            var from = deckView.GetWorldPosition();
            var to = wasteView.GetWorldPosition();

            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowBack();

            var seq = DOTween.Sequence();

            seq.Append(
                ghost.transform
                    .DOMove(to, deckToWasteDuration)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(FlipToCard(ghost, card));

            seq.Append(
                ghost.transform
                    .DOScale(new Vector3(1.12f, 0.88f, 1f), 0.07f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghost.transform
                    .DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.OutBack)
            );

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });
        }

        private void PlayRewardAnimations(GameMoveResult result)
        {
            var record = result.Record;

            if (record.AddedToDeck.Count <= 0)
                return;

            var target = deckView.GetWorldPosition();
            var cardIndex = 0;

            foreach (var sourceSlotIndex in record.UnlockResolvedSlots)
            {
                var sourceSlot = _presenter.State.Board.GetSlot(sourceSlotIndex);

                if (!sourceSlot.Card.IsWild && !sourceSlot.Card.IsAddDeckCards)
                    continue;

                var amount = sourceSlot.Card.IsAddDeckCards ? sourceSlot.Card.Value : 1;
                var from = boardView.GetCardWorldPosition(sourceSlotIndex);

                for (var i = 0; i < amount && cardIndex < record.AddedToDeck.Count; i++)
                {
                    var card = record.AddedToDeck[cardIndex++];
                    PlayRewardCardFly(card, from, target, i);
                }
            }
        }
        
        private void PlayRewardCardFly(CardData card, Vector3 from, Vector3 target, int index)
        {
            var ghost = Instantiate(ghostCardPrefab, ghostRoot);

            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;

            ghost.ShowBack();

            var delay = index * rewardSpawnInterval;

            var mid = new Vector3(
                (from.x + target.x) * 0.5f,
                Mathf.Max(from.y, target.y) + arcHeight,
                (from.z + target.z) * 0.5f
            );

            var seq = DOTween.Sequence();
            seq.SetDelay(delay);

            seq.Append(
                ghost.transform
                    .DOPath(new[] { from, mid, target }, rewardFlyDuration, PathType.CatmullRom)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(
                ghost.transform
                    .DORotate(new Vector3(0f, 0f, 360f), rewardFlyDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );

            seq.Append(
                ghost.transform
                    .DOScale(new Vector3(1.12f, 0.88f, 1f), 0.06f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghost.transform
                    .DOScale(Vector3.one, 0.08f)
                    .SetEase(Ease.OutBack)
            );

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });
        }

        private void PlayBoardToWaste(GameMoveResult result)
        {
            var record = result.Record;

            if (record.PlayedSlotIndex < 0)
                return;

            var playedSlot = _presenter.State.Board.GetSlot(record.PlayedSlotIndex);
            var playedCard = playedSlot.Card;

            if (playedCard.IsWild || playedCard.IsAddDeckCards)
                return;

            var from = boardView.GetCardWorldPosition(record.PlayedSlotIndex);
            var to = wasteView.GetWorldPosition();

            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            ghost.ShowCard(record.NewWaste, true);

            var mid = new Vector3(
                (from.x + to.x) * 0.5f,
                Mathf.Max(from.y, to.y) + arcHeight,
                (from.z + to.z) * 0.5f
            );

            var seq = DOTween.Sequence();

            seq.Append(
                ghost.transform
                    .DOPath(new[] { from, mid, to }, flyDuration, PathType.CatmullRom)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(
                ghost.transform
                    .DORotate(new Vector3(0f, 0f, rotateDegrees), flyDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );

            seq.Append(
                ghost.transform
                    .DOScale(new Vector3(1.15f, 0.85f, 1f), 0.07f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghost.transform
                    .DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.OutBack)
            );

            seq.OnComplete(() =>
            {
                Destroy(ghost.gameObject);
            });
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
    }
}