using GameDevTV.RTS.UI.Components;
using GameDevTV.RTS.Units;
using System;
using System.Collections;
using UnityEngine;

namespace GameDevTV.RTS.UI.Containers
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private UIBuildQueueButton[] unitButtons;
        [SerializeField] private ProgressBar progressBar;
        private BaseBuilding building;
        private Coroutine buildCoroutine;

        public void EnableFor(BaseBuilding item)
        {
            progressBar.SetProgress(0);
            gameObject.SetActive(true);
            building = item;
            building.OnQueueUpdated += HandleQueueUpdated;
            SetupUnitButtons();

            buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        private void SetupUnitButtons()
        {
            int i = 0;
            // i = 0, 1, 2
            for (; i < building.QueueSize; i++)
            {
                int index = i;
                unitButtons[i].EnableFor(building.Queue[i],
                    () => building.CancelBuildingUnit(index));
            }
            // Exit i = 2 then, 3, 4
            // Disable the index past the QueueSize
            for (; i < unitButtons.Length; i++)
            {
                unitButtons[i].Disable();
            }
        }

        public void Disable()
        {
            if (building != null)
            {
                building.OnQueueUpdated -= HandleQueueUpdated;
            }

            gameObject.SetActive(false);
            building = null;
            buildCoroutine = null;
        }

        private void HandleQueueUpdated(UnitSO[] unitsInQueue)
        {
            if (unitsInQueue.Length == 1 && buildCoroutine == null)
            {
                buildCoroutine = StartCoroutine(UpdateUnitProgress());
            }

            SetupUnitButtons();
        }

        private IEnumerator UpdateUnitProgress()
        {
            while(building != null && building.QueueSize > 0)
            {
                float startTime = building.CurrentQueueStartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;

                // Time keeps increasing / build time (example = 5 second) then clamp them to between 0-1
                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));

                progressBar.SetProgress(progress);
                yield return null;
            }

            buildCoroutine = null;
        }
    }
}

