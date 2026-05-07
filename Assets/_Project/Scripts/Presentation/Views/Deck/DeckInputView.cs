using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Presentation.Views.Deck
{
    public sealed class DeckInputView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Collider deckCollider;

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

            var ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out var hit))
                return;

            if (hit.collider != deckCollider)
                return;

            var result = _presenter.DrawFromDeck();

            Debug.Log(result.Success ? "Draw from deck" : "Cannot draw from deck");
        }
    }
}