using System.Collections.Generic;
using TMPro;
using UnityEngine;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Deck
{
    public sealed class DeckView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform stackRoot;
        [SerializeField] private TMP_Text countText;

        [Header("Stack Settings")]
        [SerializeField] private int maxVisibleCards = 12;
        [SerializeField] private float xOffset = 0.08f;
        [SerializeField] private float zOffset = -0.01f;

        private readonly List<CardView> _cards = new();

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            CreateStack();

            _presenter.StateChanged += Sync;
            Sync();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= Sync;
        }

        private void CreateStack()
        {
            for (var i = 0; i < maxVisibleCards; i++)
            {
                var card = Instantiate(cardPrefab, stackRoot);
                card.ShowBack();

                _cards.Add(card);
            }
        }

        private void Sync()
        {
            var deckCount = _presenter.State.Deck.Count;
            var visibleCount = Mathf.Min(deckCount, maxVisibleCards);

            gameObject.SetActive(deckCount > 0);

            for (var i = 0; i < _cards.Count; i++)
            {
                var active = i < visibleCount;
                _cards[i].gameObject.SetActive(active);

                if (!active)
                    continue;

                _cards[i].ShowBack();

                var reverseIndex = visibleCount - 1 - i;

                _cards[i].transform.localPosition = new Vector3(
                    -reverseIndex * xOffset,
                    0f,
                    i * zOffset
                );

                _cards[i].transform.localRotation = Quaternion.identity;
            }

            if (countText != null)
                countText.text = deckCount.ToString();
        }
    }
}