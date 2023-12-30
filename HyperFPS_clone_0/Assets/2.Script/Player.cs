using Cinemachine;
using Fusion;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Player : NetworkBehaviour
{
    public enum playerState
    {
        Chat,
        Ingame,
    }

    public playerState currentState = new playerState();
    // 네트워크 캐릭터 컨트롤러
    private NetworkCharacterController _cc;


    //애니메이션
    private Animator _animator;

    // 생성할 Ball 및 PhysxBall 프리팹
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    //변경 될 마테리얼
    public Material _material;
    // 이동 방향
    private Vector3 _forward = Vector3.forward;

    // 네트워크 타이머
    [Networked] private TickTimer delay { get; set; }



    // 플레이어 속성 중 하나인 스폰 여부
    [Networked]
    public bool spawned { get; set; }

    // 변경 감지기
    private ChangeDetector _changeDetector;



    //채팅 시스템
    TMP_InputField _inputField;
    Scrollbar _scrollbar;
    //private TMP_Text _messages;
    GameObject _chat;
    [SerializeField] GameObject _chat_Content_Text;
    [SerializeField] byte MaxChat = 20;


    //camera
    [SerializeField] GameObject CamParent;
    [SerializeField] Camera mainCamera; // 레이를 쏘기 위한 카메라
    public LayerMask targetLayer;  // 레이어 마스크


    //playerInfo
    [SerializeField] float PlayerSpeed = 5;
    [SerializeField] float rotationSpeed = 5.0f;


    //mouse
    private float pitch = 0f; // 마우스 Y축 회전 값
    private float yaw = 0f;



    private void Awake()
    {
        // 네트워크 캐릭터 컨트롤러 초기화
        _cc = GetComponent<NetworkCharacterController>();

        // 자식 오브젝트의 머티리얼 가져오기
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    // 스폰 이벤트 발생 시 호출되는 콜백
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    // 상태 변경 감지 후 렌더링에 대한 처리
    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawned):
                    //  Debug.Log("PSADF");
                    Color randomColor = new Color(Random.value, Random.value, Random.value);
                    _material.color = randomColor;
                    break;
            }
        }
    }
    private void Start()
    {
        _chat = GameObject.Find("ChatBox");
        _inputField = FindAnyObjectByType<TMP_InputField>();
        _scrollbar = GameObject.Find("ChatScrollbar").GetComponent<Scrollbar>();
        currentState = playerState.Ingame;
        _animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Object.HasInputAuthority)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;


            _animator.SetFloat("test", movementInput.magnitude);

        }

        //Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        //// 레이와 충돌한 객체를 검사합니다.
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        //{
        //    // 레이가 충돌한 지점의 좌표를 얻습니다.
        //    Vector3 targetPosition = hit.point;

        //    // 플레이어가 해당 지점을 바라보게 설정합니다.
        //    transform.LookAt(targetPosition);
        //}


        // 입력 권한이 있고 R 키가 눌렸을 때 메시지 전송
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.Return))
        {
            if (Input.GetKeyDown(KeyCode.Return) && currentState == playerState.Ingame)
            {
                _inputField.ActivateInputField();
                currentState = playerState.Chat;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && currentState == playerState.Chat)
            {
                // 엔터 키를 눌렀을 때의 동작 추가
                RPC_SendMessage(_inputField.text);
                _scrollbar.value = -0.1f;
                currentState = playerState.Ingame;
                //Debug.Log("엔터 키가 눌렸습니다. 입력된 텍스트: " + inputText);
                _inputField.text = null;
                _inputField.DeactivateInputField();

                if (_chat.transform.childCount >= MaxChat)
                {
                    Destroy(_chat.transform.GetChild(0).gameObject);
                }

            }

        }

    }


    // 메시지 전송 RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // 메시지 전달 RPC
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {


        if (messageSource == Runner.LocalPlayer)
        {

            message = $"You: {message}\n";

        }
        else
        {
            message = $"other: {message}\n";

        }
        GameObject chatMessage = Instantiate(_chat_Content_Text, _chat.transform);

        chatMessage.GetComponent<TextMeshProUGUI>().text = message;

    }

    [SerializeField] GameObject Aimpoint;

    //네트워크 FixedUpdate 처리
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {

            //Vector3 moveAmount = data.direction * PlayerSpeed * Runner.DeltaTime;
            //_cc.Move(moveAmount);

            
            Vector3 moveDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * data.direction;

            _cc.Move(moveDirection * PlayerSpeed *Runner.DeltaTime);

            

            //카메라 움직임
            transform.eulerAngles += new Vector3(0, data.mouseX, 0) * rotationSpeed * Runner.DeltaTime;
            CamParent.transform.eulerAngles += new Vector3(-data.mouseY, 0, 0) * rotationSpeed * Runner.DeltaTime;

            // y값이 특정 각도 범위를 벗어나지 않도록 클램핑
            float currentXRotation = transform.eulerAngles.x;
            if (currentXRotation > 180.0f)
            {
                currentXRotation -= 360.0f;
            }
            transform.eulerAngles = new Vector3(Mathf.Clamp(currentXRotation, -80.0f, 80.0f), transform.eulerAngles.y, 0);



            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          // Ball 초기화 후 동기화
                          o.GetComponent<Ball>().Init();
                      });
                    spawned = !spawned;
                }
                if ((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * _forward);
                      });
                    spawned = !spawned;
                }
            }
        }
    }
}