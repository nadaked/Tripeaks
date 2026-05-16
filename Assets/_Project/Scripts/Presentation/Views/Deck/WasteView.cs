using UnityEngine;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Deck
{
    public sealed class WasteView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private CardView cardView;

        private GamePresenter _presenter;

        private int _suppressSyncCount;
        private bool _hasDisplayedCard;
        private CardData _displayedCard;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;
            _presenter.StateChanged += Sync;

            Sync();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= Sync;
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public void SuppressSync()
        {
            _suppressSyncCount++;
        }

        public void ReleaseAndSync()
        {
            _suppressSyncCount = Mathf.Max(0, _suppressSyncCount - 1);

            if (_suppressSyncCount == 0)
                Sync();
        }

        public void ReleaseWithoutSync()
        {
            _suppressSyncCount = Mathf.Max(0, _suppressSyncCount - 1);
        }

        private void ApplyCard(CardData card)
        {
            _displayedCard = card;
            _hasDisplayedCard = true;

            gameObject.SetActive(true);
            cardView.SetSortingOrder(30);
            cardView.ShowCard(card, true);
            cardView.SetClickEnabled(false);
        }

        public void ApplyEmpty()
        {
            _hasDisplayedCard = false;
            gameObject.SetActive(false);
        }

        private void Sync()
        {
            if (_suppressSyncCount > 0)
                return;

            var state = _presenter.State;

            if (!state.Waste.HasCard)
            {
                ApplyEmpty();
                return;
            }

            var card = state.Waste.Current;

            if (card.IsAddDeckCards)
                return;

            ApplyCard(card);
        }
        
        public void ShowCard(CardData card, bool instant = true)
        {
            _displayedCard = card;
            _hasDisplayedCard = true;

            gameObject.SetActive(true);
            cardView.gameObject.SetActive(true);
            cardView.SetSortingOrder(30);
            cardView.ShowCard(card, instant);
            cardView.SetClickEnabled(false);
        }

        public void HideCard()
        {
            cardView.gameObject.SetActive(false);
        }
    }
}
