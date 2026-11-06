using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevTV.RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public int QueueSize => buildingQueue.Count;

        //This will return a copy of the queue(as array) and not the queue currently worked on here
        public UnitSO[] Queue => buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }
        private List<UnitSO> buildingQueue = new (MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("BuildUnit called when the queue was already full");
                return;
            }

            buildingQueue.Add(unit);
            if (buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnit());
            }
            //Make sure to invoke this when a unit is done building but there's another units to be built 
            else
            {
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }

        private IEnumerator DoBuildUnit()
        {
            while (buildingQueue.Count > 0)
            {
                BuildingUnit = buildingQueue[0];
                CurrentQueueStartTime = Time.time;
                //Used to check the building queue information
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());

                yield return new WaitForSeconds(BuildingUnit.BuildTime);

                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                buildingQueue.RemoveAt(0);
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }

            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }

        public void CancelBuildingUnit(int index)
        {
            if (index < 0 || index >= buildingQueue.Count)
            {
                Debug.LogError("Attempting to cancel building a unit outside the bounds of the queue!");
                return;
            }

            buildingQueue.RemoveAt(index);
            if (index == 0)
            {
                StopAllCoroutines();

                if (buildingQueue.Count > 0)
                {
                    StartCoroutine(DoBuildUnit());
                }
                else
                {
                    OnQueueUpdated?.Invoke(buildingQueue.ToArray());
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }
    }
}