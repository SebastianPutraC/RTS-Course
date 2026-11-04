using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(AbstractComandable commandable, RaycastHit hit);
        void Handle(AbstractComandable commandable, RaycastHit hit);
    }
}

