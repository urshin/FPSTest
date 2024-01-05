using Fusion.Sockets;
using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkInputData;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    // ���õ� ���� ��忡 ����Ͽ� ������ �����մϴ�.
    async void StartGame(GameMode mode)
    {
        // �÷��̾ ���� ���� �г��� ����
        PlayerData.NickName = $"Player{UnityEngine.Random.Range(0, 10000)}";
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        // Fusion ���ʸ� �����ϰ� ����� �Է��� Ȱ��ȭ�մϴ�.
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // ���� ������ NetworkSceneInfo�� �����մϴ�.
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Ư�� �̸��� ������ ����Ͽ� ������ �����ϰų� �����մϴ�.
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // ȣ���� �� �����ϴ� �� ���Ǵ� GUI ��ư
    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _mouseButton0;
    private bool _mouseButton1;

    // ���콺 ��ư ���¸� �����ϴ� ������Ʈ �޼���
    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);
    }

    // �÷��̾ ���ӿ� ������ �� ȣ��Ǵ� �ݹ�
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (runner.IsServer)
        {
            // �÷��̾ ���� ������ ��ġ ����
            Vector3 spawnPosition = new Vector3(0, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // �÷��̾� �ƹ�Ÿ�� ���� �����ϱ� ���� ����
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    // �÷��̾ ���ӿ��� ���� �� ȣ��Ǵ� �ݹ�
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    // �÷��̾� �Է��� ó���ϴ� �ݹ�
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        var data = new NetworkInputData();

        // �÷��̾� �̵� �Է� ���
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        data.direction = cameraRotation * inputDirection;
        data.moveDirection = inputDirection;
        data.mouseDirection = cameraRotation;
        // �����̽��ٿ� ����� ���� �Է� ����
        data.buttons.Set(NetworkInputButtons.Jump, Input.GetKey(KeyCode.Space));

        // ���콺 �Է� ����
        data.mouseX = Input.GetAxis("Mouse X");
        data.mouseY = Input.GetAxis("Mouse Y");

        // ���콺 ��ư 0 ���� ����
        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        _mouseButton0 = false;

        // ���콺 ��ư 0 ���� ����
        data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        _mouseButton1 = false;


        input.Set(data);
    }



    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }





}