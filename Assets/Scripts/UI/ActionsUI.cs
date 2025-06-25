using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ActionsUI : MonoBehaviour
{
    [SerializeField] private UIActionButton[] actionButtons;
    private HashSet<AbstractCommandable> selectedUnits = new(12);

    private void Awake()
    {
        Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
    }

    private void Start()
    {
        foreach (UIActionButton button in actionButtons)
        {
            button.Disable();
        }
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
    }

    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        if (evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Add(commandable);
            RefreshButtons();
        }
    }

    private void HandleUnitDeselected(UnitDeselectedEvent evt)
    {
        if (evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Remove(commandable);
            RefreshButtons();
        }
    }

    private void RefreshButtons()
    {
        HashSet<ActionBase> availableCommands = new(9);

        foreach(AbstractCommandable commandable in selectedUnits)
        {
            availableCommands.UnionWith(commandable.AvailableCommands);
        }

        for(int i = 0; i < actionButtons.Length; i++)
        {
            ActionBase actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();

            if (actionForSlot != null)
                actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
            else
                actionButtons[i].Disable();
        }
    }

    private UnityAction HandleClick(ActionBase action)
    {
        return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(action));
    }
}
