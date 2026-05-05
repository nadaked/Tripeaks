using System.Collections.Generic;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Undo
{
    public sealed class MoveRecord
    {
        public int PlayedSlotIndex;

        public CardData PreviousWaste;
        public bool HadWaste;

        public CardData NewWaste;

        public readonly List<int> RemovedSlots = new();

        public readonly List<int> UnlockResolvedSlots = new();

        public readonly List<CardData> AddedToDeck = new();

        public CardData? DrawnFromDeck;
    }
}