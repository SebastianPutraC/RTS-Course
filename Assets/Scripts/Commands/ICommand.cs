using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.Commands
{
    public interface ICommand
    {
        //Previously
        //bool CanHandle(AbstractComandable commandable, RaycastHit hit);
        //void Handle(AbstractComandable commandable, RaycastHit hit);

        bool CanHandle(CommandContext context);
        void Handle(CommandContext context);
    }
}

