using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Gather Supplies", story: "[Unit] gathers [Amount] supplies from [GatherableSupplies] .", category: "Action/Units", id: "a4bd42223fe75d3bcd55d03f0a4e5dbc")]
public partial class GatherSuppliesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Unit;
    [SerializeReference] public BlackboardVariable<int> Amount;
    [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupplies;

    private float enterTime;

    protected override Status OnStart()
    {
        enterTime = Time.time;

        GatherableSupplies.Value.BeginGather();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (GatherableSupplies.Value.Supply.BaseGatherTime + enterTime <= Time.time)
        {
            int amountGathered = GatherableSupplies.Value.EndGather();
            return Status.Success;
        }

        return Status.Running;
    }
}

