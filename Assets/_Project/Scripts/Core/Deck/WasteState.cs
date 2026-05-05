using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Core.Deck
{
    public sealed class WasteState
    {
        public CardData Current { get; private set; }

        public bool HasCard { get; private set; }

        public void Set(CardData card)
        {
            Current = card;
            HasCard = true;
        }

        public void Clear()
        {
            HasCard = false;
        }
    }
}