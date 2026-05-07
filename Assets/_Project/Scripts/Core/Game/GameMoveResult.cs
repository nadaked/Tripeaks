using _Project.Scripts.Core.Undo;

namespace _Project.Scripts.Core.Game
{
    public readonly struct GameMoveResult
    {
        public readonly bool Success;
        public readonly GameMoveType MoveType;
        public readonly MoveRecord Record;

        public GameMoveResult(bool success, GameMoveType moveType, MoveRecord record)
        {
            Success = success;
            MoveType = moveType;
            Record = record;
        }

        public static GameMoveResult Failed(GameMoveType moveType)
        {
            return new GameMoveResult(false, moveType, null);
        }

        public static GameMoveResult Succeeded(GameMoveType moveType, MoveRecord record)
        {
            return new GameMoveResult(true, moveType, record);
        }

    }
}