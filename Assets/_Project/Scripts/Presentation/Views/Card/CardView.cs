using TMPro;
using UnityEngine;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Presentation.Views.Card
{
    public sealed class CardView : MonoBehaviour
    {
        [Header("Faces")]
        [SerializeField] private GameObject front;
        [SerializeField] private GameObject back;
        [SerializeField] private GameObject special;

        [Header("Texts")]
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text suitText;
        [SerializeField] private TMP_Text specialText;

        public void ShowNormal(CardData card, bool faceUp)
        {
            front.SetActive(faceUp);
            back.SetActive(!faceUp);
            special.SetActive(false);

            if (!faceUp)
                return;

            rankText.text = GetRankText(card.Rank);
            suitText.text = GetSuitText(card.Suit);
        }

        public void ShowSpecial(CardData card)
        {
            front.SetActive(false);
            back.SetActive(false);
            special.SetActive(true);

            if (card.IsWild)
                specialText.text = "WILD";
            else if (card.IsAddDeckCards)
                specialText.text = $"+{card.Value}";
            else
                specialText.text = "?";
        }

        private string GetRankText(CardRank rank)
        {
            return rank switch
            {
                CardRank.Ace => "A",
                CardRank.Jack => "J",
                CardRank.Queen => "Q",
                CardRank.King => "K",
                CardRank.Ten => "10",
                _ => ((int)rank).ToString()
            };
        }

        private string GetSuitText(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Clubs => "♣",
                CardSuit.Diamonds => "♦",
                CardSuit.Hearts => "♥",
                CardSuit.Spades => "♠",
                _ => ""
            };
        }
    }
}