using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

namespace GameDevTV.RTS.Behaviour
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target Location", story: "[Agent] moves to [TargetLocation]", category: "Action/Navigation", id: "0393ce4dcb2193b989c788daa6700a27")]
    public partial class MoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        private NavMeshAgent agent;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out agent))
            {
                return Status.Failure;
            }
            if (Vector3.Distance(agent.transform.position, TargetLocation.Value) <= agent.stoppingDistance)
            {
                return Status.Success;
            }

            agent.SetDestination(TargetLocation.Value);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return Status.Success;
            }
            return Status.Running;
        }

        protected override void OnEnd()
        {

        }
    }
}