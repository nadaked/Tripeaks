using _Project.Scripts.Application.Presenters;
using UnityEngine;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardCardView : MonoBehaviour
    {
        [SerializeField] private CardView cardView;
        private GamePresenter _presenter;

        public int SlotIndex { get; private set; }

        public void Init(int slotIndex, GamePresenter presenter)
        {
            SlotIndex = slotIndex;
            _presenter = presenter;

            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();
            
            cardView.Clicked += OnCardClicked;

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
            if (cardView != null)
                cardView.Clicked -= OnCardClicked;
        }

        private void OnCardClicked()
        {
            _presenter.PlayBoardSlot(SlotIndex);
        }

        public void ShowBack()
        {
            cardView.ShowBack();
        }
    }
}