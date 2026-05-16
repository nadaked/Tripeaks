using System;
using _Project.Scripts.Core.Board;
using UnityEngine;

namespace _Project.Scripts.Application.LevelData
{
    [Serializable]
    public sealed class BoardSlotDefinition
    {
        public int index;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public int sortingOrder;
        public SerializableCardData card;
        public int[] blockedBy = Array.Empty<int>();
        public SerializableGameAction unlockAction;

        public SlotData ToSlotData()
        {
            return new SlotData(index, card.ToCardData(), blockedBy, unlockAction.ToGameAction());
        }
    }
}
