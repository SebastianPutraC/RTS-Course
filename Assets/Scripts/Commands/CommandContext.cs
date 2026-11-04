using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.Commands
{
    public struct CommandContext
    {
        public AbstractComandable Commandable { get; private set; }
        public RaycastHit Hit { get; private set; }
        public int UnitIndex { get; private set; }

        public CommandContext(AbstractComandable commandable, RaycastHit hit, int unitIndex = 0)
        {
            Commandable = commandable;
            Hit = hit;
            UnitIndex = unitIndex;
        }
    }
}
