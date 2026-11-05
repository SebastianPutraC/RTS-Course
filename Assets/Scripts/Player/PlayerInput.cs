using GameDevTV.RTS.Commands;
using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using GameDevTV.RTS.Units;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.CanvasScaler;

namespace GameDevTV.RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Camera camera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;

        private Vector2 startingMousePosition;

        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float maxRotationAmount;
        //100 as max number of units that can be selected at once and to prevent resizing list at runtime
        private HashSet<AbstractUnit> aliveUnits = new HashSet<AbstractUnit>(100);
        //Units inside of the selectionBox before added to selectedUnits
        private HashSet<AbstractUnit> addedUnits = new HashSet<AbstractUnit>(24);
        //12 same reason as hashset above
        private List<ISelectable> selectedUnits = new List<ISelectable>(12);

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("CinemachineCamera does not have a CinemachineFollow component.");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;

            // Should be 8 as in the initial value on the inspector (Follow Offset)
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;

        }
        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            selectedUnits.Add(evt.Unit);
        }
        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            selectedUnits.Remove(evt.Unit);
        }
        private void HandleUnitSpawn(UnitSpawnEvent evt)
        {
            aliveUnits.Add(evt.Unit);
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
            if (selectionBox == null)
            {
                return;
            }

            //Left button pressed to start
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();
            }

            //Hold but not released (drag and select)
            else if (Mouse.current.leftButton.isPressed
                && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDrag();
            }

            //Released and check if theres unit on the box
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseUp()
        {
            //Keep adding unit if shift key is pressed. Drag or click will still add new units
            if (!Keyboard.current.shiftKey.isPressed)
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
        private void HandleMouseDrag()
        {
            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AbstractUnit unit in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }
        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
        }

        //Copy to currentlySelectedUnits to avoid modifying(removing units) from the
        //selectedUnitList while iterating through it
        private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
            foreach (ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;

            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            //Center is the anchoredPosition and size is the sizeDelta of the selection box
            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            if (selectedUnits.Count == 0)
            {
                return;
            }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
            {
                List<AbstractUnit> abstractUnits = new(selectedUnits.Count);
                foreach(ISelectable selectable in selectedUnits)
                {
                    if (selectable is AbstractUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }

                //Will this do everything on the AvailableCommand on each unit?
                for (int i = 0; i < abstractUnits.Count; i++)
                {
                    //i is UnitIndex on CommandContext
                    CommandContext context = new(abstractUnits[i], hit, i);

                    foreach (ICommand command in abstractUnits[i].AvailableCommands)
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
            if (camera == null) 
            { 
                return; 
            }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            //Deselect first and then reselect to prevent same unit not selected if we clicked the same unit
            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                rotationStartTime = Time.time;
            }
            //rotationStartTime will be captured when key is pressed/released
            //Time will keep increasing which result in the rotationTime increasing as well, rotationSpeed will multiply this
            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);

            //Debug.Log("Time : " +  Time.time + " Rotation Start Time : " + rotationStartTime);
            //Debug.Log("Rotation Time : " + rotationTime);

            Vector3 targetFollowOffset;

            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else
            {
                //The fix for the follow offset z is here. 
                //Will always go to -8 if no end or page up/down key is pressed
                targetFollowOffset = new Vector3(
                    startingFollowOffset.x,
                    cinemachineFollow.FollowOffset.y,
                    startingFollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );

        }

        private bool ShouldSetRotationStartTime()
        {
            return Keyboard.current.pageUpKey.wasPressedThisFrame
                    || Keyboard.current.pageDownKey.wasPressedThisFrame
                    || Keyboard.current.pageUpKey.wasReleasedThisFrame
                    || Keyboard.current.pageDownKey.wasReleasedThisFrame;
        }

        private void HandleZooming()
        {
            // If the end key is pressed or released
            if (ShouldSetZoomStartTime())
            {
                zoomStartTime = Time.time;
            }

            //Clamp01 ensures it wont go beyond 0 and 1.
            float zoomTime = Mathf.Clamp01(Time.time - zoomStartTime) * cameraConfig.ZoomSpeed;
            Vector3 targetFollowOffset;

            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    cameraConfig.MinZoomDistance,
                    cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    startingFollowOffset.y,
                    cinemachineFollow.FollowOffset.z
                );
            }

            //The actual zooming is done here with slerp. 
            cinemachineFollow.FollowOffset = Vector3.Slerp(
                   cinemachineFollow.FollowOffset,
                   targetFollowOffset,
                   zoomTime);
        }

        private bool ShouldSetZoomStartTime()
        {
            return Keyboard.current.endKey.wasPressedThisFrame
                || Keyboard.current.endKey.wasReleasedThisFrame;
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            //It seems that linearVelocity doesnt need Time.deltaTime as it is already applied in the physics engine.
            //moveAmount *= Time.deltaTime;
            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (!cameraConfig.EnableEdgePan)
            {
                return moveAmount;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y <= cameraConfig.EdgePanSize)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
    }

}