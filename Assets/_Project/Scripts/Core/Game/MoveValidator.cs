using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Game
{
    public sealed class MoveValidator
    {
        public bool CanPlay(CardData fromBoard, CardData waste)
        {
            if (!waste.IsValid)
                return true;

            if (fromBoard.IsWild || waste.IsWild)
                return true;
            
            if (fromBoard.IsDualRank)
                return IsAdjacent(fromBoard.Rank, waste.Rank) ||
                       IsAdjacent(fromBoard.SecondRank, waste.Rank);

            return IsAdjacent(fromBoard.Rank, waste.Rank);
        }

        private static bool IsAdjacent(CardRank fromBoard, CardRank waste)
        {
            var a = (int)fromBoard;
            var b = (int)waste;

            switch (a)
            {
                // A-K wrap (1 ↔ 13)
                case 1 when b == 13:
                case 13 when b == 1:
                    return true;
                default:
                    return System.Math.Abs(a - b) == 1;
            }
        }
    }
}
