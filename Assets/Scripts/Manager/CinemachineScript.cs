using Unity.Cinemachine;
using UnityEngine;

public class CinemachineScript : MonoBehaviour
{
    public static CinemachineScript Singleton { get; private set; }
    private CinemachineCamera cinemachineCamera;

    internal void Follow(Transform transform)
    {
        cinemachineCamera.Target = new CameraTarget { TrackingTarget = transform };
    }

    private void Awake()
    {
        Singleton = this;
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }
}
