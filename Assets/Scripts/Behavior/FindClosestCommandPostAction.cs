using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Closest Command Post", story: "[Unit] finds nearest [CommandPost] .", category: "Action/Units", id: "abe6d4fa24f7adca550d2dfc8c88d75f")]
public partial class FindClosestCommandPostAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Unit;
    [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10);
    [SerializeReference] public BlackboardVariable<UnitSO> CommandPostBuilding;

    protected override Status OnStart()
    {
        Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius.Value, LayerMask.GetMask("Buildings"));
        List<BaseBuilding> nearbyCommandPosts = new();
        foreach(Collider collider in colliders)
        {
            if(collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(CommandPostBuilding.Value))
                nearbyCommandPosts.Add(building);
        }

        if(nearbyCommandPosts.Count == 0) return Status.Failure;

        CommandPost.Value = nearbyCommandPosts[0].gameObject;
        return Status.Success;
    }
}

