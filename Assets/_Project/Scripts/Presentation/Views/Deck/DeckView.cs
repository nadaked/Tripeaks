using System;
using System.Collections.Generic;
using System.Linq;
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
        private Vector3[] _initialDealTargetPositions = Array.Empty<Vector3>();
        private Quaternion[] _initialDealTargetRotations = Array.Empty<Quaternion>();
        private Vector3 _initialDealStartPosition;
        private bool _initialDealRequested;

        public bool IsReady { get; private set; }
        public int VisibleCardCount { get; private set; }

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            CreateStack();

            _presenter.StateChanged += Sync;
            Sync();

            if (_initialDealRequested)
                PrepareInitialDeal();

            IsReady = true;
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= Sync;

            foreach (var card in _cards.Where(card => card != null))
            {
                card.Clicked -= OnDeckClicked;
            }
        }

        private void CreateStack()
        {
            for (var i = 0; i < maxVisibleCards; i++)
            {
                var card = Instantiate(cardPrefab, stackRoot);
                card.ShowBack();
                card.SetSortingOrder(20 + i);
                card.Clicked += OnDeckClicked;
                _cards.Add(card);
            }
        }

        private void Sync()
        {
            var deckCount = _presenter.State.Deck.Count;
            var visibleCount = Mathf.Min(deckCount, maxVisibleCards);
            VisibleCardCount = visibleCount;

            gameObject.SetActive(deckCount > 0);

            for (var i = 0; i < _cards.Count; i++)
            {
                var active = i < visibleCount;
                _cards[i].gameObject.SetActive(active);
                _cards[i].SetSortingOrder(20 + i);

                if (!active)
                    continue;

                _cards[i].ShowBack();

                var reverseIndex = visibleCount - 1 - i;
                
                var isTopClickable = i == visibleCount - 1;
                _cards[i].SetClickEnabled(isTopClickable);
                
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

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public void RequestInitialDeal(Vector3 startWorldPosition)
        {
            _initialDealRequested = true;
            _initialDealStartPosition = startWorldPosition;

            if (IsReady)
                PrepareInitialDeal();
        }

        private void PrepareInitialDeal()
        {
            if (_cards.Count == 0 || VisibleCardCount <= 0)
                return;

            _initialDealTargetPositions = new Vector3[VisibleCardCount];
            _initialDealTargetRotations = new Quaternion[VisibleCardCount];

            for (var i = 0; i < VisibleCardCount; i++)
            {
                var card = _cards[i];
                if (card == null)
                    continue;

                _initialDealTargetPositions[i] = card.transform.position;
                _initialDealTargetRotations[i] = card.transform.localRotation;
                card.transform.position = _initialDealStartPosition;
                card.SetClickEnabled(false);
                card.gameObject.SetActive(true);
            }
        }

        public Transform GetCardTransform(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cards.Count)
                return transform;

            return _cards[cardIndex] == null ? transform : _cards[cardIndex].transform;
        }

        public Vector3 GetInitialDealTargetWorldPosition(int cardIndex)
        {
            if (_initialDealTargetPositions == null)
                return GetCardTransform(cardIndex).position;

            if (cardIndex < 0 || cardIndex >= _initialDealTargetPositions.Length)
                return GetCardTransform(cardIndex).position;

            return _initialDealTargetPositions[cardIndex];
        }

        public Vector3 GetInitialDealStackWorldPosition()
        {
            if (VisibleCardCount <= 0)
                return GetWorldPosition();

            return GetInitialDealTargetWorldPosition(0);
        }

        public Quaternion GetInitialDealTargetLocalRotation(int cardIndex)
        {
            if (_initialDealTargetRotations == null)
                return Quaternion.identity;

            if (cardIndex < 0 || cardIndex >= _initialDealTargetRotations.Length)
                return Quaternion.identity;

            return _initialDealTargetRotations[cardIndex];
        }

        public void CompleteInitialDeal()
        {
            for (var i = 0; i < VisibleCardCount; i++)
            {
                var card = _cards[i];
                if (card == null)
                    continue;

                if (_initialDealTargetPositions != null && i < _initialDealTargetPositions.Length)
                    card.transform.position = _initialDealTargetPositions[i];

                if (_initialDealTargetRotations != null && i < _initialDealTargetRotations.Length)
                    card.transform.localRotation = _initialDealTargetRotations[i];
            }

            Sync();
        }
        
        private void OnDeckClicked()
        {
            _presenter.DrawFromDeck();
        }
    }
}
