using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class AbstractUnit : AbstractCommandable, IMoveable
{
    public float AgentRadius => agent.radius;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Start()
    {
        base.Start();
        Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }
}