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

    // 선택된 게임 모드에 기반하여 게임을 시작합니다.
    async void StartGame(GameMode mode)
    {
        // 플레이어를 위한 랜덤 닉네임 생성
        PlayerData.NickName = $"Player{UnityEngine.Random.Range(0, 10000)}";
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        // Fusion 러너를 생성하고 사용자 입력을 활성화합니다.
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // 현재 씬에서 NetworkSceneInfo를 생성합니다.
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // 특정 이름의 세션을 사용하여 세션을 시작하거나 참가합니다.
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // 호스팅 및 참가하는 데 사용되는 GUI 버튼
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

    // 마우스 버튼 상태를 추적하는 업데이트 메서드
    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);
    }

    // 플레이어가 게임에 참가할 때 호출되는 콜백
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (runner.IsServer)
        {
            // 플레이어에 대한 고유한 위치 생성
            Vector3 spawnPosition = new Vector3(0, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // 플레이어 아바타를 쉽게 참조하기 위해 추적
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    // 플레이어가 게임에서 나갈 때 호출되는 콜백
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    // 플레이어 입력을 처리하는 콜백
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        var data = new NetworkInputData();

        // 플레이어 이동 입력 얻기
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        data.direction = cameraRotation * inputDirection;
        data.moveDirection = inputDirection;
        data.mouseDirection = cameraRotation;
        // 스페이스바에 기반한 점프 입력 설정
        data.buttons.Set(NetworkInputButtons.Jump, Input.GetKey(KeyCode.Space));

        // 마우스 입력 추적
        data.mouseX = Input.GetAxis("Mouse X");
        data.mouseY = Input.GetAxis("Mouse Y");

        // 마우스 버튼 0 상태 추적
        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        _mouseButton0 = false;

        // 마우스 버튼 0 상태 추적
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