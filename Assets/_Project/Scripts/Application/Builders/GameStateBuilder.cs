using System;
using _Project.Scripts.Application.LevelData;
using _Project.Scripts.Core.Actions;
using _Project.Scripts.Core.Board;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Deck;
using _Project.Scripts.Core.Game;

namespace _Project.Scripts.Application.Builders
{
    public sealed class GameStateBuilder
    {
        public GameState BuildState(BoardDefinition boardDefinition, int deckSeed = 12345)
        {
            if (boardDefinition == null)
                return BuildTestState();

            var provider = new LevelCardProvider(deckSeed, boardDefinition.DeckGenerationMode);
            var board = boardDefinition.BuildBoardState(provider);

            var deckCards = provider.BuildInitialDeck(board, boardDefinition.InitialDeckCardCount);
            ApplyOpeningDeckCard(boardDefinition, deckCards);
            var deck = new DeckState(deckCards);

            var waste = new WasteState();

            return new GameState(board, deck, waste);
        }

        public GameState BuildState(BoardDefinition boardDefinition, LevelCardProvider provider)
        {
            if (boardDefinition == null)
                return BuildTestState();

            provider ??= new LevelCardProvider(12345, boardDefinition.DeckGenerationMode);

            var board = boardDefinition.BuildBoardState(provider);
            var deckCards = provider.BuildInitialDeck(board, boardDefinition.InitialDeckCardCount);
            ApplyOpeningDeckCard(boardDefinition, deckCards);
            var deck = new DeckState(deckCards);
            var waste = new WasteState();

            return new GameState(board, deck, waste);
        }

        private static void ApplyOpeningDeckCard(BoardDefinition boardDefinition, CardData[] deckCards)
        {
            if (boardDefinition == null || deckCards == null || deckCards.Length == 0)
                return;

            if (!boardDefinition.HasOpeningDeckCard)
                return;

            deckCards[^1] = boardDefinition.OpeningDeckCard;
        }

        public GameState BuildTestState()
        {
            var slots = new[]
            {
                new SlotData(
                    index: 0,
                    card: CardData.Normal(CardRank.Five, CardSuit.Hearts),
                    //card: CardData.DualRank(CardRank.Ace, CardRank.King, CardSuit.Clubs),
                    blockedBy: Array.Empty<int>()
                ),

                new SlotData(
                    index: 1,
                    card: CardData.AddDeckCards(3),
                    blockedBy: new[] { 0 },
                    unlockAction: GameAction.AddDeckCards(3)
                ),

                new SlotData(
                    index: 2,
                    card: CardData.Normal(CardRank.Five, CardSuit.Clubs),
                    blockedBy: new[] { 0 }
                )
            };

            var board = new BoardState(slots);

            var deck = new DeckBuilder()
                .AddStandardDeck()
                .Shuffle(seed: 12345)
                .Build();

            var waste = new WasteState();

            return new GameState(board, deck, waste);
        }
    }
}
