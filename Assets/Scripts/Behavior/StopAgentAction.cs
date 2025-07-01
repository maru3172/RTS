using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Agent", story: "[Agent] Stop Moving.", category: "Action/Navigation", id: "04722e23a7331899f3bae97b1613e1d1")]
public partial class StopAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if(Agent.Value.TryGetComponent(out NavMeshAgent agent))
        {
            if (agent.TryGetComponent(out Animator animator))
                animator.SetFloat(AnimationConstants.SPEED, 0);
            agent.ResetPath();
            return Status.Success;
        }

        return Status.Failure;
    }
}

