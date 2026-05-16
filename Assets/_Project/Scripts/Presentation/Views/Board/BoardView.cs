using UnityEngine;
using _Project.Scripts.Application.LevelData;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;
using System;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private BoardCardView cardPrefab;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private BoardCardView[] cards;

        private GamePresenter _presenter;
        private BoardDefinition _boardDefinition;
        private Vector3[] _initialDealTargetPositions = Array.Empty<Vector3>();
        private Quaternion[] _initialDealTargetRotations = Array.Empty<Quaternion>();
        private Vector3 _initialDealStartPosition;
        private bool _initialDealRequested;
        private int _suppressSyncCount;

        public event Action Ready;
        public bool IsReady { get; private set; }
        public int CardCount => cards == null ? 0 : cards.Length;

        private void Start()
        {
            if (bootstrapper == null)
            {
                Debug.LogWarning($"{nameof(BoardView)} needs a GameBootstrapper reference.", this);
                return;
            }

            _presenter = bootstrapper.Presenter;
            if (_presenter == null)
            {
                Debug.LogWarning($"{nameof(BoardView)} could not find a running presenter on the bootstrapper.", this);
                return;
            }

            _boardDefinition = bootstrapper.BoardDefinition;

            BuildCardsIfNeeded();
            InitCards();

            _presenter.StateChanged += Sync;
            Sync();

            if (_initialDealRequested)
                PrepareInitialDeal();

            IsReady = true;
            Ready?.Invoke();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= Sync;
        }

        private void BuildCardsIfNeeded()
        {
            if (_boardDefinition == null || cardPrefab == null)
            {
                if (_boardDefinition == null)
                    Debug.LogWarning($"{nameof(BoardView)} has no BoardDefinition. Assign it on GameBootstrapper.", this);

                if (cardPrefab == null)
                    Debug.LogWarning($"{nameof(BoardView)} has no Card Prefab, so it cannot generate runtime board cards.", this);

                return;
            }

            if (cardsRoot == null)
                cardsRoot = transform;

            for (var i = cardsRoot.childCount - 1; i >= 0; i--)
                Destroy(cardsRoot.GetChild(i).gameObject);

            cards = new BoardCardView[_boardDefinition.Slots.Count];

            for (var i = 0; i < cards.Length; i++)
            {
                var card = Instantiate(cardPrefab, cardsRoot);
                card.name = $"BoardCard_{i:00}";
                card.transform.localPosition = _boardDefinition.GetLocalPosition(i);
                card.transform.localEulerAngles = _boardDefinition.GetLocalEulerAngles(i);
                card.SetSortingOrder(_boardDefinition.GetSortingOrder(i));
                cards[i] = card;
            }
        }

        private void InitCards()
        {
            if (cards == null)
                return;

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                cards[i].Init(i, _presenter);
                cards[i].SetSortingOrder(_boardDefinition.GetSortingOrder(i));
            }
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
            if (cards == null || cards.Length == 0)
                return;

            _initialDealTargetPositions = new Vector3[cards.Length];
            _initialDealTargetRotations = new Quaternion[cards.Length];

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                _initialDealTargetPositions[i] = cards[i].transform.position;
                _initialDealTargetRotations[i] = cards[i].transform.localRotation;
                cards[i].transform.position = _initialDealStartPosition;
                cards[i].SetClickEnabled(false);
                cards[i].gameObject.SetActive(true);
            }
        }

        public Vector3 GetInitialDealTargetWorldPosition(int slotIndex)
        {
            if (_initialDealTargetPositions == null) return GetCardWorldPosition(slotIndex);

            if (slotIndex < 0 || slotIndex >= _initialDealTargetPositions.Length)
                return GetCardWorldPosition(slotIndex);

            return _initialDealTargetPositions[slotIndex];
        }

        public Quaternion GetInitialDealTargetLocalRotation(int slotIndex)
        {
            if (_initialDealTargetRotations == null)
                return Quaternion.identity;

            if (slotIndex < 0 || slotIndex >= _initialDealTargetRotations.Length)
                return Quaternion.identity;

            return _initialDealTargetRotations[slotIndex];
        }

        public void CompleteInitialDeal()
        {
            if (cards == null)
                return;

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                if (_initialDealTargetPositions != null && i < _initialDealTargetPositions.Length)
                    cards[i].transform.position = _initialDealTargetPositions[i];

                if (_initialDealTargetRotations != null && i < _initialDealTargetRotations.Length)
                    cards[i].transform.localRotation = _initialDealTargetRotations[i];

                cards[i].SetClickEnabled(true);
            }
        }

        private void Sync()
        {
            if (_suppressSyncCount > 0)
                return;

            var state = _presenter.State;

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                if (i >= state.Board.SlotCount)
                {
                    cards[i].gameObject.SetActive(false);
                    continue;
                }

                var slot = state.Board.GetSlot(i);
                var selectable = state.Board.IsSelectable(i);
                var removed = state.Board.IsRemoved(i);

                cards[i].Sync(slot.Card, selectable, removed);
            }
        }

        public Vector3 GetCardWorldPosition(int slotIndex)
        {
            if (cards == null) return transform.position;
            
            if (slotIndex < 0 || slotIndex >= cards.Length) return transform.position;
            
            return cards[slotIndex] == null ? transform.position : cards[slotIndex].transform.position;
        }

        public Transform GetCardTransform(int slotIndex)
        {
            if (cards == null) return transform;
            
            if (slotIndex < 0 || slotIndex >= cards.Length) return transform;
            
            return cards[slotIndex] == null ? transform : cards[slotIndex].transform;
        }
        
        public void ShowBackAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= cards.Length) return;
            if (!cards[slotIndex]) return;

            cards[slotIndex].ShowBack();
        }

        public void ShowCardAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= cards.Length) return;
            if (!cards[slotIndex]) return;

            var state = _presenter.State;
            if (slotIndex >= state.Board.SlotCount) return;

            var slot = state.Board.GetSlot(slotIndex);
            cards[slotIndex].ShowCard(slot.Card, true);
        }

        public void HideAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= cards.Length) return;
            if (!cards[slotIndex]) return;

            cards[slotIndex].gameObject.SetActive(false);
        }

        public void SuppressSync()
        {
            _suppressSyncCount++;
        }

        public void ReleaseAndSync()
        {
            _suppressSyncCount = Mathf.Max(0, _suppressSyncCount - 1);

            if (_suppressSyncCount == 0)
                Sync();
        }
    }
}
