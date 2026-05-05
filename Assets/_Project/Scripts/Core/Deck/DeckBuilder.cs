using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Deck
{
    public sealed class DeckBuilder
    {
        private readonly List<CardData> _cards = new();

        public DeckBuilder AddStandardDeck()
        {
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                if (suit == CardSuit.None)
                    continue;

                foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                {
                    if (rank == CardRank.None)
                        continue;

                    _cards.Add(CardData.Normal(rank, suit));
                }
            }

            return this;
        }

        public DeckBuilder AddWild(int amount)
        {
            for (var i = 0; i < amount; i++)
                _cards.Add(CardData.Wild());

            return this;
        }

        public DeckBuilder AddDeckCardsBonus(int amount, int bonusValue)
        {
            for (var i = 0; i < amount; i++)
                _cards.Add(CardData.AddDeckCards(bonusValue));

            return this;
        }

        public DeckBuilder Shuffle(int seed)
        {
            var random = new Random(seed);

            for (var i = _cards.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }

            return this;
        }

        public DeckState Build()
        {
            return new DeckState(_cards);
        }

        public CardData[] ToArray()
        {
            return _cards.ToArray();
        }
    }
}