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
            var temp = new Stack<CardData>(_cards);

            _cards.Clear();

            foreach (var card in cards)
                _cards.Push(card);

            foreach (var card in temp)
                _cards.Push(card);
        }

        public void AddToTop(CardData card)
        {
            _cards.Push(card);
        }
        
        public void RemoveFromBottom(int count)
        {
            if (count <= 0)
                return;

            var temp = new Stack<CardData>();
            
            while (_cards.Count > count)
                temp.Push(_cards.Pop());
            
            for (var i = 0; i < count && _cards.Count > 0; i++)
                _cards.Pop();
            
            while (temp.Count > 0)
                _cards.Push(temp.Pop());
        }
    }
}