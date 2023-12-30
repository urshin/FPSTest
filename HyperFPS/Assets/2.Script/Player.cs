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
    // ��Ʈ��ũ ĳ���� ��Ʈ�ѷ�
    private NetworkCharacterController _cc;


    //�ִϸ��̼�
    private Animator _animator;

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



    //ä�� �ý���
    TMP_InputField _inputField;
    Scrollbar _scrollbar;
    //private TMP_Text _messages;
    GameObject _chat;
    [SerializeField] GameObject _chat_Content_Text;
    [SerializeField] byte MaxChat = 20;


    //camera
    [SerializeField] GameObject CamParent;
    [SerializeField] Camera mainCamera; // ���̸� ��� ���� ī�޶�
    public LayerMask targetLayer;  // ���̾� ����ũ


    //playerInfo
    [SerializeField] float PlayerSpeed = 5;
    [SerializeField] float rotationSpeed = 5.0f;


    //mouse
    private float pitch = 0f; // ���콺 Y�� ȸ�� ��
    private float yaw = 0f;



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

        //// ���̿� �浹�� ��ü�� �˻��մϴ�.
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        //{
        //    // ���̰� �浹�� ������ ��ǥ�� ����ϴ�.
        //    Vector3 targetPosition = hit.point;

        //    // �÷��̾ �ش� ������ �ٶ󺸰� �����մϴ�.
        //    transform.LookAt(targetPosition);
        //}


        // �Է� ������ �ְ� R Ű�� ������ �� �޽��� ����
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.Return))
        {
            if (Input.GetKeyDown(KeyCode.Return) && currentState == playerState.Ingame)
            {
                _inputField.ActivateInputField();
                currentState = playerState.Chat;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && currentState == playerState.Chat)
            {
                // ���� Ű�� ������ ���� ���� �߰�
                RPC_SendMessage(_inputField.text);
                _scrollbar.value = -0.1f;
                currentState = playerState.Ingame;
                //Debug.Log("���� Ű�� ���Ƚ��ϴ�. �Էµ� �ؽ�Ʈ: " + inputText);
                _inputField.text = null;
                _inputField.DeactivateInputField();

                if (_chat.transform.childCount >= MaxChat)
                {
                    Destroy(_chat.transform.GetChild(0).gameObject);
                }

            }

        }

    }


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

    //��Ʈ��ũ FixedUpdate ó��
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {

            //Vector3 moveAmount = data.direction * PlayerSpeed * Runner.DeltaTime;
            //_cc.Move(moveAmount);

            
            Vector3 moveDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * data.direction;

            _cc.Move(moveDirection * PlayerSpeed *Runner.DeltaTime);

            

            //ī�޶� ������
            transform.eulerAngles += new Vector3(0, data.mouseX, 0) * rotationSpeed * Runner.DeltaTime;
            CamParent.transform.eulerAngles += new Vector3(-data.mouseY, 0, 0) * rotationSpeed * Runner.DeltaTime;

            // y���� Ư�� ���� ������ ����� �ʵ��� Ŭ����
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