using UnityEngine;
using UnityEngine.Rendering.Universal;
public abstract class AbstractCommandable : MonoBehaviour, ISelectable
{
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public ActionBase[] AvailableCommands { get; private set; }
    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private UnitSO UnitSO;

    protected virtual void Start()
    {
        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;
    }

    public void Deselect()
    {
        if (decalProjector != null)
            decalProjector.gameObject.SetActive(false);

        Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
    }

    public void Select()
    {
        if (decalProjector != null)
            decalProjector.gameObject.SetActive(true);

        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }
}