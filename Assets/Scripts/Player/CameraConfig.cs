using UnityEngine;

namespace GameDevTV.RTS.Player
{
    //System.Serializable and SerializeField are used to make the class and its fields visible in the inspector
    [System.Serializable]
    public class CameraConfig
    {
        [field: SerializeField] public bool EnableEdgePan { get; private set; } = true;
        [field: SerializeField] public float MousePanSpeed { get; private set; } = 5;
        //Act as the threshold for the mouse position to trigger the edge pan
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50;

        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 5;

        [field: SerializeField] public float ZoomSpeed { get; private set; } = 1;
        [field: SerializeField] public float MinZoomDistance { get; private set; } = 7.5f;

        [field: SerializeField] public float RotationSpeed { get; private set; } = 1;
    }
}
