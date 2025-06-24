using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class AbstractUnit : MonoBehaviour, ISelectable, IMoveable
{
    [SerializeField] private DecalProjector decalProjector;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
    }

    public void Deselect()
    {
        if (decalProjector != null)
            decalProjector.gameObject.SetActive(false);

        Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public void Select()
    {
        if (decalProjector != null)
            decalProjector.gameObject.SetActive(true);

        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }
}