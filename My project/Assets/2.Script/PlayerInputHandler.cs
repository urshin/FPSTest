using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkInputData;

public class PlayerInputHandler : MonoBehaviour
{
    //플레이어 인풋 핸들러
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private bool _mouseButton0;
    private bool _mouseButton1;

    // 마우스 버튼 상태를 추적하는 업데이트 메서드
    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //print(System.Reflection.MethodBase.GetCurrentMethod().Name);
      

        // 플레이어 이동 입력 얻기
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        networkInputData.direction = cameraRotation * inputDirection;
        networkInputData.moveDirection = inputDirection;
        networkInputData.mouseDirection = cameraRotation;
        // 스페이스바에 기반한 점프 입력 설정
        networkInputData.buttons.Set(NetworkInputButtons.Jump, Input.GetKey(KeyCode.Space));

        // 마우스 입력 추적
        networkInputData.mouseX = Input.GetAxis("Mouse X");
        networkInputData.mouseY = Input.GetAxis("Mouse Y");

        // 마우스 버튼 0 상태 추적
        networkInputData.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        _mouseButton0 = false;

        // 마우스 버튼 0 상태 추적
        networkInputData.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        _mouseButton1 = false;

        return networkInputData;
    }


}
