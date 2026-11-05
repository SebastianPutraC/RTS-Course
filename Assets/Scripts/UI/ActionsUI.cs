using GameDevTV.RTS.Commands;
using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using GameDevTV.RTS.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevTV.RTS.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] ActionButtons;
        private HashSet<AbstractCommandable> selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;

            foreach (UIActionButton actionButton in ActionButtons)
            {
                actionButton.SetIcon(null);
            }
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Add(commandable);
                RefreshButtons();
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> availableCommands = new(9);

            //Merge availableCommands from the unit to here^, no duplicates on UnionWith
            foreach (AbstractCommandable commandable in selectedUnits)
            {
                availableCommands.UnionWith(commandable.AvailableCommands);
            }

            for (int i = 0; i < ActionButtons.Length; i++)
            {
                ActionBase actionForSlot =
                    availableCommands.Where(action => action.Slot == i).FirstOrDefault();

                if (actionForSlot != null)
                {
                    ActionButtons[i].SetIcon(actionForSlot.Icon);
                }
                else
                {
                    ActionButtons[i].SetIcon(null);
                }
            }

        }
    }
}