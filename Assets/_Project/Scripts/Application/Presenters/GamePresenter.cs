using System;
using _Project.Scripts.Core.Game;

namespace _Project.Scripts.Application.Presenters
{
    public sealed class GamePresenter
    {
        private readonly GameController _controller;
        private readonly GameState _state;

        public GameState State => _state;

        public event Action StateChanged;

        public GamePresenter(GameState state, GameController controller)
        {
            _state = state;
            _controller = controller;
        }

        public bool StartGame()
        {
            var success = _controller.StartGame();

            if (success)
                StateChanged?.Invoke();

            return success;
        }

        public bool PlayBoardSlot(int slotIndex)
        {
            var success = _controller.TryPlayFromBoard(slotIndex);

            if (success)
                StateChanged?.Invoke();

            return success;
        }

        public bool DrawFromDeck()
        {
            var success = _controller.TryDrawFromDeck();

            if (success)
                StateChanged?.Invoke();

            return success;
        }

        public bool Undo()
        {
            var success = _controller.Undo();

            if (success)
                StateChanged?.Invoke();

            return success;
        }
    }
}