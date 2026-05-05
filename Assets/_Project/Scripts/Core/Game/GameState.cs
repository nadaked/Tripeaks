using _Project.Scripts.Core.Board;
using _Project.Scripts.Core.Deck;

namespace _Project.Scripts.Core.Game
{
    public sealed class GameState
    {
        public BoardState Board { get; }
        public DeckState Deck { get; }
        public WasteState Waste { get; }
        
        public GameStatus Status { get; private set; } = GameStatus.Playing;

        public GameState(BoardState board, DeckState deck, WasteState waste)
        {
            Board = board;
            Deck = deck;
            Waste = waste;
        }
        
        public void SetStatus(GameStatus status)
        {
            Status = status;
        }
    }
}