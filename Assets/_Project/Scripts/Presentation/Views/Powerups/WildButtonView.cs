using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;
using _Project.Scripts.Presentation.Views.Card;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Presentation.Views.Powerups
{
    public sealed class WildButtonView : MonoBehaviour, IPointerDownHandler
    {
        [Header("Refs")]
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private CardView cardView;
        [SerializeField] private Collider2D clickCollider;

        [Header("Animation")]
        [SerializeField] private float shrinkDuration = 0.12f;
        [SerializeField] private float hiddenDelay = 0.2f;
        [SerializeField] private float growDuration = 0.16f;
        [SerializeField] private int sortingOrder = 80;

        private GamePresenter _presenter;
        private bool _isAnimating;
        private Vector3 _initialScale;

        public Transform FlySource => transform;

        private void Awake()
        {
            if (cardView == null)
                cardView = GetComponentInChildren<CardView>(true);

            if (clickCollider == null)
                clickCollider = GetComponentInChildren<Collider2D>(true);

            _initialScale = transform.localScale;
        }

        private void Start()
        {
            if (bootstrapper == null)
                bootstrapper = FindFirstObjectByType<GameBootstrapper>();

            _presenter = bootstrapper != null ? bootstrapper.Presenter : null;

            if (cardView != null)
            {
                cardView.ShowCard(CardData.Wild(), true);
                cardView.SetSortingOrder(sortingOrder);
                cardView.SetClickEnabled(false);
            }

            RefreshInteractable();
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TryUse();
        }

        private void OnMouseDown()
        {
            TryUse();
        }

        private void TryUse()
        {
            if (_isAnimating || _presenter == null || _presenter.IsInputLocked)
                return;

            var result = _presenter.UseWildButton();
            if (!result.Success)
                return;

            PlayButtonCycle();
        }

        private void PlayButtonCycle()
        {
            _isAnimating = true;
            RefreshInteractable();

            transform.DOKill();
            transform.localScale = _initialScale;

            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack))
                .AppendInterval(hiddenDelay)
                .Append(transform.DOScale(_initialScale, growDuration).SetEase(Ease.OutBack))
                .OnComplete(() =>
                {
                    _isAnimating = false;
                    transform.localScale = _initialScale;
                    RefreshInteractable();
                });
        }

        private void RefreshInteractable()
        {
            if (clickCollider != null)
                clickCollider.enabled = !_isAnimating;
        }
    }
}
