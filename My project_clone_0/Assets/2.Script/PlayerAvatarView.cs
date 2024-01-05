using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{

    private Animator camAnimator;

    private void Start()
    {
        camAnimator = GetComponent<Animator>();
    }
    public void SetCameraTarget()
    {
        var freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        freeLookCamera.LookAt = transform;
        freeLookCamera.Follow = transform;


        ////StateDrivenCamera
        //var cinemachineCamera = FindObjectOfType<CinemachineStateDrivenCamera>();
        //cinemachineCamera.LookAt = transform;
        //cinemachineCamera.Follow = transform;
        //cinemachineCamera.m_AnimatedTarget = gameObject.GetComponent<Animator>();


    }
}