using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Presentation.Views.Card;

namespace _Project.Scripts.Presentation.Views.Board
{
    public sealed class BoardInputView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Camera targetCamera;

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            var screenPosition = Mouse.current.position.ReadValue();
            var ray = targetCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray, out var hit))
                return;

            var card = hit.collider.GetComponentInParent<BoardCardView>();
            if (!card)
                return;

            var success = _presenter.PlayBoardSlot(card.SlotIndex);

            Debug.Log(success
                ? $"Played slot {card.SlotIndex}"
                : $"Cannot play slot {card.SlotIndex}");
        }
    }
}