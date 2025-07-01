using TMPro;
using UnityEngine;

public class Supplies : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mineralsText;
    [SerializeField] private TextMeshProUGUI gasText;
    [SerializeField] private TextMeshProUGUI populationText;

    [SerializeField] private SupplySO mineralsSO;
    [SerializeField] private SupplySO gasSO;

    public static int Minerals { get;private set; }
    public static int Gas { get;private set; }
    public static int Population { get;private set; }
    public static int PopulationLimit { get;private set; }

    private void Awake()
    {
        Bus<SupplyEvent>.OnEvent += HandleSupplyEvent;
    }

    private void OnDestroy()
    {
        Bus<SupplyEvent>.OnEvent -= HandleSupplyEvent;
    }

    private void HandleSupplyEvent(SupplyEvent evt)
    {
        if (evt.Supply.Equals(mineralsSO))
        {
            Minerals += evt.Amount;
            mineralsText.SetText(Minerals.ToString());
        }   
        else if (evt.Supply.Equals(gasSO))
        {
            Gas += evt.Amount;
            gasText.SetText(Gas.ToString());
        }
    }
}