using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
public abstract class AbstractUnit : AbstractCommandable, IMoveable
{
    public float AgentRadius => agent.radius;
    private NavMeshAgent agent;
    protected BehaviorGraphAgent graphAgent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        graphAgent = GetComponent<BehaviorGraphAgent>();
        graphAgent.SetVariableValue("Commands", UnitCommands.Stop);
    }

    protected override void Start()
    {
        base.Start();
        Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
    }

    public void MoveTo(Vector3 position)
    {
        graphAgent.SetVariableValue("TargetLocation", position);
        graphAgent.SetVariableValue("Commands", UnitCommands.Move);
    }

    public void Stop()
    {
        graphAgent.SetVariableValue("Commands", UnitCommands.Stop);
    }
}