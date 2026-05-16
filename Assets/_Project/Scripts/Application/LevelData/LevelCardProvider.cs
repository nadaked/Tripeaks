using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Core.Board;
using _Project.Scripts.Core.Cards;

namespace _Project.Scripts.Application.LevelData
{
    public sealed class LevelCardProvider : ICardProvider
    {
        private readonly Random _random;
        private readonly DeckGenerationMode _mode;
        private CardRank _sequenceRank;
        private List<CardRank> _plannedBoardPlayRanks;

        public LevelCardProvider(int seed, DeckGenerationMode mode = DeckGenerationMode.Random, CardRank startRank = CardRank.None)
        {
            _random = new Random(seed);
            _mode = mode;
            _sequenceRank = startRank == CardRank.None ? GetRandomRank() : startRank;
        }

        public CardData GetRandomNormalCard()
        {
            return _mode == DeckGenerationMode.AssistedSequence
                ? GetSequenceCard()
                : CardData.Normal(GetRandomRank(), GetRandomSuit());
        }

        public CardData[] BuildInitialDeck(BoardState board, int count)
        {
            count = Math.Max(1, count);

            var drawOrder = _mode == DeckGenerationMode.AssistedSequence
                ? BuildComboDrawOrder(board, count)
                : BuildRandomDrawOrder(count);

            return drawOrder.Reverse().ToArray();
        }

        public CardData[] BuildAssistedBoardCards(IReadOnlyList<BoardSlotDefinition> slots, bool[] shouldGenerate)
        {
            if (slots == null || shouldGenerate == null || slots.Count != shouldGenerate.Length)
                return Array.Empty<CardData>();

            var cards = new CardData[slots.Count];
            var playPath = BuildGeneratedPlayPath(slots, shouldGenerate);
            var playRanks = BuildComboRanks(playPath.Count);

            _plannedBoardPlayRanks = playRanks;

            for (var i = 0; i < playPath.Count; i++)
                cards[playPath[i]] = CardData.Normal(playRanks[i], GetRandomSuit());

            for (var i = 0; i < cards.Length; i++)
            {
                if (!shouldGenerate[i] || cards[i].IsValid)
                    continue;

                cards[i] = CardData.Normal(GetRandomRank(), GetRandomSuit());
            }

            return cards;
        }

        private CardData[] BuildRandomDrawOrder(int count)
        {
            var cards = new CardData[count];

            for (var i = 0; i < cards.Length; i++)
                cards[i] = CardData.Normal(GetRandomRank(), GetRandomSuit());

            return cards;
        }

        private CardData[] BuildComboDrawOrder(BoardState board, int count)
        {
            if (_plannedBoardPlayRanks == null || _plannedBoardPlayRanks.Count == 0)
                return BuildAssistedDrawOrder(board, count);

            var cards = new List<CardData>(count);
            var cursor = 0;
            var runLength = NextComboRunLength(_plannedBoardPlayRanks.Count, true);

            cards.Add(CardData.Normal(GetAdjacentRank(_plannedBoardPlayRanks[0]), GetRandomSuit()));

            while (cards.Count < count)
            {
                cursor += runLength;
                if (cursor >= _plannedBoardPlayRanks.Count)
                    break;

                var nextComboRank = _plannedBoardPlayRanks[cursor];
                var fillerCount = NextDeckFillerCount();

                for (var i = 0; i < fillerCount && cards.Count < count - 1; i++)
                    cards.Add(CardData.Normal(GetNonAdjacentRank(nextComboRank), GetRandomSuit()));

                cards.Add(CardData.Normal(GetAdjacentRank(nextComboRank), GetRandomSuit()));
                runLength = NextComboRunLength(_plannedBoardPlayRanks.Count - cursor, false);
            }

            var tailRank = cards.Count > 0 ? cards[^1].Rank : GetRandomRank();
            while (cards.Count < count)
            {
                var shouldFollowChain = _random.NextDouble() < 0.62;
                tailRank = shouldFollowChain
                    ? StepRank(tailRank, _random.Next(0, 2) == 0 ? -1 : 1)
                    : GetRandomRank();

                cards.Add(CardData.Normal(tailRank, GetRandomSuit()));
            }

            return cards.ToArray();
        }

        private CardData[] BuildAssistedDrawOrder(BoardState board, int count)
        {
            var cards = new List<CardData>(count);
            var currentRank = PickStartRankNearOpenBoardCard(board);

            for (var i = 0; i < count; i++)
            {
                var shouldFollowChain = i == 0 || _random.NextDouble() < 0.72;
                currentRank = shouldFollowChain
                    ? StepRank(currentRank, _random.Next(0, 2) == 0 ? -1 : 1)
                    : GetRandomRank();

                cards.Add(CardData.Normal(currentRank, GetRandomSuit()));
            }

            return cards.ToArray();
        }

