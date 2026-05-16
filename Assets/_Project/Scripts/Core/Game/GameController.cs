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

        public GameMoveResult StartGame()
        {
            if (_state.Waste.HasCard) return GameMoveResult.Failed(GameMoveType.StartGame);
            
            if (!_state.Deck.CanDraw()) return GameMoveResult.Failed(GameMoveType.StartGame);
            
            var record = new MoveRecord();
            
            var card = _state.Deck.Draw();
            record.DrawnFromDeck = card;
            
            _state.Waste.Set(card);
            record.NewWaste = card;
            
            EvaluateGameStatus();
            
            return GameMoveResult.Succeeded(GameMoveType.StartGame, record);;
        }

        public GameMoveResult TryPlayFromBoard(int slotIndex)
        {
            if (!_state.Board.IsSelectable(slotIndex))
                return GameMoveResult.Failed(GameMoveType.PlayFromBoard);

            var slot = _state.Board.GetSlot(slotIndex);

            if (!_validator.CanPlay(slot.Card, _state.Waste.Current))
                return GameMoveResult.Failed(GameMoveType.PlayFromBoard);

            var record = new MoveRecord
            {
                PlayedSlotIndex = slotIndex,
                PreviousWaste = _state.Waste.Current,
                HadWaste = _state.Waste.HasCard
            };

            var selectableBeforeMove = CaptureSelectableSlots();

            // remove from board
            _state.Board.RemoveSlot(slotIndex);
            record.RemovedSlots.Add(slotIndex);

            // push to waste
            _state.Waste.Set(slot.Card);
            record.NewWaste = slot.Card;

            ResolveUnlockSlots(record);
            RecordRevealedSlots(record, selectableBeforeMove);
            
            _undo.Push(record);
            
            EvaluateGameStatus();

            return GameMoveResult.Succeeded(GameMoveType.PlayFromBoard, record);
        }

        public GameMoveResult TryDrawFromDeck()
        {
            if (!_state.Deck.CanDraw())
                return GameMoveResult.Failed(GameMoveType.DrawFromDeck);
            var record = new MoveRecord
            {
                PreviousWaste = _state.Waste.Current,
                HadWaste = _state.Waste.HasCard
            };
            var card = _state.Deck.Draw();
            record.DrawnFromDeck = card;
            
            _state.Waste.Set(card);
            
            _undo.Push(record);
            
            EvaluateGameStatus();
            
            return GameMoveResult.Succeeded(GameMoveType.DrawFromDeck, record);
        }

        private void ResolveUnlockSlots(MoveRecord record)
        {
            var resolvedAny = true;

            while (resolvedAny)
            {
                resolvedAny = false;

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
                    
                    _state.Board.RemoveSlot(i);
                    record.RemovedSlots.Add(i);

                    resolvedAny = true;
                }
            }
        }

        private bool[] CaptureSelectableSlots()
        {
            var selectable = new bool[_state.Board.SlotCount];

            for (var i = 0; i < selectable.Length; i++)
                selectable[i] = _state.Board.IsSelectable(i);

            return selectable;
        }

        private void RecordRevealedSlots(MoveRecord record, bool[] selectableBeforeMove)
        {
            for (var i = 0; i < _state.Board.SlotCount; i++)
            {
                if (_state.Board.IsRemoved(i)) continue;

                if (!_state.Board.IsSelectable(i)) continue;

                if (selectableBeforeMove[i]) continue;

                record.RevealedSlots.Add(i);
            }
        }

        public GameMoveResult Undo()
        {
            if (!_undo.CanUndo()) return GameMoveResult.Failed(GameMoveType.Undo);

            var record = _undo.Pop();
            
            if (record.HadWaste)
                _state.Waste.Set(record.PreviousWaste);
            else
                _state.Waste.Clear();
            
            if (record.DrawnFromDeck.HasValue)
                _state.Deck.AddToTop(record.DrawnFromDeck.Value);
            
            if (record.AddedToDeckPositions.Count > 0)
                _state.Deck.RemoveAtPositions(record.AddedToDeckPositions);
            else if (record.AddedToDeck.Count > 0)
                _state.Deck.RemoveFromBottom(record.AddedToDeck.Count);
            
            foreach (var slotIndex in record.RemovedSlots)
                _state.Board.RestoreSlot(slotIndex);

            foreach (var slotIndex in record.UnlockResolvedSlots)
                _state.Board.GetState(slotIndex).ResetUnlockResolved();
            
            EvaluateGameStatus();
            
            return GameMoveResult.Succeeded(GameMoveType.Undo, record);
        }
        
        private void EvaluateGameStatus()
        {
            if (!_state.Board.HasRemainingCards())
            {
                _state.SetStatus(GameStatus.Win);
                return;
            }

            if (!_state.Deck.CanDraw())
            {
                _state.SetStatus(GameStatus.OutOfDeck);
                return;
            }

            _state.SetStatus(GameStatus.Playing);
        }
        
        public bool CanUndo()
        {
            return _undo.CanUndo();
        }

        public MoveRecord PeekUndoRecord()
        {
            return _undo.Peek();
        }
    }
}
