using _Project.Scripts.Application.Presenters;
using UnityEngine;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardCardView : MonoBehaviour
    {
        [SerializeField] private CardView cardView;
        [SerializeField] private bool useDirectPointerInput;
        private GamePresenter _presenter;

        public int SlotIndex { get; private set; }

        public void Init(int slotIndex, GamePresenter presenter)
        {
            SlotIndex = slotIndex;
            _presenter = presenter;

            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();

            if (useDirectPointerInput)
                cardView.Clicked += OnCardClicked;
            else
                cardView.SetClickEnabled(false);

        }

        public void Sync(CardData card, bool selectable, bool removed)
        {
            gameObject.SetActive(!removed);

            if (removed)
                return;
            
            if (card.IsWild || card.IsAddDeckCards)
                cardView.ShowCard(card, true);
            else
                cardView.ShowCard(card, selectable);
        }

        private void OnDestroy()
        {
            if (cardView != null && useDirectPointerInput)
                cardView.Clicked -= OnCardClicked;
        }

        private void OnCardClicked()
        {
            _presenter.PlayBoardSlot(SlotIndex);
        }

        public void ShowBack()
        {
            gameObject.SetActive(true);
            cardView.ShowBack();
        }

        public void ShowCard(CardData card, bool faceUp)
        {
            gameObject.SetActive(true);
            cardView.ShowCard(card, faceUp);
        }

        public void SetSortingOrder(int order)
        {
            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();

            cardView.SetSortingOrder(order);
        }

        public void SetClickEnabled(bool clickEnabled)
        {
            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();

            cardView.SetClickEnabled(useDirectPointerInput && clickEnabled);
        }

        public int GetSortingOrder()
        {
            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();

            return cardView == null ? 0 : cardView.GetSortingOrder();
        }
    }
}
