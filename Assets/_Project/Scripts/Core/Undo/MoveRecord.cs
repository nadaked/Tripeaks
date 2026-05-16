using System.Collections.Generic;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Undo
{
    public sealed class MoveRecord
    {
        public int PlayedSlotIndex = -1;

        public CardData PreviousWaste;
        public bool HadWaste;

        public CardData NewWaste;

        public readonly List<int> RemovedSlots = new();

        public readonly List<int> UnlockResolvedSlots = new();

        public readonly List<int> RevealedSlots = new();

        public readonly List<CardData> AddedToDeck = new();

        public readonly List<int> AddedToDeckPositions = new();

        public CardData? DrawnFromDeck;
    }
}