        private List<int> BuildGeneratedPlayPath(IReadOnlyList<BoardSlotDefinition> slots, bool[] shouldGenerate)
        {
            var removed = new bool[slots.Count];
            var path = new List<int>();
            var generatedCount = shouldGenerate.Count(generate => generate);

            while (path.Count < generatedCount)
            {
                var selectableGenerated = new List<int>();
                var selectableOther = new List<int>();

                for (var i = 0; i < slots.Count; i++)
                {
                    if (removed[i] || !IsDefinitionSelectable(slots[i], removed))
                        continue;

                    if (shouldGenerate[i])
                        selectableGenerated.Add(i);
                    else
                        selectableOther.Add(i);
                }

                if (selectableGenerated.Count > 0)
                {
                    var slotIndex = selectableGenerated[_random.Next(selectableGenerated.Count)];
                    removed[slotIndex] = true;
                    path.Add(slotIndex);
                    continue;
                }

                if (selectableOther.Count > 0)
                {
                    removed[selectableOther[_random.Next(selectableOther.Count)]] = true;
                    continue;
                }

                for (var i = 0; i < shouldGenerate.Length; i++)
                {
                    if (removed[i] || !shouldGenerate[i])
                        continue;

                    removed[i] = true;
                    path.Add(i);
                    break;
                }
            }

            return path;
        }

        private List<CardRank> BuildComboRanks(int count)
        {
            var ranks = new List<CardRank>(count);
            if (count <= 0)
                return ranks;

            var currentRank = GetRandomRank();
            var runRemaining = NextComboRunLength(count, true);
            var direction = _random.Next(0, 2) == 0 ? -1 : 1;

            for (var i = 0; i < count; i++)
            {
                if (runRemaining <= 0)
                {
                    currentRank = GetNonAdjacentRank(currentRank);
                    runRemaining = NextComboRunLength(count - i, false);
                    direction = _random.Next(0, 2) == 0 ? -1 : 1;
                }
                else if (i > 0)
                {
                    if (runRemaining > 2 && _random.NextDouble() < 0.18)
                        direction *= -1;

                    currentRank = StepRank(currentRank, direction);
                }

                ranks.Add(currentRank);
                runRemaining--;
            }

            return ranks;
        }

        private int NextComboRunLength(int remaining, bool openingRun)
        {
            if (remaining <= 0)
                return 0;

            var roll = _random.NextDouble();
            int length;

            if (openingRun)
            {
                length = roll switch
                {
                    < 0.12 => _random.Next(3, 5),
                    < 0.58 => _random.Next(5, 7),
                    < 0.88 => _random.Next(7, 9),
                    _ => _random.Next(9, 11)
                };
            }
            else
            {
                length = roll switch
                {
                    < 0.2 => _random.Next(2, 4),
                    < 0.62 => _random.Next(4, 6),
                    < 0.9 => _random.Next(6, 8),
                    _ => _random.Next(8, 10)
                };
            }

            return Math.Min(remaining, length);
        }

        private int NextDeckFillerCount()
        {
            var roll = _random.NextDouble();

            return roll switch
            {
                < 0.18 => 0,
                < 0.56 => 1,
                < 0.86 => 2,
                _ => 3
            };
        }

        private static bool IsDefinitionSelectable(BoardSlotDefinition slot, bool[] removed)
        {
            if (slot.blockedBy == null)
                return true;

            foreach (var blocker in slot.blockedBy)
            {
                if (blocker < 0 || blocker >= removed.Length)
                    continue;

                if (!removed[blocker])
                    return false;
            }

            return true;
        }

        private CardData GetSequenceCard()
        {
            var shouldFollowChain = _random.NextDouble() < 0.72;
            _sequenceRank = shouldFollowChain
                ? StepRank(_sequenceRank, _random.Next(0, 2) == 0 ? -1 : 1)
                : GetRandomRank();

            return CardData.Normal(_sequenceRank, GetRandomSuit());
        }

        private CardRank PickStartRankNearOpenBoardCard(BoardState board)
        {
            if (board == null)
                return GetRandomRank();

            var openRanks = new List<CardRank>();

            for (var i = 0; i < board.SlotCount; i++)
            {
                if (!board.IsSelectable(i))
                    continue;

                var card = board.GetSlot(i).Card;
                if (card.Type == CardType.Normal && card.Rank != CardRank.None)
                    openRanks.Add(card.Rank);
            }

            if (openRanks.Count == 0)
                return GetRandomRank();

            var target = openRanks[_random.Next(openRanks.Count)];
            return StepRank(target, _random.Next(0, 2) == 0 ? -1 : 1);
        }

        private CardRank GetRandomRank()
        {
            return (CardRank)_random.Next((int)CardRank.Ace, (int)CardRank.King + 1);
        }

        private CardRank GetAdjacentRank(CardRank rank)
        {
            return StepRank(rank, _random.Next(0, 2) == 0 ? -1 : 1);
        }

        private CardRank GetNonAdjacentRank(CardRank rank)
        {
            var candidate = GetRandomRank();
            var guard = 0;

            while (IsAdjacent(candidate, rank) && guard++ < 20)
                candidate = GetRandomRank();

            return candidate;
        }

        private CardSuit GetRandomSuit()
        {
            return (CardSuit)_random.Next((int)CardSuit.Clubs, (int)CardSuit.Spades + 1);
        }

        private static CardRank StepRank(CardRank rank, int delta)
        {
            var value = (int)rank + delta;

            if (value < (int)CardRank.Ace)
                value = (int)CardRank.King;
            else if (value > (int)CardRank.King)
                value = (int)CardRank.Ace;

            return (CardRank)value;
        }

        private static bool IsAdjacent(CardRank first, CardRank second)
        {
            return StepRank(first, 1) == second || StepRank(first, -1) == second;
        }
    }
}
