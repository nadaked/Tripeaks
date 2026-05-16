using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardInputView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool logFailedMoves = true;

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            var screenPosition = Mouse.current.position.ReadValue();
            var ray = targetCamera.ScreenPointToRay(screenPosition);
            var card = FindTopBoardCard(ray);

            if (card == null)
                return;

            var result = _presenter.PlayBoardSlot(card.SlotIndex);

            if (!result.Success && logFailedMoves)
                LogFailedMove(card);
        }

        private static BoardCardView FindTopBoardCard(Ray ray)
        {
            BoardCardView bestCard = null;
            var bestSortingOrder = int.MinValue;
            var bestDistance = float.MaxValue;

            foreach (var hit in Physics.RaycastAll(ray))
            {
                var hitCard = hit.collider.GetComponentInParent<BoardCardView>();
                if (hitCard == null)
                    continue;

                PickIfHigher(hitCard, hit.distance, ref bestCard, ref bestSortingOrder, ref bestDistance);
            }

            foreach (var hit in Physics2D.GetRayIntersectionAll(ray))
            {
                var hitCard = hit.collider.GetComponentInParent<BoardCardView>();
                if (hitCard == null)
                    continue;

                PickIfHigher(hitCard, hit.distance, ref bestCard, ref bestSortingOrder, ref bestDistance);
            }

            return bestCard;
        }

        private static void PickIfHigher(
            BoardCardView hitCard,
            float distance,
            ref BoardCardView bestCard,
            ref int bestSortingOrder,
            ref float bestDistance)
        {
            var sortingOrder = hitCard.GetSortingOrder();
            if (sortingOrder < bestSortingOrder)
                return;

            if (sortingOrder == bestSortingOrder && distance >= bestDistance)
                return;

            bestCard = hitCard;
            bestSortingOrder = sortingOrder;
            bestDistance = distance;
        }

        private void LogFailedMove(BoardCardView card)
        {
            var state = _presenter.State;
            var slotIndex = card.SlotIndex;

            if (slotIndex < 0 || slotIndex >= state.Board.SlotCount)
            {
                Debug.LogWarning($"Board click failed. Slot index out of range: {slotIndex}", card);
                return;
            }

            var slot = state.Board.GetSlot(slotIndex);
            var selectable = state.Board.IsSelectable(slotIndex);
            var waste = state.Waste.HasCard ? state.Waste.Current.ToString() : "Empty";

            Debug.LogWarning(
                $"Board click failed. Slot: {slotIndex}, Card: {slot.Card}, Selectable: {selectable}, Waste: {waste}, InputLocked: {_presenter.IsInputLocked}",
                card);
        }
    }
}
