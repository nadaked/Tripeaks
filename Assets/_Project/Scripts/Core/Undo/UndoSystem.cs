using System.Collections.Generic;

namespace _Project.Scripts.Core.Undo
{
    public sealed class UndoSystem
    {
        private readonly Stack<MoveRecord> _stack = new();

        public void Push(MoveRecord record)
        {
            _stack.Push(record);
        }

        public bool CanUndo()
        {
            return _stack.Count > 0;
        }

        public MoveRecord Pop()
        {
            return _stack.Pop();
        }
        
        public MoveRecord Peek()
        {
            return _stack.Peek();
        }
    }
}