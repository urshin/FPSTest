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

    // ��Ʈ��ũ���� ���� ���θ� ��Ÿ���� ������Ƽ
    [Networked] public bool spawned { get; set; }

    // ������ Ÿ�̸�
    [Networked] private TickTimer delay { get; set; }

    // ChangeDetector �ν��Ͻ�
    private ChangeDetector _changeDetector;

    // �� ������
    [SerializeField] private Ball _prefabBall;

    //���� ��.
    private Vector3 _forward = Vector3.forward;

    // ī�޶� ȸ���� ���� Transform
    [SerializeField] Transform cameraArm;

    // �ó׸ӽ� ������
    [SerializeField] private PlayerAvatarView view;

    // ä�� �ý���
    private TMP_Text _messages;

    // �÷��̾ ���� �� ���̾ ǥ���ϴ� ��Ƽ����
    public Material _material;

    // ���콺 ���� ���� �Ű�����
    public float sensitivity = 2.0f;

    //ī�޶�
    [SerializeField] Camera _camera;
    [SerializeField] CinemachineFreeLook freeLookCamera;
    Animator camController;

    //ī�޶� �θ�
    [SerializeField] GameObject Cam;


    //���� ����
    // ������ ���̾ �����մϴ�.
    public LayerMask ignoreLayer;

    private void Start()
    {
        // ���콺 Ŀ�� ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // �Է� ������ �ְ� R Ű�� ������ �� ä�� �޽��� ����
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }
   

    // ä�� �޽����� �����ϴ� RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // ä�� �޽����� �������ϴ� RPC
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

    // ��Ʈ��ũ���� �÷��̾ ������ �� ȣ��Ǵ� �޼���
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        _camera = Camera.main;
        // �Է� ������ �ִ� ��� �ó׸ӽ��� ī�޶� Ÿ���� �����մϴ�.
        if (Object.HasInputAuthority)
        {
            view.SetCameraTarget();
            

        }
        Debug.Log(PlayerData.NickName);
        camController = Cam.GetComponent<Animator>();
        gameObject.layer = Object.HasInputAuthority ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Enemy");

        Cam.GetComponent<PlayerAvatarView>().SetNickName(PlayerData.NickName);
    }

    // ��Ʈ��ũ ���°� ����� �� ȣ��Ǵ� �޼���
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
    // ��Ʈ��ũ���� FixedUpdateNetwork �޼��带 ������ �κ�
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Debug.DrawRay(cameraArm.position, cameraArm.forward, Color.blue);


            //���� ī�޶� �����̼� ������ ��
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
                         // ���� ����ȭ�ϱ� ���� �ʱ�ȭ�մϴ�.
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
        // ī�޶� �߾ӿ��� ȭ�� ���� ���� ��ǥ
        Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        // ����ĳ��Ʈ�� ���� Ray ����
        Ray ray = new Ray(rayOrigin, _camera.transform.forward);

        // ����ĳ��Ʈ ���� ����
        RaycastHit hitInfo;

        // ���̾ �����ϵ��� ������ layerMask ����
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer))
        {
            // ���̿� �浹�� ������Ʈ ���� ���
            Debug.Log($"Hit object: {hitInfo.collider.gameObject.name}");
        }

        // ���̸� ����׷� �׸��� (�ִ� �Ÿ��� 100���� ����)
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }
}