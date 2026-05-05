using System;
using _Project.Scripts.Core.Actions;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Board
{
    [Serializable]
    public readonly struct SlotData
    {
        public readonly int Index;
        public readonly CardData Card;
        public readonly int[] BlockedBy;
        public readonly GameAction UnlockAction;

        public SlotData(int index, CardData card, int[] blockedBy, GameAction unlockAction = default)
        {
            Index = index;
            Card = card;
            BlockedBy = blockedBy ?? Array.Empty<int>();
            UnlockAction = unlockAction;
        }
    }
}