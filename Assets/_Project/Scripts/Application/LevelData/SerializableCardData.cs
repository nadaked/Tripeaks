using System;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Application.LevelData
{
    [Serializable]
    public struct SerializableCardData
    {
        public CardType type;
        public CardRank rank;
        public CardRank secondRank;
        public CardSuit suit;
        public int value;

        public CardData ToCardData()
        {
            return type switch
            {
                CardType.Wild => CardData.Wild(),
                CardType.AddDeckCards => CardData.AddDeckCards(value),
                CardType.DualRank => CardData.DualRank(rank, secondRank, suit),
                _ => CardData.Normal(rank, suit)
            };
        }

        public static SerializableCardData FromCardData(CardData card)
        {
            return new SerializableCardData
            {
                type = card.Type,
                rank = card.Rank,
                secondRank = card.SecondRank,
                suit = card.Suit,
                value = card.Value
            };
        }
    }
}
