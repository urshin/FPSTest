using Cinemachine;
using Fusion;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static NetworkInputData;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    // 네트워크에서 생성 여부를 나타내는 프로퍼티
    [Networked] public bool spawned { get; set; }

    // 딜레이 타이머
    [Networked] private TickTimer delay { get; set; }

    // ChangeDetector 인스턴스
    private ChangeDetector _changeDetector;

    // 볼 프리팹
    [SerializeField] private Ball _prefabBall;

    //전방 값.
    private Vector3 _forward = Vector3.forward;

    // 카메라 회전을 위한 Transform
    [SerializeField] Transform cameraArm;

    // 시네머신 프리룩
    [SerializeField] private PlayerAvatarView view;

    // 채팅 시스템
    private TMP_Text _messages;

    // 플레이어가 속한 팀 레이어를 표시하는 머티리얼
    public Material _material;

    // 마우스 감도 조절 매개변수
    public float sensitivity = 2.0f;

    //카메라
    [SerializeField] Camera _camera;
    [SerializeField] CinemachineFreeLook freeLookCamera;
    Animator camController;

    //카메라 부모
    [SerializeField] GameObject Cam;


    //레이 관련
    // 무시할 레이어를 설정합니다.
    public LayerMask ignoreLayer;

    private void Start()
    {
        // 마우스 커서 설정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 입력 권한이 있고 R 키를 눌렀을 때 채팅 메시지 전송
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }
   

    // 채팅 메시지를 전송하는 RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // 채팅 메시지를 릴레이하는 RPC
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }

        _messages.text += message;
    }

    // 네트워크에서 플레이어가 생성될 때 호출되는 메서드
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        _camera = Camera.main;
        // 입력 권한이 있는 경우 시네머신의 카메라 타겟을 설정합니다.
        if (Object.HasInputAuthority)
        {
            view.SetCameraTarget();
            

        }
        Debug.Log(PlayerData.NickName);
        camController = Cam.GetComponent<Animator>();
        gameObject.layer = Object.HasInputAuthority ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Enemy");

        Cam.GetComponent<PlayerAvatarView>().SetNickName(PlayerData.NickName);
    }

    // 네트워크 상태가 변경될 때 호출되는 메서드
    public override void Render()
    {
        
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawned):
                    //_material.color = Color.white;
                    _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
                    break;
            }
        }
    }

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            // freeLookCamera.m_Lens.FieldOfView = 30f;
            camController.Play("3thPersonAim");

        }
        else
        {
            // freeLookCamera.m_Lens.FieldOfView = 60f;
            camController.Play("FreeLook");
        }
    }
    // 네트워크에서 FixedUpdateNetwork 메서드를 구현한 부분
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Debug.DrawRay(cameraArm.position, cameraArm.forward, Color.blue);


            //메인 카메라 로테이션 돌리면 됨
            //Camera.main.transform.rotation = 


            //  Debug.Log(data.mouseDirection);


            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            transform.rotation = data.mouseDirection;
          
            if (data.buttons.IsSet(NetworkInputButtons.Jump))
            {
                _cc.Jump();
            }
            



            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;


            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.2f);
                    GetInfoRay();

                    Runner.Spawn(_prefabBall,
                    transform.position + _forward, transform.rotation,
                     Object.InputAuthority, (runner, o) =>
                     {
                         // 볼을 동기화하기 전에 초기화합니다.
                         o.GetComponent<Ball>().Init();
                     });
                }

                //freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
                
                
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    // freeLookCamera.m_Lens.FieldOfView = 30f;
                   // camController.Play("3thPersonAim");

                }
                else if(!data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    // freeLookCamera.m_Lens.FieldOfView = 60f;
                   // camController.Play("FreeLook");
                }    
               
            }
        }
    }

    private void GetInfoRay()
    {
        // 카메라 중앙에서 화면 상의 중점 좌표
        Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        // 레이캐스트를 위한 Ray 생성
        Ray ray = new Ray(rayOrigin, _camera.transform.forward);

        // 레이캐스트 정보 저장
        RaycastHit hitInfo;

        // 레이어를 무시하도록 설정된 layerMask 전달
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer))
        {
            // 레이에 충돌한 오브젝트 정보 출력
            Debug.Log($"Hit object: {hitInfo.collider.gameObject.name}");
        }

        // 레이를 디버그로 그리기 (최대 거리는 100으로 가정)
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }
}