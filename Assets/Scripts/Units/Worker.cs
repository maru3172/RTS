
public class Worker : AbstractUnit
{
   public void Gather(GatherableSupply supply)
    {
        graphAgent.SetVariableValue("Supply", supply);
        graphAgent.SetVariableValue("TargetLocation", supply.transform.position);
        graphAgent.SetVariableValue("Commands", UnitCommands.Gather);
    }
}
