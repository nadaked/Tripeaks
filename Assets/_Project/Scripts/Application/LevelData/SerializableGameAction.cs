using System;
using _Project.Scripts.Core.Actions;

namespace _Project.Scripts.Application.LevelData
{
    [Serializable]
    public struct SerializableGameAction
    {
        public GameActionType type;
        public int value;

        public GameAction ToGameAction()
        {
            return type switch
            {
                GameActionType.AddDeckCards => GameAction.AddDeckCards(value),
                GameActionType.AddWildToDeck => GameAction.AddWildToDeck(value),
                _ => default
            };
        }
    }
}
