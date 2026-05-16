using _Project.Scripts.Core.Actions;
using _Project.Scripts.Core.Board;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Deck;
using _Project.Scripts.Core.Game;
using NUnit.Framework;

namespace _Project.Tests.EditMode.Core.Game
{
    public sealed class GameControllerTests
    {
        [Test]
        public void PlaySlot_WhenTwoSlotsUnlock_AddDeckActionsRun_AndUndoRestoresDeck()
        {
            var slot0 = new SlotData(
                index: 0,
                card: CardData.Normal(CardRank.Five, CardSuit.Hearts),
                blockedBy: new int[0]
            );

            var slot1 = new SlotData(
                index: 1,
                card: CardData.AddDeckCards(3),
                blockedBy: new[] { 0 },
                unlockAction: GameAction.AddDeckCards(3)
            );

            var slot2 = new SlotData(
                index: 2,
                card: CardData.AddDeckCards(3),
                blockedBy: new[] { 0 },
                unlockAction: GameAction.AddDeckCards(3)
            );

            var board = new BoardState(new[] { slot0, slot1, slot2 });

            var deck = new DeckBuilder()
                .AddStandardDeck()
                .Build();

            var waste = new WasteState();
            waste.Set(CardData.Normal(CardRank.Four, CardSuit.Spades));

            var state = new GameState(board, deck, waste);
            var provider = new RandomCardProvider(123);
            var controller = new GameController(state, provider);

            var deckCountBefore = state.Deck.Count;

            var played = controller.TryPlayFromBoard(0);
            var deckCountAfterPlay = state.Deck.Count;

            var undoResult = controller.Undo();
            var deckCountAfterUndo = state.Deck.Count;

            Assert.IsTrue(played.Success);
            Assert.AreEqual(deckCountBefore + 6, deckCountAfterPlay);

            Assert.IsTrue(undoResult.Success);
            Assert.AreEqual(deckCountBefore, deckCountAfterUndo);

            Assert.IsFalse(state.Board.IsRemoved(0));
            Assert.IsFalse(state.Board.GetState(1).IsUnlockResolved);
            Assert.IsFalse(state.Board.GetState(2).IsUnlockResolved);
        }

        [Test]
        public void PlaySlot_WhenUnlocksChain_RecordKeepsAnimationOrderAndRevealedSlot()
        {
            var slot0 = new SlotData(
                index: 0,
                card: CardData.Normal(CardRank.Two, CardSuit.Hearts),
                blockedBy: new[] { 1 }
            );

            var slot1 = new SlotData(
                index: 1,
                card: CardData.Wild(),
                blockedBy: new[] { 2 },
                unlockAction: GameAction.AddWildToDeck(1)
            );

            var slot2 = new SlotData(
                index: 2,
                card: CardData.AddDeckCards(3),
                blockedBy: new[] { 3 },
                unlockAction: GameAction.AddDeckCards(3)
            );

            var slot3 = new SlotData(
                index: 3,
                card: CardData.Normal(CardRank.Five, CardSuit.Clubs),
                blockedBy: new int[0]
            );

            var board = new BoardState(new[] { slot0, slot1, slot2, slot3 });

            var deck = new DeckBuilder()
                .AddStandardDeck()
                .Build();

            var waste = new WasteState();
            waste.Set(CardData.Normal(CardRank.Four, CardSuit.Spades));

            var state = new GameState(board, deck, waste);
            var provider = new RandomCardProvider(123);
            var controller = new GameController(state, provider);

            var deckCountBefore = state.Deck.Count;

            var result = controller.TryPlayFromBoard(3);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(new[] { 2, 1 }, result.Record.UnlockResolvedSlots);
            Assert.AreEqual(new[] { 0 }, result.Record.RevealedSlots);
            Assert.AreEqual(4, result.Record.AddedToDeck.Count);
            Assert.AreEqual(deckCountBefore + 4, state.Deck.Count);

            var undo = controller.Undo();

            Assert.IsTrue(undo.Success);
            Assert.AreEqual(deckCountBefore, state.Deck.Count);
            Assert.IsFalse(state.Board.IsRemoved(0));
            Assert.IsFalse(state.Board.IsRemoved(1));
            Assert.IsFalse(state.Board.IsRemoved(2));
            Assert.IsFalse(state.Board.IsRemoved(3));
        }
    }
}
