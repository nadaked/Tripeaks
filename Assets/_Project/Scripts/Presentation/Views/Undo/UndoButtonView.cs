using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Presentation.Views.Undo
{
    public sealed class UndoButtonView : MonoBehaviour, IPointerDownHandler
    {
        [Header("Refs")]
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D clickCollider;

        [Header("State")]
        [SerializeField] private bool hideVisualWhenUnavailable = true;
        [SerializeField, Range(0f, 1f)] private float availableAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float unavailableAlpha = 0f;

        private GamePresenter _presenter;
        private bool _isAvailable;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (image == null)
                image = GetComponent<Image>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

            if (clickCollider == null)
                clickCollider = GetComponentInChildren<Collider2D>(true);
        }

        private void Start()
        {
            if (bootstrapper == null)
                bootstrapper = FindFirstObjectByType<GameBootstrapper>();

            _presenter = bootstrapper != null ? bootstrapper.Presenter : null;

            if (button != null)
                button.onClick.AddListener(OnClicked);

            if (_presenter != null)
            {
                _presenter.MovePerformed += OnMovePerformed;
                _presenter.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClicked);

            if (_presenter != null)
            {
                _presenter.MovePerformed -= OnMovePerformed;
                _presenter.StateChanged -= Refresh;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null)
                return;

            OnClicked();
        }

        private void OnMouseDown()
        {
            if (button != null)
                return;

            OnClicked();
        }

        private void OnMovePerformed(GameMoveResult _)
        {
            Refresh();
        }

        private void OnClicked()
        {
            if (!_isAvailable || _presenter == null || !_presenter.CanUndo)
                return;

            var result = _presenter.Undo();
            if (result.Success)
                Refresh();
        }

        private void Refresh()
        {
            _isAvailable = _presenter != null && _presenter.CanUndo && !_presenter.IsInputLocked;

            if (button != null)
                button.interactable = _isAvailable;

            if (clickCollider != null)
                clickCollider.enabled = _isAvailable;

            ApplyImageState();
            ApplySpriteState();
        }

        private void ApplyImageState()
        {
            if (image == null)
                return;

            image.enabled = !hideVisualWhenUnavailable || _isAvailable;

            var color = image.color;
            color.a = _isAvailable ? availableAlpha : unavailableAlpha;
            image.color = color;
        }

        private void ApplySpriteState()
        {
            if (spriteRenderer == null)
                return;

            spriteRenderer.enabled = !hideVisualWhenUnavailable || _isAvailable;

            var color = spriteRenderer.color;
            color.a = _isAvailable ? availableAlpha : unavailableAlpha;
            spriteRenderer.color = color;
        }
    }
}
