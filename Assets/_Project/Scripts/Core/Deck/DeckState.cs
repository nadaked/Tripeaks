using System.Collections.Generic;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Deck
{
    public sealed class DeckState
    {
        private readonly Stack<CardData> _cards;

        public int Count => _cards.Count;

        public DeckState(IEnumerable<CardData> cards)
        {
            _cards = new Stack<CardData>(cards);
        }

        public bool CanDraw()
        {
            return _cards.Count > 0;
        }

        public CardData Draw()
        {
            return _cards.Pop();
        }

        public void AddToBottom(IEnumerable<CardData> cards)
        {
            var current = _cards.ToArray(); 

            _cards.Clear();

            var added = cards is CardData[] array ? array : new List<CardData>(cards).ToArray();

            for (var i = added.Length - 1; i >= 0; i--)
                _cards.Push(added[i]);

            for (var i = current.Length - 1; i >= 0; i--)
                _cards.Push(current[i]);
        }

        public void AddToTop(CardData card)
        {
            _cards.Push(card);
        }
        
        public void RemoveFromBottom(int count)
        {
            if (count <= 0)
                return;

            var cards = _cards.ToArray();

            var keepCount = cards.Length - count;

            if (keepCount <= 0)
            {
                _cards.Clear();
                return;
            }

            _cards.Clear();
            
            for (var i = keepCount - 1; i >= 0; i--)
                _cards.Push(cards[i]);
        }
    }
}