namespace _Project.Scripts.Core.Board
{
    public sealed class SlotState
    {
        public int Index { get; }
        public bool IsRemoved { get; private set; }
        
        public bool IsUnlockResolved { get; private set; }

        public SlotState(int index)
        {
            Index = index;
        }

        public void Remove()
        {
            IsRemoved = true;
        }

        public void Restore()
        {
            IsRemoved = false;
        }

        public void MarkUnlockResolved()
        {
            IsUnlockResolved = true;
        }

        public void ResetUnlockResolved()
        {
            IsUnlockResolved = false;
        }
    }
}