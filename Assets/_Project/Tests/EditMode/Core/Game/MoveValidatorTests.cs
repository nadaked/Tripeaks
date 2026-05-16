using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;
using NUnit.Framework;

namespace _Project.Tests.EditMode.Core.Game
{
    public sealed class MoveValidatorTests
    {
        private MoveValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new MoveValidator();
        }

        [Test]
        public void CanPlay_WhenRankIsOneHigher_ReturnsTrue()
        {
            var board = CardData.Normal(CardRank.Five, CardSuit.Hearts);
            var waste = CardData.Normal(CardRank.Four, CardSuit.Spades);

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenRankIsOneLower_ReturnsTrue()
        {
            var board = CardData.Normal(CardRank.Five, CardSuit.Hearts);
            var waste = CardData.Normal(CardRank.Six, CardSuit.Spades);

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenAceAndKing_ReturnsTrue()
        {
            var board = CardData.Normal(CardRank.Ace, CardSuit.Hearts);
            var waste = CardData.Normal(CardRank.King, CardSuit.Spades);

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenKingAndAce_ReturnsTrue()
        {
            var board = CardData.Normal(CardRank.King, CardSuit.Hearts);
            var waste = CardData.Normal(CardRank.Ace, CardSuit.Spades);

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenRankIsNotAdjacent_ReturnsFalse()
        {
            var board = CardData.Normal(CardRank.Five, CardSuit.Hearts);
            var waste = CardData.Normal(CardRank.Eight, CardSuit.Spades);

            Assert.IsFalse(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenBoardCardIsWild_ReturnsTrue()
        {
            var board = CardData.Wild();
            var waste = CardData.Normal(CardRank.Eight, CardSuit.Spades);

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }

        [Test]
        public void CanPlay_WhenWasteCardIsWild_ReturnsTrue()
        {
            var board = CardData.Normal(CardRank.Five, CardSuit.Hearts);
            var waste = CardData.Wild();

            Assert.IsTrue(_validator.CanPlay(board, waste));
        }
        
        [Test]
        public void CanPlay_WhenDualRankMatchesAnyAdjacentRank_ReturnsTrue()
        {
            var card = CardData.DualRank(CardRank.Ace, CardRank.King);
    
            Assert.IsTrue(_validator.CanPlay(card, CardData.Normal(CardRank.Two, CardSuit.Spades)));
            Assert.IsTrue(_validator.CanPlay(card, CardData.Normal(CardRank.Queen, CardSuit.Spades)));
            Assert.IsTrue(_validator.CanPlay(card, CardData.Normal(CardRank.Ace, CardSuit.Spades)));
            Assert.IsTrue(_validator.CanPlay(card, CardData.Normal(CardRank.King, CardSuit.Spades)));
        }
    }
}
