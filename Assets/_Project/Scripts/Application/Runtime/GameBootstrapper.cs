using _Project.Scripts.Application.Builders;
using UnityEngine;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;

namespace _Project.Scripts.Application.Runtime
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private GamePresenter _presenter;
        public GamePresenter Presenter => _presenter;

        private void Awake()
        {
            var builder = new GameStateBuilder();

            var state = builder.BuildTestState();
            var provider = new RandomCardProvider(123);

            var controller = new GameController(state, provider);

            _presenter = new GamePresenter(state, controller);

            _presenter.StartGame();

            // test log
            Debug.Log("Game Started");
        }
    }
}