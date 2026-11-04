using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
    public class MoveCommand : ActionBase
    {
        public override bool CanHandle(AbstractComandable commandable, RaycastHit hit)
        {
            return commandable is IMoveable;
        }

        public override void Handle(AbstractComandable commandable, RaycastHit hit)
        {
            IMoveable moveable = (IMoveable)commandable;
            moveable.MoveTo(hit.point);
        }
    }
}