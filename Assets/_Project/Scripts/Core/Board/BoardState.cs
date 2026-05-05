using System;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.Core.Board
{
    public sealed class BoardState
    {
        private readonly SlotData[] _slots;
        private readonly SlotState[] _states;

        public int SlotCount => _slots.Length;

        public BoardState(SlotData[] slots)
        {
            _slots = slots ?? Array.Empty<SlotData>();
            _states = new SlotState[_slots.Length];

            for (var i = 0; i < _slots.Length; i++)
                _states[i] = new SlotState(_slots[i].Index);
        }

        public SlotData GetSlot(int index)
        {
            return _slots[index];
        }

        public SlotState GetState(int index)
        {
            return _states[index];
        }

        public bool IsRemoved(int index)
        {
            return _states[index].IsRemoved;
        }

        public bool IsSelectable(int index)
        {
            if (IsRemoved(index))
                return false;

            var slot = _slots[index];

            return slot.BlockedBy.All(IsRemoved);
        }

        public IReadOnlyList<int> GetUnlockedSlotsAfterRemoving(int removedIndex)
        {
            var result = new List<int>();

            for (var i = 0; i < _slots.Length; i++)
            {
                if (i == removedIndex)
                    continue;

                if (IsSelectable(i))
                    result.Add(i);
            }

            return result;
        }

        public void RemoveSlot(int index)
        {
            _states[index].Remove();
        }

        public void RestoreSlot(int index)
        {
            _states[index].Restore();
        }
        
        public bool HasRemainingCards()
        {
            return _states.Any(slotState => !slotState.IsRemoved);
        }
    }
}