using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Game
{
    public sealed class MoveValidator
    {
        public bool CanPlay(CardData fromBoard, CardData waste)
        {
            if (!waste.IsValid)
                return true;

            if (fromBoard.IsWild)
                return true;

            var a = (int)fromBoard.Rank;
            var b = (int)waste.Rank;

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