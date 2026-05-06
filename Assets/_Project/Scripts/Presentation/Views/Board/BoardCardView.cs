using UnityEngine;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardCardView : MonoBehaviour
    {
        [SerializeField] private CardView cardView;
        [SerializeField] private Renderer selectableRenderer;

        public int SlotIndex { get; private set; }

        public void Init(int slotIndex)
        {
            SlotIndex = slotIndex;

            if (cardView == null)
                cardView = GetComponentInChildren<CardView>();

            if (selectableRenderer == null)
                selectableRenderer = GetComponentInChildren<Renderer>();
        }

        public void Sync(CardData card, bool selectable, bool removed)
        {
            gameObject.SetActive(!removed);

            if (removed)
                return;

            if (card.IsWild || card.IsAddDeckCards)
            {
                cardView.ShowSpecial(card);
            }
            else
            {
                cardView.ShowNormal(card, selectable);
            }

            if (selectableRenderer != null)
                selectableRenderer.material.color = selectable ? Color.white : Color.gray;
        }
    }
}