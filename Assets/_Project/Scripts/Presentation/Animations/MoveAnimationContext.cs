using _Project.Scripts.Core.Game;

namespace _Project.Scripts.Presentation.Animations
{
    public readonly struct MoveAnimationContext
    {
        public readonly GameMoveResult Result;
        public readonly bool IsUndo;

        public MoveAnimationContext(GameMoveResult result, bool isUndo)
        {
            Result = result;
            IsUndo = isUndo;
        }
    }
}