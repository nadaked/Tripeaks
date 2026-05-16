using _Project.Scripts.Application.Builders;
using _Project.Scripts.Application.LevelData;
using _Project.Scripts.Application.Presenters;
using _Project.Scripts.Core.Game;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Application.Runtime
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private BoardDefinition boardDefinition;
        [SerializeField] private int deckSeed = 12345;
        [SerializeField] private bool randomizeDeckSeedOnStart = true;
        [SerializeField] private bool reloadSceneOnWin = true;
        [SerializeField] private float reloadSceneDelay = 2f;

        private GamePresenter _presenter;
        private bool _reloadScheduled;
        public GamePresenter Presenter => _presenter;
        public BoardDefinition BoardDefinition => boardDefinition;

        private void Awake()
        {
            var builder = new GameStateBuilder();
            var runSeed = randomizeDeckSeedOnStart ? Environment.TickCount : deckSeed;
            var provider = boardDefinition == null
                ? new LevelCardProvider(runSeed)
                : new LevelCardProvider(runSeed, boardDefinition.DeckGenerationMode);

            var state = builder.BuildState(boardDefinition, provider);

            var controller = new GameController(state, provider);

            _presenter = new GamePresenter(state, controller);

            _presenter.StartGame();
            _presenter.StateChanged += OnStateChanged;

            Debug.Log("Game Started");
        }

        private void OnDestroy()
        {
            if (_presenter != null)
                _presenter.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged()
        {
            if (!reloadSceneOnWin || _reloadScheduled)
                return;

            if (_presenter.State.Status != GameStatus.Win)
                return;

            _reloadScheduled = true;
            StartCoroutine(ReloadSceneAfterDelay());
        }

        private IEnumerator ReloadSceneAfterDelay()
        {
            yield return new WaitForSeconds(reloadSceneDelay);

            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
