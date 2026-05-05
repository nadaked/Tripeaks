using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Application.Presenters;

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

            if (!hit.collider.TryGetComponent(out SlotClickView slot))
                return;

            var success = _presenter.PlayBoardSlot(slot.SlotIndex);

            Debug.Log(success
                ? $"Played slot {slot.SlotIndex}"
                : $"Cannot play slot {slot.SlotIndex}");
        }
    }
}