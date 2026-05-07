using System;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
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

            ghost.ShowBack();

            var seq = DOTween.Sequence();

            seq.Append(
                ghost.transform
                    .DOMove(to, deckToWasteDuration)
                    .SetEase(Ease.OutCubic)
            );

            seq.Join(
                ghost.transform
                    .DORotate(new Vector3(0f, 180f, 0f), flipDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        if (card.IsWild)
                            ghost.ShowSpecial(card);
                        else
                            ghost.ShowNormal(card, true);
                    })
            );

            /*seq.Append(
                ghost.transform
                    .DOScale(new Vector3(1.12f, 0.88f, 1f), 0.07f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                ghost.transform
                    .DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.OutBack)
            );*/

            seq.OnComplete(() => Destroy(ghost.gameObject));
        }

        private void PlayRewardAnimations(GameMoveResult result)
        {
            var record = result.Record;

            if (record.AddedToDeck.Count <= 0)
                return;

            var target = deckView.GetWorldPosition();

            for (var i = 0; i < record.AddedToDeck.Count; i++)
            {
                var card = record.AddedToDeck[i];

                var ghost = Instantiate(ghostCardPrefab, ghostRoot);

                ghost.transform.position = target + new Vector3(0f, 1.2f, 0f);
                ghost.transform.rotation = Quaternion.identity;
                ghost.transform.localScale = Vector3.zero;

                if (card.IsWild)
                    ghost.ShowSpecial(card);
                else
                    ghost.ShowBack();

                var delay = i * rewardSpawnInterval;

                var seq = DOTween.Sequence();
                seq.SetDelay(delay);

                /*seq.Append(
                    ghost.transform
                        .DOScale(Vector3.one, 0.08f)
                        .SetEase(Ease.OutBack)
                );

                seq.Append(
                    ghost.transform
                        .DOMove(target, rewardFlyDuration)
                        .SetEase(Ease.InCubic)
                );*/

                seq.Join(
                    ghost.transform
                        .DORotate(new Vector3(0f, 360f, 0f), rewardFlyDuration, RotateMode.FastBeyond360)
                );

                seq.OnComplete(() => Destroy(ghost.gameObject));
            }
        }

        private void PlayBoardToWaste(GameMoveResult result)
        {
            var record = result.Record;
            
            if (record.PlayedSlotIndex < 0)
                return;

            var from = boardView.GetCardWorldPosition(record.PlayedSlotIndex);
            var to = wasteView.GetWorldPosition();

            //TODO: make me pool
            var ghost = Instantiate(ghostCardPrefab, ghostRoot);
            ghost.transform.position = from;
            ghost.transform.rotation = Quaternion.identity;

            ghost.ShowNormal(record.NewWaste, true);

            var mid = new Vector3(
                (from.x + to.x) * 0.5f,
                Mathf.Max(from.y, to.y) + arcHeight,
                (from.z + to.z) * .5f
            );

            var path = new[] { from, mid, to };

            var sequence = DOTween.Sequence();

            sequence.Join(
                ghost.transform
                    .DOPath(path, flyDuration, PathType.CatmullRom)
                    .SetEase(Ease.OutCubic)
            );

            sequence.Join(
                ghost.transform
                    .DORotate(new Vector3(0f, rotateDegrees, 0f), flyDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );

            //TODO: limon sıkma yapılacak
            /*sequence.Append(
                ghost.transform
                    .DOScale(new Vector3(1.15f, 0.85f, 1f), 0.08f)
                    .SetEase(Ease.OutQuad)
            );

            sequence.Append(
                ghost.transform
                    .DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.OutBack)
            );*/

            sequence.OnComplete(() =>
            {
                //TODO: return to pool
                Destroy(ghost.gameObject);
            });
        }
    }
}