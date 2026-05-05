using _Project.Scripts.Core.Actions;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Undo;

namespace _Project.Scripts.Core.Game
{
    public sealed class GameController
    {
        private readonly GameState _state;
        private readonly MoveValidator _validator;
        private readonly ActionResolver _resolver;
        private readonly UndoSystem _undo;

        public GameController(GameState state, ICardProvider cardProvider)
        {
            _state = state;
            _validator = new MoveValidator();
            _resolver = new ActionResolver(cardProvider);
            _undo = new UndoSystem();
        }

        public bool StartGame()
        {
            if (_state.Waste.HasCard) return false;
            
            if (!_state.Deck.CanDraw()) return false;

            var card = _state.Deck.Draw();
            _state.Waste.Set(card);

            return true;
        }

        public bool TryPlayFromBoard(int slotIndex)
        {
            if (!_state.Board.IsSelectable(slotIndex))
                return false;

            var slot = _state.Board.GetSlot(slotIndex);

            if (!_validator.CanPlay(slot.Card, _state.Waste.Current))
                return false;

            var record = new MoveRecord
            {
                PlayedSlotIndex = slotIndex,
                PreviousWaste = _state.Waste.Current,
                HadWaste = _state.Waste.HasCard
            };

            // remove from board
            _state.Board.RemoveSlot(slotIndex);
            record.RemovedSlots.Add(slotIndex);

            // push to waste
            _state.Waste.Set(slot.Card);
            record.NewWaste = slot.Card;

            ResolveUnlockSlots(record);
            
            _undo.Push(record);

            return true;
        }

        public bool TryDrawFromDeck()
        {
            if (!_state.Deck.CanDraw())
                return false;
            var record = new MoveRecord
            {
                PreviousWaste = _state.Waste.Current,
                HadWaste = _state.Waste.HasCard
            };
            var card = _state.Deck.Draw();
            record.DrawnFromDeck = card;
            
            _state.Waste.Set(card);
            
            _undo.Push(record);

            return true;
        }

        private void ResolveUnlockSlots(MoveRecord record)
        {
            for (var i = 0; i < _state.Board.SlotCount; i++)
            {
                if (!_state.Board.IsSelectable(i)) continue;

                var slot = _state.Board.GetSlot(i);
                var state = _state.Board.GetState(i);

                if (state.IsUnlockResolved) continue;

                if (slot.UnlockAction.Type == GameActionType.None) continue;

                _resolver.Resolve(_state, slot.UnlockAction, record);
                state.MarkUnlockResolved();
                record.UnlockResolvedSlots.Add(i);
            }
        }

        public bool Undo()
        {
            if (!_undo.CanUndo()) return false;

            var record = _undo.Pop();
            
            if (record.HadWaste)
                _state.Waste.Set(record.PreviousWaste);
            else
                _state.Waste.Clear();
            
            if (record.DrawnFromDeck.HasValue)
                _state.Deck.AddToTop(record.DrawnFromDeck.Value);
            
            UnityEngine.Debug.Log($"AddedToDeck Count: {record.AddedToDeck.Count}");
            if (record.AddedToDeck.Count > 0)
                _state.Deck.RemoveFromBottom(record.AddedToDeck.Count);
            
            foreach (var slotIndex in record.RemovedSlots)
                _state.Board.RestoreSlot(slotIndex);

            foreach (var slotIndex in record.UnlockResolvedSlots)
                _state.Board.GetState(slotIndex).ResetUnlockResolved();
            
            return true;
        }
    }
}