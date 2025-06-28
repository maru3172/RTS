using UnityEngine;

public class GatherableSupply : MonoBehaviour, IGatherable
{
    [field: SerializeField] public SupplySO Supply { get; private set; }

    [field: SerializeField] public int Amount { get; private set; }

    [field: SerializeField] public bool IsBusy { get; private set; }

    private void Start()
    {
        Amount = Supply.MaxAmount;
    }

    public bool BeginGather()
    {
        if (IsBusy) return false;

        IsBusy = true;
        return true;
    }

    public int EndGather()
    {
        IsBusy = false;
        int amountGahered = Mathf.Min(Supply.AmountPerGather, Amount);
        Amount -= amountGahered;

        if (Amount <= 0) Destroy(gameObject);
        return amountGahered;
    }
}