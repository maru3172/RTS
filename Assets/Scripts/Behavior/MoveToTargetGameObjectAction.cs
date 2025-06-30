using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move to Target GameObject", story: "[Agent] moves to [TargetGameObject] .", category: "Action/Navigation", id: "dc3abda82340112edd93b70a53a68d3d")]
public partial class MoveToTargetGameObjectAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
    private NavMeshAgent agent; 

    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent(out agent)) return Status.Failure;

        Vector3 targetPosition = TargetGameObject.Value.transform.position;
        if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
            return Status.Success;

        agent.SetDestination(targetPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) return Status.Success;

        return Status.Running;
    }
}

