namespace _Project.Scripts.Core.Cards
{
    [System.Serializable]
    public readonly struct CardData
    {
        public readonly CardRank Rank;
        public readonly CardSuit Suit;
        public readonly CardType Type;
        public readonly int Value;

        public bool IsValid => Type != CardType.Normal || Rank != CardRank.None;
        public bool IsWild => Type == CardType.Wild;
        public bool IsAddDeckCards => Type == CardType.AddDeckCards;

        public CardData(CardRank rank, CardSuit suit, CardType type = CardType.Normal, int value = 0)
        {
            Rank = rank;
            Suit = suit;
            Type = type;
            Value = value;
        }

        public static CardData Normal(CardRank rank, CardSuit suit)
        {
            return new CardData(rank, suit, CardType.Normal);
        }

        public static CardData Wild()
        {
            return new CardData(CardRank.None, CardSuit.None, CardType.Wild);
        }

        public static CardData AddDeckCards(int amount)
        {
            return new CardData(CardRank.None, CardSuit.None, CardType.AddDeckCards, amount);
        }
    }
}