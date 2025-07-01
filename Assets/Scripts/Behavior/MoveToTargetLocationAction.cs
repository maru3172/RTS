using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move to Target Location", story: "[Agent] moves to [TargetLocation] .", category: "Action/Navigation", id: "d34e0372a09045032fd32dc16bc3fb4a")]
public partial class MoveToTargetLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

    private NavMeshAgent agent;
    private Animator animator;

    protected override Status OnStart()
    {
        if(!Agent.Value.TryGetComponent(out agent)) return Status.Failure;

        Agent.Value.TryGetComponent(out animator);

        if (Vector3.Distance(agent.transform.position, TargetLocation.Value) <= agent.stoppingDistance)
            return Status.Success;

        agent.SetDestination(TargetLocation.Value);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (animator != null) animator.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (animator != null) animator.SetFloat(AnimationConstants.SPEED, 0);
    }
}

