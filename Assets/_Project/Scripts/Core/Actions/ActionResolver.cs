using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Core.Game;
using _Project.Scripts.Core.Undo;

namespace _Project.Scripts.Core.Actions
{
    public sealed class ActionResolver
    {
        private readonly ICardProvider _cardProvider;

        public ActionResolver(ICardProvider cardProvider)
        {
            _cardProvider = cardProvider;
        }
        
        public void Resolve(GameState state, GameAction action, MoveRecord record)
        {
            switch (action.Type)
            {
                case GameActionType.AddDeckCards:
                    AddDeckCards(state, action.Value, record);
                    break;

                case GameActionType.AddWildToDeck:
                    AddWildToDeck(state, action.Value, record);
                    break;
                case GameActionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddDeckCards(GameState state, int amount, MoveRecord record)
        {
            if (amount <= 0)
                return;

            var cards = new List<CardData>();

            for (var i = 0; i < amount; i++)
            {
                var card = _cardProvider.GetRandomNormalCard();
                cards.Add(card);
                record.AddedToDeck.Add(card);
            }

            state.Deck.AddToBottom(cards);
        }

        private static void AddWildToDeck(GameState state, int amount, MoveRecord record)
        {
            if (amount <= 0)
                return;

            var cards = new List<CardData>();

            for (var i = 0; i < amount; i++)
            {
                var card = CardData.Wild();
                cards.Add(card);
                record.AddedToDeck.Add(card);
            }

            state.Deck.AddToBottom(cards);
        }
    }
}