using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // ��Ʈ��ũ ĳ���� ��Ʈ�ѷ�
    private NetworkCharacterController _cc;

    // ������ Ball �� PhysxBall ������
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    //���� �� ���׸���
    public Material _material;
    // �̵� ����
    private Vector3 _forward = Vector3.forward;

    // ��Ʈ��ũ Ÿ�̸�
    [Networked] private TickTimer delay { get; set; }

    // �÷��̾� �Ӽ� �� �ϳ��� ���� ����
    [Networked]
    public bool spawned { get; set; }

    // ���� ������
    private ChangeDetector _changeDetector;

    private void Awake()
    {
        // ��Ʈ��ũ ĳ���� ��Ʈ�ѷ� �ʱ�ȭ
        _cc = GetComponent<NetworkCharacterController>();

        // �ڽ� ������Ʈ�� ��Ƽ���� ��������
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    // ���� �̺�Ʈ �߻� �� ȣ��Ǵ� �ݹ�
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    // ���� ���� ���� �� �������� ���� ó��
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
        // �Է� ������ �ְ� R Ű�� ������ �� �޽��� ����
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    //private TMP_Text _messages;
    GameObject _chat;
    [SerializeField] GameObject _chat_Content_Text;
    // �޽��� ���� RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // �޽��� ���� RPC
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

    //��Ʈ��ũ FixedUpdate ó��
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
                          // Ball �ʱ�ȭ �� ����ȭ
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