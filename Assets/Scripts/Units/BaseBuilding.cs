using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace GameDevTV.RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private Queue<UnitSO> buildingQueue = new (MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("BuildUnit called when the queue was already full");
                return;
            }

            buildingQueue.Enqueue(unit);
            if (buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnit());
            }
        }

        private IEnumerator DoBuildUnit()
        {
            while (buildingQueue.Count > 0)
            {
                UnitSO newUnit = buildingQueue.Peek();
                yield return new WaitForSeconds(newUnit.BuildTime);
                Instantiate(newUnit.Prefab, transform.position, Quaternion.identity);
                buildingQueue.Dequeue();
            }
        }
    }
}