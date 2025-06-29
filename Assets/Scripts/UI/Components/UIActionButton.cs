using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIActionButton : MonoBehaviour, IUIElement<ActionBase, UnityAction>
{
    [SerializeField] private Image Icon;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        Disable();
    }

    public void EnableFor(ActionBase action, UnityAction onClick)
    {
        SetIcon(action.Icon);
        button.interactable = true;
        button.onClick.AddListener(onClick);
    }

    public void Disable()
    {
        SetIcon(null);
        button.interactable = false;
        button.onClick.RemoveAllListeners();
    }

    private void SetIcon(Sprite icon)
    {
        if(icon == null)
            this.Icon.enabled = false;
        else
        {
            this.Icon.sprite = icon;
            this.Icon.enabled = true;
        }
    }
}
