using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Board;
using _Project.Scripts.Core.Cards;
using UnityEngine;

namespace _Project.Scripts.Application.LevelData
{
    [CreateAssetMenu(fileName = "BoardDefinition", menuName = "Tripeaks/Board Definition")]
    public sealed class BoardDefinition : ScriptableObject
    {
        [SerializeField] private BoardCardGenerationMode cardGenerationMode = BoardCardGenerationMode.RandomNormal;
        [SerializeField] private int randomSeed = 12345;
        [SerializeField] private DeckGenerationMode deckGenerationMode = DeckGenerationMode.AssistedSequence;
        [SerializeField] private int initialDeckCardCount = 20;
        [SerializeField] private bool useOpeningDeckCard;
        [SerializeField] private SerializableCardData openingDeckCard = new()
        {
            type = CardType.Normal,
            rank = CardRank.Four,
            suit = CardSuit.Spades
        };
        [SerializeField] private BoardSlotDefinition[] slots = Array.Empty<BoardSlotDefinition>();

        public BoardCardGenerationMode CardGenerationMode => cardGenerationMode;
        public int RandomSeed => randomSeed;
        public DeckGenerationMode DeckGenerationMode => deckGenerationMode;
        public int InitialDeckCardCount => Math.Max(1, initialDeckCardCount);
        public bool HasOpeningDeckCard => useOpeningDeckCard && openingDeckCard.ToCardData().IsValid;
        public CardData OpeningDeckCard => openingDeckCard.ToCardData();
        public IReadOnlyList<BoardSlotDefinition> Slots => slots;

        public BoardState BuildBoardState()
        {
            return BuildBoardState(null);
        }

        public BoardState BuildBoardState(LevelCardProvider provider)
        {
            var slotData = new SlotData[slots.Length];
            var shouldGenerate = CreateGenerationMask();
            var generatedCards = provider != null && deckGenerationMode == DeckGenerationMode.AssistedSequence
                ? provider.BuildAssistedBoardCards(slots, shouldGenerate)
                : CreateGeneratedCards(slots.Length);

            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.index = i;

                var card = shouldGenerate[i]
                    ? generatedCards[i]
                    : slot.card.ToCardData();

                slotData[i] = new SlotData(i, card, slot.blockedBy, slot.unlockAction.ToGameAction());
            }

            return new BoardState(slotData);
        }

        private bool[] CreateGenerationMask()
        {
            var mask = new bool[slots.Length];

            for (var i = 0; i < slots.Length; i++)
                mask[i] = ShouldUseGeneratedCard(slots[i]);

            return mask;
        }

        public Vector3 GetLocalPosition(int index)
        {
            if (index < 0 || index >= slots.Length)
                return Vector3.zero;

            return slots[index].localPosition;
        }

        public Vector3 GetLocalEulerAngles(int index)
        {
            if (index < 0 || index >= slots.Length)
                return Vector3.zero;

            return slots[index].localEulerAngles;
        }

        public int GetSortingOrder(int index)
        {
            if (index < 0 || index >= slots.Length)
                return index + 1;

            return slots[index].sortingOrder <= 0 ? index + 1 : slots[index].sortingOrder;
        }

        public void SetSlots(BoardSlotDefinition[] newSlots)
        {
            slots = newSlots ?? Array.Empty<BoardSlotDefinition>();

            for (var i = 0; i < slots.Length; i++)
                slots[i].index = i;
        }

        private bool ShouldUseGeneratedCard(BoardSlotDefinition slot)
        {
            var card = slot.card.ToCardData();

            if (card.IsWild || card.IsAddDeckCards)
                return false;

            if (!card.IsValid)
                return true;

            return cardGenerationMode != BoardCardGenerationMode.Fixed;
        }

        private CardData[] CreateGeneratedCards(int count)
        {
            var cards = new List<CardData>();

            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                if (suit == CardSuit.None)
                    continue;

                foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                {
                    if (rank == CardRank.None)
                        continue;

                    cards.Add(CardData.Normal(rank, suit));
                }
            }

            var random = new System.Random(randomSeed);
            for (var i = cards.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }

            while (cards.Count < count)
                cards.AddRange(cards);

            return cards.GetRange(0, count).ToArray();
        }
    }
}
