using _Project.Scripts.Core.Cards;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Presentation.Data
{
    [CreateAssetMenu(fileName = "CardSpriteLibrary", menuName = "Card Sprite Library")]
    public sealed class CardSpriteLibrary : ScriptableObject
    {
        [Header("Fixed Face Sprites")]
        [SerializeField] private Sprite clubsFaceSprite;
        [SerializeField] private Sprite diamondFaceSprite;
        [SerializeField] private Sprite heartFaceSprite;
        [SerializeField] private Sprite spadeFaceSprite;

        [Header("Other Sprites")]
        [SerializeField] private Sprite backSprite;
        [SerializeField] private Sprite wildSprite;
        [SerializeField] private Sprite specialSprite;

        [Header("Rank Fonts")]
        [SerializeField] private TMP_FontAsset clubsFont;
        [SerializeField] private TMP_FontAsset diamondFont;
        [SerializeField] private TMP_FontAsset heartFont;
        [SerializeField] private TMP_FontAsset spadeFont;

        [Header("Rank Colors")]
        [SerializeField] private Color clubsRankColor = Color.white;
        [SerializeField] private Color diamondRankColor = Color.white;
        [SerializeField] private Color heartRankColor = Color.white;
        [SerializeField] private Color spadeRankColor = Color.white;

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

        public TMP_FontAsset GetRankFont(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Clubs => clubsFont,
                CardSuit.Diamonds => diamondFont,
                CardSuit.Hearts => heartFont,
                CardSuit.Spades => spadeFont,
                _ => null
            };
        }

        public Color GetRankColor(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Clubs => clubsRankColor,
                CardSuit.Diamonds => diamondRankColor,
                CardSuit.Hearts => heartRankColor,
                CardSuit.Spades => spadeRankColor,
                _ => Color.white
            };
        }

        public TMP_FontAsset GetSpecialFont()
        {
            return spadeFont != null ? spadeFont : clubsFont;
        }

        public static string GetPrimaryRankText(CardData card)
        {
            return GetRankText(card.Rank);
        }

        public static string GetSecondaryRankText(CardData card)
        {
            if (card.IsDualRank && card.SecondRank != CardRank.None)
                return GetRankText(card.SecondRank);

            return GetRankText(card.Rank);
        }

        public static string GetRankText(CardRank rank)
        {
            return rank switch
            {
                CardRank.Ace => "A",
                CardRank.Two => "2",
                CardRank.Three => "3",
                CardRank.Four => "4",
                CardRank.Five => "5",
                CardRank.Six => "6",
                CardRank.Seven => "7",
                CardRank.Eight => "8",
                CardRank.Nine => "9",
                CardRank.Ten => "10",
                CardRank.Jack => "J",
                CardRank.Queen => "Q",
                CardRank.King => "K",
                _ => string.Empty
            };
        }

        private Sprite GetNormalSprite(CardData card)
        {
            return GetFaceSprite(card.Suit);
        }

        private Sprite GetDualSprite(CardData card)
        {
            return GetFaceSprite(card.Suit);
        }

        private Sprite GetFaceSprite(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Clubs => clubsFaceSprite,
                CardSuit.Diamonds => diamondFaceSprite,
                CardSuit.Hearts => heartFaceSprite,
                CardSuit.Spades => spadeFaceSprite,
                _ => null
            };
        }

    }
}
