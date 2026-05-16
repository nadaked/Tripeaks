using System.Collections.Generic;
using System.Linq;
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

        public void AddAtPositions(IReadOnlyList<CardData> cards, IReadOnlyList<int> positionsFromTop)
        {
            if (cards == null || positionsFromTop == null || cards.Count == 0)
                return;

            var current = _cards.ToArray().ToList();
            var finalCount = current.Count + cards.Count;
            var cardCursor = 0;
            var currentCursor = 0;
            var positionSet = new HashSet<int>(positionsFromTop);
            var merged = new List<CardData>(finalCount);

            for (var i = 0; i < finalCount; i++)
            {
                if (positionSet.Contains(i) && cardCursor < cards.Count)
                {
                    merged.Add(cards[cardCursor++]);
                    continue;
                }

                if (currentCursor < current.Count)
                    merged.Add(current[currentCursor++]);
            }

            SetFromTopToBottom(merged);
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

        public void RemoveAtPositions(IReadOnlyList<int> positionsFromTop)
        {
            if (positionsFromTop == null || positionsFromTop.Count == 0)
                return;

            var cards = _cards.ToArray().ToList();
            var positions = positionsFromTop
                .Where(position => position >= 0 && position < cards.Count)
                .Distinct()
                .OrderByDescending(position => position);

            foreach (var position in positions)
                cards.RemoveAt(position);

            SetFromTopToBottom(cards);
        }

        private void SetFromTopToBottom(IReadOnlyList<CardData> cards)
        {
            _cards.Clear();

            for (var i = cards.Count - 1; i >= 0; i--)
                _cards.Push(cards[i]);
        }
    }
}
