using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Presentation.Views.Undo
{
    public sealed class UndoButtonView : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Button button;
        

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;

            if (button == null)
                button = GetComponent<Button>();

            button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            if (!_presenter.CanUndo)
                return;

            var result = _presenter.Undo();
            Debug.Log(result.Success ? "Undo success" : "Cannot undo");
        }
    }
}