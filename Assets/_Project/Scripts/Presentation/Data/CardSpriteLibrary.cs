using _Project.Scripts.Core.Cards;
using UnityEngine;

namespace _Project.Scripts.Presentation.Data
{
    [CreateAssetMenu(fileName = "CardSpriteLibrary", menuName = "Card Sprite Library")]
    public sealed class CardSpriteLibrary : ScriptableObject
    {
        [Header("Diamond Sprites")]
        [SerializeField] private Sprite[] diamondSprites;
        [SerializeField] private Sprite[] diamondDualSprites;

        [Header("Clubs Sprites")]
        [SerializeField] private Sprite[] clubsSprites;
        [SerializeField] private Sprite[] clubsDualSprites;

        [Header("Heart Sprites")]
        [SerializeField] private Sprite[] heartSprites;
        [SerializeField] private Sprite[] heartDualSprites;

        [Header("Spade Sprites")]
        [SerializeField] private Sprite[] spadeSprites;
        [SerializeField] private Sprite[] spadeDualSprites;

        [Header("Other Sprites")]
        [SerializeField] private Sprite backSprite;
        [SerializeField] private Sprite wildSprite;
        [SerializeField] private Sprite specialSprite;

        public Sprite BackSprite => backSprite;
        public Sprite WildSprite => wildSprite;
        public Sprite SpecialSprite => specialSprite;

        public Sprite GetCardSprite(CardData card)
        {
            return card.Type switch
            {
                CardType.Normal => GetNormalSprite(card),
                CardType.DualRank => GetDualSprite(card),
                CardType.Wild => wildSprite,
                CardType.AddDeckCards => specialSprite,
                _ => null
            };
        }

        private Sprite GetNormalSprite(CardData card)
        {
            var array = GetNormalArray(card.Suit);

            return GetSpriteFromArray(array, card.Rank);
        }

        private Sprite GetDualSprite(CardData card)
        {
            var array = GetDualArray(card.Suit);

            return GetSpriteFromArray(array, card.Rank);
        }

        private Sprite[] GetNormalArray(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Diamonds => diamondSprites,
                CardSuit.Clubs => clubsSprites,
                CardSuit.Hearts => heartSprites,
                CardSuit.Spades => spadeSprites,
                _ => null
            };
        }

        private Sprite[] GetDualArray(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Diamonds => diamondDualSprites,
                CardSuit.Clubs => clubsDualSprites,
                CardSuit.Hearts => heartDualSprites,
                CardSuit.Spades => spadeDualSprites,
                _ => null
            };
        }

        private static Sprite GetSpriteFromArray(Sprite[] array, CardRank rank)
        {
            if (array == null)
                return null;

            var index = GetRankIndex(rank);

            if (index < 0 || index >= array.Length)
                return null;

            return array[index];
        }

        private static int GetRankIndex(CardRank rank)
        {
            return rank switch
            {
                CardRank.Ace => 0,
                CardRank.Two => 1,
                CardRank.Three => 2,
                CardRank.Four => 3,
                CardRank.Five => 4,
                CardRank.Six => 5,
                CardRank.Seven => 6,
                CardRank.Eight => 7,
                CardRank.Nine => 8,
                CardRank.Ten => 9,
                CardRank.Jack => 10,
                CardRank.Queen => 11,
                CardRank.King => 12,
                _ => -1
            };
        }
    }
}