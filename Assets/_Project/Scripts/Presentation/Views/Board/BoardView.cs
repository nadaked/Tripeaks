using UnityEngine;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private BoardCardView[] cards;

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                cards[i].Init(i);
            }

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

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                if (i >= state.Board.SlotCount)
                {
                    cards[i].gameObject.SetActive(false);
                    continue;
                }

                var slot = state.Board.GetSlot(i);
                var selectable = state.Board.IsSelectable(i);
                var removed = state.Board.IsRemoved(i);

                cards[i].Sync(slot.Card, selectable, removed);
            }
        }
    }
}