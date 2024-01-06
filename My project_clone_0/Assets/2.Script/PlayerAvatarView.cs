using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{

    private Animator camAnimator;
    [SerializeField]  private TextMeshProUGUI nameLabel;

    private void Start()
    {
        camAnimator = GetComponent<Animator>();
    }
    public void SetCameraTarget()
    {
        //var freeLookCamera = FindObjectOfType<CinemachineFreeLook>();

        //freeLookCamera.LookAt = transform;
        //freeLookCamera.Follow = transform;



        //StateDrivenCamera
        var cinemachineCamera = FindObjectOfType<CinemachineStateDrivenCamera>();
        cinemachineCamera.LookAt = transform;
        cinemachineCamera.Follow = transform;
        cinemachineCamera.m_AnimatedTarget = gameObject.GetComponent<Animator>();


    }
   
    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }

    private void LateUpdate()
    {
    
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }
}