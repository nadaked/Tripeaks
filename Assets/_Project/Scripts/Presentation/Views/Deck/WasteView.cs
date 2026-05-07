using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Presentation.Views.Card;
using UnityEngine;

namespace _Project.Scripts.Presentation.Views.Deck
{
    public sealed class WasteView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private CardView cardView;

        private GamePresenter _presenter;

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

        private void Sync()
        {
            var state = _presenter.State;

            if (!state.Waste.HasCard)
            {
                gameObject.SetActive(false);
                return;
            }

            var card = state.Waste.Current;

            if (card.IsAddDeckCards)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (card.IsWild)
                cardView.ShowSpecial(card);
            else
                cardView.ShowNormal(card, true);
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }
}