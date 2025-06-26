using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private new Camera camera;
    [SerializeField] private CameraConfig cameraConfig;
    [SerializeField] private LayerMask selectableUnitsLayers;
    [SerializeField] private LayerMask floorLayers;
    [SerializeField] private RectTransform selectionBox;

    private Vector2 startingMousePosition;

    private ActionBase activeAction;
    private bool wasMouseDownOnUI;
    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;
    private HashSet<AbstractUnit> aliveUnits = new(100);
    private HashSet<AbstractUnit> addedUnits = new(24);
    private List<ISelectable> selectUnits = new(12);

    private void Awake()
    {
        if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            Debug.LogError("시네머신 카메라에 시네머신 팔로우가 없습니다. 줌 기능을 활성화시킬 수 없습니다!");

        startingFollowOffset = cinemachineFollow.FollowOffset;
        maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

        Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
        Bus<ActionSelectedEvent>.OnEvent += HandleActionSelected;
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        Bus<ActionSelectedEvent>.OnEvent -= HandleActionSelected;
    }

    private void HandleUnitSelected(UnitSelectedEvent evt) => selectUnits.Add(evt.Unit);
    private void HandleUnitDeselected(UnitDeselectedEvent evt) => selectUnits.Remove(evt.Unit);
    private void HandleUnitSpawn(UnitSpawnEvent evt) => aliveUnits.Add(evt.Unit);
    private void HandleActionSelected(ActionSelectedEvent evt)
    {
        activeAction = evt.Action;
        if(!activeAction.RequiresClickToActivate)
            ActivateAction(new RaycastHit());
    }

    private void Update()
    {
        HandlePanning();
        HandleZooming();
        HandleRotation();
        HandleRightClick();
        HandleDragSelect();
    }

    private void HandleDragSelect()
    {
        if (selectionBox == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDown();
        }
        else if(Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDrag();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        selectionBox.sizeDelta = Vector2.zero;
        selectionBox.gameObject.SetActive(true);
        startingMousePosition = Mouse.current.position.ReadValue();
        addedUnits.Clear();
        wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleMouseDrag()
    {
        if (activeAction != null || wasMouseDownOnUI) return;

        Bounds selectionBoxBounds = ResizeSelectionBox();
        foreach (AbstractUnit unit in aliveUnits)
        {
            Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
            if (selectionBoxBounds.Contains(unitPosition))
                addedUnits.Add(unit);
        }
    }

    private void HandleMouseUp()
    {
        if(!wasMouseDownOnUI && activeAction == null && !Keyboard.current.shiftKey.isPressed)
        {
            DeselectAllUnits();
        }
        HandleLeftClick();
        foreach (AbstractUnit unit in addedUnits)
        {
            unit.Select();
        }
        selectionBox.gameObject.SetActive(false);
    }

    private void DeselectAllUnits()
    {
        ISelectable[] currentlySelectedUnits = selectUnits.ToArray();
        foreach (ISelectable selectable in currentlySelectedUnits)
            selectable.Deselect();
    }

    private Bounds ResizeSelectionBox()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float width = mousePosition.x - startingMousePosition.x;
        float height = mousePosition.y - startingMousePosition.y;

        selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
    }

    private void HandleRightClick()
    {
        if (selectUnits.Count == 0) return;

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Mouse.current.rightButton.wasReleasedThisFrame
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
        {
            List<AbstractUnit> abstractUnits = new (selectUnits.Count);
            foreach(ISelectable selectable in selectUnits)
            {
                if (selectable is AbstractUnit unit)
                    abstractUnits.Add(unit);
            }

            for (int i = 0; i < abstractUnits.Count; i++)
            {
                CommandContext context = new(abstractUnits[i], hit, i);
                foreach(ICommand command in abstractUnits[i].AvailableCommands)
                {
                    if (command.CanHandle(context))
                    {
                        command.Handle(context);
                        break;
                    }
                }

            }
        }
    }

    private void HandleLeftClick()
    {
        if (camera == null) return;

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (activeAction == null
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
            && hit.collider.TryGetComponent(out ISelectable selectable))
        {
            selectable.Select();
        }
        else if(activeAction != null
            && !EventSystem.current.IsPointerOverGameObject()
            && Physics.Raycast(cameraRay, out hit, float.MaxValue, floorLayers))
        {
            ActivateAction(hit);
        }
    }

    private void ActivateAction(RaycastHit hit)
    {
        List<AbstractCommandable> abstractCommandables = selectUnits.Where((unit) => unit is AbstractCommandable).Cast<AbstractCommandable>().ToList();

        for (int i = 0; i < abstractCommandables.Count; i++)
        {
            CommandContext context = new(abstractCommandables[i], hit, i);
            activeAction.Handle(context);
        }
        activeAction = null;
    }

    private void HandleRotation()
    {
        if (ShouldSetRotationStartTime())
            rotationStartTime = Time.time;

        float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);

        Vector3 targetFollowOffset;
        if (Keyboard.current.pageDownKey.isPressed)
            targetFollowOffset = new Vector3(maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
        else if (Keyboard.current.pageUpKey.isPressed)
            targetFollowOffset = new Vector3(-maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
        else
            targetFollowOffset = new Vector3(startingFollowOffset.x, cinemachineFollow.FollowOffset.y, startingFollowOffset.z);

        cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, rotationTime);
    }

    private bool ShouldSetRotationStartTime()
    {
        return Keyboard.current.pageUpKey.wasPressedThisFrame || Keyboard.current.pageDownKey.wasPressedThisFrame || Keyboard.current.pageUpKey.wasReleasedThisFrame || Keyboard.current.pageDownKey.wasReleasedThisFrame;
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomStartTime())
            zoomStartTime = Time.time;

        Vector3 targetFollowOffset;
        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);

        if (Keyboard.current.endKey.isPressed)
            targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, cameraConfig.MinZoomDistance, cinemachineFollow.FollowOffset.z);
        else
            targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, startingFollowOffset.y, cinemachineFollow.FollowOffset.z);

        cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, zoomTime);
    }

    private bool ShouldSetZoomStartTime()
    {
        return Keyboard.current.endKey.wasPressedThisFrame || Keyboard.current.endKey.wasReleasedThisFrame;
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = GetKeyboardMoveAmount();
        moveAmount += GetMouseMoveAmount();

        cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
    }

    private Vector2 GetMouseMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (!cameraConfig.EnableEdgePan) return moveAmount;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        if (mousePosition.x <= cameraConfig.EdgePanSize)
            moveAmount.x -= cameraConfig.MousePanSpeed;
        else if(mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            moveAmount.x += cameraConfig.MousePanSpeed;
        if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
            moveAmount.y += cameraConfig.MousePanSpeed;
        else if (mousePosition.y <= cameraConfig.EdgePanSize)
            moveAmount.y -= cameraConfig.MousePanSpeed;

        return moveAmount;
    }

    private Vector2 GetKeyboardMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
            moveAmount.y += cameraConfig.KeyboardPanSpeed;
        if (Keyboard.current.leftArrowKey.isPressed)
            moveAmount.x -= cameraConfig.KeyboardPanSpeed;
        if (Keyboard.current.downArrowKey.isPressed)
            moveAmount.y -= cameraConfig.KeyboardPanSpeed;
        if (Keyboard.current.rightArrowKey.isPressed)
            moveAmount.x += cameraConfig.KeyboardPanSpeed;

        return moveAmount;
    }
}
