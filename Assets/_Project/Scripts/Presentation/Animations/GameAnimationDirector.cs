using System;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Application.Runtime;
using _Project.Scripts.Core.Game;
using UnityEngine;

namespace _Project.Scripts.Presentation.Animations
{
    public class GameAnimationDirector : MonoBehaviour
    {
        [SerializeField] private GameBootstrapper bootstrapper;

        private GamePresenter _presenter;

        private void Start()
        {
            _presenter = bootstrapper.Presenter;
            _presenter.MovePerformed += OnMovePerformed;
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.MovePerformed -= OnMovePerformed;
        }

        private void OnMovePerformed(GameMoveResult result)
        {
            if (!result.Success) return;

            var isUndo = result.MoveType == GameMoveType.Undo;
            var context = new MoveAnimationContext(result, isUndo);

            Play(context);
        }

        private void Play(MoveAnimationContext context)
        {
            switch (context.Result.MoveType)
            {
                case GameMoveType.StartGame:
                    Debug.Log("Animation: Start Game / Deck to waste");
                    break;
                case GameMoveType.PlayFromBoard:
                    Debug.Log("Animation: Play from Board");
                    break;
                case GameMoveType.DrawFromDeck:
                    Debug.Log("Animation: Draw from Deck");
                    break;
                case GameMoveType.Undo:
                    Debug.Log("Animation: Undo");
                    break;
                case GameMoveType.None:
                default:
                    Debug.Log("Animation: Unknown Move");
                    break;
            }
        }
    }
}