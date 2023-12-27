using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // 네트워크 캐릭터 컨트롤러
    private NetworkCharacterController _cc;

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

    private void Update()
    {
        // 입력 권한이 있고 R 키가 눌렸을 때 메시지 전송
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    //private TMP_Text _messages;
    GameObject _chat;
    [SerializeField] GameObject _chat_Content_Text;
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
        _chat = GameObject.Find("ChatBox");

        if (messageSource == Runner.LocalPlayer)
        {
            
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }
        GameObject chatMessage =  Instantiate(_chat_Content_Text, _chat.transform);

        chatMessage.GetComponent<TextMeshProUGUI>().text = message;
    }

    //네트워크 FixedUpdate 처리
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

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