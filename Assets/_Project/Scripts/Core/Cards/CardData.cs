namespace _Project.Scripts.Core.Cards
{
    [System.Serializable]
    public readonly struct CardData
    {
        public readonly CardRank Rank;
        public readonly CardRank SecondRank;
        public readonly CardSuit Suit;
        public readonly CardType Type;
        public readonly int Value;

        public bool IsValid => Type != CardType.Normal || Rank != CardRank.None;
        public bool IsWild => Type == CardType.Wild;
        public bool IsAddDeckCards => Type == CardType.AddDeckCards;
        public bool IsDualRank => Type == CardType.DualRank;

        public CardData(
            CardRank rank,
            CardSuit suit,
            CardType type = CardType.Normal,
            int value = 0,
            CardRank secondRank = CardRank.None
        )
        {
            Rank = rank;
            Suit = suit;
            Type = type;
            Value = value;
            SecondRank = secondRank;
        }

        public static CardData Normal(CardRank rank, CardSuit suit)
        {
            return new CardData(rank, suit, CardType.Normal);
        }

        public static CardData DualRank(CardRank firstRank, CardRank secondRank, CardSuit suit = CardSuit.None)
        {
            return new CardData(firstRank, suit, CardType.DualRank, 0, secondRank);
        }

        public static CardData Wild()
        {
            return new CardData(CardRank.None, CardSuit.None, CardType.Wild);
        }

        public static CardData AddDeckCards(int amount)
        {
            return new CardData(CardRank.None, CardSuit.None, CardType.AddDeckCards, amount);
        }

        public override string ToString()
        {
            return Type switch
            {
                CardType.Normal => $"{Rank} of {Suit}",
                CardType.DualRank => $"{Rank}/{SecondRank} of {Suit}",
                CardType.Wild => "Wild",
                CardType.AddDeckCards => $"+{Value} Deck",
                _ => "Invalid"
            };
        }
    }
}
