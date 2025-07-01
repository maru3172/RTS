using System;
using TMPro;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move to Target GameObject", story: "[Agent] moves to [TargetGameObject] .", category: "Action/Navigation", id: "dc3abda82340112edd93b70a53a68d3d")]
public partial class MoveToTargetGameObjectAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
    private Animator animator;
    private NavMeshAgent agent; 

    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent(out agent) || TargetGameObject.Value == null) return Status.Failure;

        Agent.Value.TryGetComponent(out animator);

        Vector3 targetPosition= GetTargetPosition();

        if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
            return Status.Success;

        agent.SetDestination(targetPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (animator != null) animator.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);

        if (agent.remainingDistance <= agent.stoppingDistance) return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (animator != null) animator.SetFloat(AnimationConstants.SPEED, 0);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition;
        if (TargetGameObject.Value.TryGetComponent(out Collider collider))
            targetPosition = collider.ClosestPoint(agent.transform.position);
        else
            targetPosition = TargetGameObject.Value.transform.position;

        return targetPosition;
    }
}

