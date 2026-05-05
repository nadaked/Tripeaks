using System;

namespace _Project.Scripts.Core.Cards
{
    public sealed class RandomCardProvider : ICardProvider
    {
        private readonly Random _random;

        public RandomCardProvider(int seed)
        {
            _random = new Random(seed);
        }

        public CardData GetRandomNormalCard()
        {
            var rank = (CardRank)_random.Next(1, 14);
            var suit = (CardSuit)_random.Next(1, 5);

            return CardData.Normal(rank, suit);
        }
    }
}