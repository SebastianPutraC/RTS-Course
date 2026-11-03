using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace GameDevTV.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable
    {
        [SerializeField] private Transform target;
        [SerializeField] DecalProjector decalProjector;

        private NavMeshAgent agent;

        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }
        }

        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}
