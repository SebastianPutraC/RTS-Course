using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace GameDevTV.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : MonoBehaviour, ISelectable, IMoveable
    {
        //[SerializeField] private Transform target;
        [SerializeField] DecalProjector decalProjector;

        private NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }
        private void Start()
        {
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }

            //Will run the HandleUnitDeselected method in PlayerInput.cs
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);
            }

            //Will run the HandleUnitSelected method in PlayerInput.cs
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
        /*private void Update()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }*/
    }
}
