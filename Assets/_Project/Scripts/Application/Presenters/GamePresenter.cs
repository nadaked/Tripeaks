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
        public event Action StateChanged;

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
            var result = _controller.TryPlayFromBoard(slotIndex);

            if (!result.Success) return result;
            MovePerformed?.Invoke(result);
            StateChanged?.Invoke();

            return result;
        }

        public GameMoveResult DrawFromDeck()
        {
            var result = _controller.TryDrawFromDeck();

            if (!result.Success) return result;
            MovePerformed?.Invoke(result);
            StateChanged?.Invoke();

            return result;
        }

        public GameMoveResult Undo()
        {
            if (!_controller.CanUndo())
                return GameMoveResult.Failed(GameMoveType.Undo);

            var record = _controller.PeekUndoRecord();
            var visualResult = GameMoveResult.Succeeded(GameMoveType.Undo, record);

            MovePerformed?.Invoke(visualResult);

            return visualResult;
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
        
    }
}