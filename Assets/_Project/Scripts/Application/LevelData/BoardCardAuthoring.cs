using System;
using _Project.Scripts.Presentation.Views.Board;
using UnityEngine;

namespace _Project.Scripts.Application.LevelData
{
    [DisallowMultipleComponent]
    public sealed class BoardCardAuthoring : MonoBehaviour
    {
        [SerializeField] private BoardCardView boardCardView;
        [SerializeField] private SerializableCardData card;
        [SerializeField] private BoardCardAuthoring[] blockers = Array.Empty<BoardCardAuthoring>();

        public BoardCardView BoardCardView => boardCardView;
        public SerializableCardData Card
        {
            get => card;
            set => card = value;
        }

        public BoardCardAuthoring[] Blockers
        {
            get => blockers;
            set => blockers = value ?? Array.Empty<BoardCardAuthoring>();
        }

        private void Reset()
        {
            boardCardView = GetComponent<BoardCardView>();
        }
    }
}
