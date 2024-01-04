using Cinemachine;
using TMPro;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{



    public void SetCameraTarget()
    {
        var freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        freeLookCamera.LookAt = transform;
        freeLookCamera.Follow = transform;
    }
}