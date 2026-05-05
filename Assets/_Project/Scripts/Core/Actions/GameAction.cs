namespace _Project.Scripts.Core.Actions
{
    [System.Serializable]
    public readonly struct GameAction
    {
        public readonly GameActionType Type;
        public readonly int Value;

        public GameAction(GameActionType type, int value = 0)
        {
            Type = type;
            Value = value;
        }

        public static GameAction AddDeckCards(int amount)
        {
            return new GameAction(GameActionType.AddDeckCards, amount);
        }

        public static GameAction AddWildToDeck(int amount)
        {
            return new GameAction(GameActionType.AddWildToDeck, amount);
        }
    }
}