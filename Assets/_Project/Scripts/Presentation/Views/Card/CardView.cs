using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Data;

namespace _Project.Scripts.Presentation.Views.Card
{
    public sealed class CardView : MonoBehaviour, IPointerDownHandler
    {
        public event Action Clicked;

        [Header("Refs")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text specialText;

        [Header("Data")]
        [SerializeField] private CardSpriteLibrary spriteLibrary;
        
        public void ShowBack()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = spriteLibrary.BackSprite;

            if (specialText != null)
                specialText.gameObject.SetActive(false);
        }

        public void ShowCard(CardData card, bool faceUp)
        {

            if (!faceUp)
            {
                ShowBack();
                return;
            }

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = spriteLibrary.GetCardSprite(card);

            if (specialText != null)
            {
                var isSpecial = card.IsAddDeckCards;
                specialText.gameObject.SetActive(isSpecial);
                
                if (isSpecial)
                    specialText.text = $"+{card.Value}";
            }
        }

        public void SetSpriteLibrary(CardSpriteLibrary library)
        {
            spriteLibrary = library;
        }
        
        private bool _clickEnabled = true;

        public void SetClickEnabled(bool clickEnabled)
        {
            _clickEnabled = clickEnabled;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            if (!_clickEnabled)
                return;

            Debug.Log("OnPointerDown after if");
            Clicked?.Invoke();
        }
        
        public void SetSortingOrder(int order)
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = order;
        }
    }
}