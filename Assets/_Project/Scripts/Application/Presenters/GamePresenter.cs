using System;
using _Project.Scripts.Core.Game;
using _Project.Scripts.Core.Undo;

namespace _Project.Scripts.Application.Presenters
{
    public sealed class GamePresenter
    {
        private readonly GameController _controller;
        private readonly GameState _state;

        public GameState State => _state;
        
        public event Action<GameMoveResult> MovePerformed;
        public event Action<int> InvalidBoardCardSelected;
        public event Action StateChanged;

        public bool IsInputLocked { get; private set; }

        public GamePresenter(GameState state, GameController controller)
        {
            _state = state;
            _controller = controller;
        }

        public GameMoveResult StartGame()
        {
            var result = _controller.StartGame();

            if (!result.Success) return result;
            MovePerformed?.Invoke(result);
            StateChanged?.Invoke();

            return result;
        }

        public GameMoveResult PlayBoardSlot(int slotIndex)
        {
            if (IsInputLocked)
                return GameMoveResult.Failed(GameMoveType.PlayFromBoard);

            var result = _controller.TryPlayFromBoard(slotIndex);

            if (!result.Success)
            {
                if (IsSelectableBoardSlot(slotIndex))
                    InvalidBoardCardSelected?.Invoke(slotIndex);

                return result;
            }

            MovePerformed?.Invoke(result);

            return result;
        }

        public GameMoveResult DrawFromDeck()
        {
            if (IsInputLocked)
                return GameMoveResult.Failed(GameMoveType.DrawFromDeck);

            var result = _controller.TryDrawFromDeck();

            if (!result.Success) return result;
            MovePerformed?.Invoke(result);

            return result;
        }

        public GameMoveResult UseWildButton()
        {
            if (IsInputLocked)
                return GameMoveResult.Failed(GameMoveType.UseWildButton);

            var result = _controller.TryUseWildButton();

            if (!result.Success) return result;
            MovePerformed?.Invoke(result);

            return result;
        }

        public GameMoveResult Undo()
        {
            if (IsInputLocked)
                return GameMoveResult.Failed(GameMoveType.Undo);

            var result = _controller.Undo();

            if (!result.Success)
                return result;

            MovePerformed?.Invoke(result);

            return result;
        }
        
        public GameMoveResult CommitUndo()
        {
            var result = _controller.Undo();

            if (!result.Success)
                return result;

            StateChanged?.Invoke();

            return result;
        }
        
        public bool CanUndo => _controller.CanUndo();

        public MoveRecord PeekUndoRecord()
        {
            return _controller.PeekUndoRecord();
        }

        public void SetInputLocked(bool locked)
        {
            IsInputLocked = locked;
        }

        public void PublishStateChanged()
        {
            StateChanged?.Invoke();
        }

        private bool IsSelectableBoardSlot(int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < _state.Board.SlotCount &&
                   _state.Board.IsSelectable(slotIndex);
        }
        
    }
}
