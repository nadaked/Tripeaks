using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using UnityEngine;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardDebugView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private SlotClickView[] slots;

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            // Slotları presenter ile initialize et
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    continue;

                slots[i].Init(i);
            }

            _presenter.StateChanged += OnStateChanged;

            Render();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void Render()
        {
            var state = _presenter.State;

            Debug.Log("===== BOARD STATE =====");

            for (var i = 0; i < state.Board.SlotCount; i++)
            {
                var slot = state.Board.GetSlot(i);
                var removed = state.Board.IsRemoved(i);
                var selectable = state.Board.IsSelectable(i);
                
                if (i < slots.Length && slots[i] != null)
                    slots[i].SetVisual(selectable, removed);

                Debug.Log(
                    $"Slot {i} | Removed: {removed} | Selectable: {selectable} | Card: {slot.Card.Type} {slot.Card.Rank}"
                );
            }

            if (state.Waste.HasCard)
                Debug.Log($"Waste: {state.Waste.Current.Type} {state.Waste.Current.Rank}");
            else
                Debug.Log("Waste: EMPTY");

            Debug.Log($"Deck Count: {state.Deck.Count}");
            Debug.Log($"Game Status: {state.Status}");

            Debug.Log("=======================");
        }
    }
}