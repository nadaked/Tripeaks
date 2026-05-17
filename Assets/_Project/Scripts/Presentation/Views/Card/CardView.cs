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
        [SerializeField] private TMP_Text rankTopText;
        [SerializeField] private TMP_Text rankBottomText;
        [SerializeField] private TMP_Text specialText;

        [Header("Generated Text Layout")]
        [SerializeField] private Vector3 rankTopLocalPosition = new(-3.35f, 5.25f, -0.5f);
        [SerializeField] private Vector3 rankBottomLocalPosition = new(3.35f, -5.25f, -0.5f);
        [SerializeField] private Vector3 specialLocalPosition = new(0f, 0f, -0.5f);

        [Header("Data")]
        [SerializeField] private CardSpriteLibrary spriteLibrary;

        private bool _clickEnabled = true;
        private int _sortingOrder;
        private int _sortingLayerId;

        public void ShowBack()
        {
            EnsureSpriteRenderer();

            if (spriteRenderer != null && spriteLibrary != null)
                spriteRenderer.sprite = spriteLibrary.BackSprite;

            HideAllTexts();
        }

        public void ShowCard(CardData card, bool faceUp)
        {
            if (!faceUp)
            {
                if (card.IsWild)
                {
                    ShowCard(card, true);
                    return;
                }

                ShowBack();
                return;
            }

            EnsureSpriteRenderer();

            if (spriteRenderer != null && spriteLibrary != null)
                spriteRenderer.sprite = spriteLibrary.GetCardSprite(card);

            HideAllTexts();

            if (spriteLibrary == null)
                return;

            if (card.IsAddDeckCards)
            {
                ShowSpecialText($"x{card.Value}");
                return;
            }

            if (card.IsWild)
                return;

            if (card.Type is CardType.Normal or CardType.DualRank)
                ShowRankTexts(card);
        }

        public void SetSpriteLibrary(CardSpriteLibrary library)
        {
            spriteLibrary = library;
        }

        public void SetClickEnabled(bool clickEnabled)
        {
            _clickEnabled = clickEnabled;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_clickEnabled)
                return;

            Clicked?.Invoke();
        }

        public void SetSortingOrder(int order)
        {
            _sortingOrder = order;

            EnsureSpriteRenderer();

            if (spriteRenderer != null)
            {
                _sortingLayerId = spriteRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = order;
                spriteRenderer.rendererPriority = 0;
            }

            SetTextSorting(rankTopText, _sortingLayerId, order + 1);
            SetTextSorting(rankBottomText, _sortingLayerId, order + 1);
            SetTextSorting(specialText, _sortingLayerId, order + 1);
        }

        public int GetSortingOrder()
        {
            EnsureSpriteRenderer();

            return spriteRenderer == null ? _sortingOrder : spriteRenderer.sortingOrder;
        }

        private void ShowRankTexts(CardData card)
        {
            HideSpecialText();
            EnsureRankText(ref rankTopText, "Rank Top Text (TMP)", rankTopLocalPosition, TextAlignmentOptions.Center);
            EnsureRankText(ref rankBottomText, "Rank Bottom Text (TMP)", rankBottomLocalPosition, TextAlignmentOptions.Center);
            PlaceText(rankTopText, rankTopLocalPosition, TextAlignmentOptions.Center);
            PlaceText(rankBottomText, rankBottomLocalPosition, TextAlignmentOptions.Center);

            var font = spriteLibrary.GetRankFont(card.Suit);
            var color = spriteLibrary.GetRankColor(card.Suit);

            ConfigureRankText(rankTopText, CardSpriteLibrary.GetPrimaryRankText(card), font, color);
            ConfigureRankText(rankBottomText, CardSpriteLibrary.GetSecondaryRankText(card), font, color);
        }

        private void ShowSpecialText(string text)
        {
            HideRankTexts();
            EnsureSpecialText();
            PlaceText(specialText, specialLocalPosition, TextAlignmentOptions.Center);

            if (specialText == null)
                return;

            specialText.gameObject.SetActive(true);
            specialText.text = text;

            var font = spriteLibrary.GetSpecialFont();
            if (font != null)
                specialText.font = font;

            SetTextSorting(specialText, _sortingLayerId, _sortingOrder + 1);
        }

        private void ConfigureRankText(TMP_Text text, string value, TMP_FontAsset font, Color color)
        {
            if (text == null)
                return;

            var hasValue = !string.IsNullOrWhiteSpace(value);
            text.gameObject.SetActive(hasValue);
            text.text = hasValue ? value : string.Empty;
            text.color = color;

            if (font != null)
                text.font = font;

            SetTextSorting(text, _sortingLayerId, _sortingOrder + 1);
        }

        private void HideRankTexts()
        {
            if (rankTopText != null)
                rankTopText.gameObject.SetActive(false);

            if (rankBottomText != null)
                rankBottomText.gameObject.SetActive(false);
        }

        private void HideSpecialText()
        {
            if (specialText != null)
                specialText.gameObject.SetActive(false);
        }

        private void HideAllTexts()
        {
            HideRankTexts();
            HideSpecialText();
        }

        private void EnsureSpriteRenderer()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void EnsureRankText(
            ref TMP_Text target,
            string objectName,
            Vector3 localPosition,
            TextAlignmentOptions alignment
        )
        {
            if (target == null)
                target = CreateText(objectName, localPosition, alignment);

            PlaceText(target, localPosition, alignment);
        }

        private void EnsureSpecialText()
        {
            if (specialText == null)
                specialText = CreateText("Special Text (TMP)", specialLocalPosition, TextAlignmentOptions.Center);
        }

        private TMP_Text CreateText(
            string objectName,
            Vector3 localPosition,
            TextAlignmentOptions alignment
        )
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(transform, false);
            textObject.transform.localPosition = localPosition;

            var text = textObject.AddComponent<TextMeshPro>();
            text.alignment = alignment;

            SetTextSorting(text, _sortingLayerId, _sortingOrder + 1);

            return text;
        }

        private static void PlaceText(TMP_Text text, Vector3 localPosition, TextAlignmentOptions alignment)
        {
            if (text == null)
                return;

            text.alignment = alignment;

            if (text.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = new Vector2(localPosition.x, localPosition.y);
                var position = rectTransform.localPosition;
                position.z = localPosition.z;
                rectTransform.localPosition = position;
                return;
            }

            text.transform.localPosition = localPosition;
        }

        private static void SetTextSorting(TMP_Text text, int sortingLayerId, int sortingOrder)
        {
            if (text != null && text.TryGetComponent<Renderer>(out var textRenderer))
            {
                textRenderer.sortingLayerID = sortingLayerId;
                textRenderer.sortingOrder = sortingOrder;
                textRenderer.rendererPriority = 1;
            }
        }
    }
}
