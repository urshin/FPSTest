using Fusion.Sockets;
using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Addons.Physics;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
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

    private NetworkRunner _runner;

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        gameObject.AddComponent<RunnerSimulatePhysics3D>();

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()

        });
    }


    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();



    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        print(nameof(OnPlayerJoined));
        if (runner.IsServer)
        {
            // Create a unique position for the player
           // Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 0, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        print(nameof(OnPlayerLeft));

        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            
        }
    }


    private bool _mouseButton0;
    private bool _mouseButton1;
    private void Update()
    {
        _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        //if (Input.GetKey(KeyCode.W))
        //    data.direction += Vector3.forward;

        //if (Input.GetKey(KeyCode.S))
        //    data.direction += Vector3.back;

        //if (Input.GetKey(KeyCode.A))
        //    data.direction += Vector3.left;

        //if (Input.GetKey(KeyCode.D))
        //    data.direction += Vector3.right;

        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        data.direction = new Vector3(horizontalMovement, 0f, verticalMovement).normalized;


       data.mouseX= Input.GetAxis("Mouse X");
       data.mouseY= Input.GetAxis("Mouse Y");

        //마우스 입력
        if (_mouseButton0)
            data.buttons |= NetworkInputData.MOUSEBUTTON1;
        _mouseButton0 = false;

        if (_mouseButton1)
            data.buttons |= NetworkInputData.MOUSEBUTTON2;
        _mouseButton1 = false;





        input.Set(data);
    }
 

   
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        print(nameof(OnInputMissing));
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        print(nameof(OnShutdown));
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        print(nameof(OnConnectedToServer));
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        print(nameof(OnConnectedToServer));
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        print(nameof(OnConnectRequest));
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        print(nameof(OnConnectFailed));
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        print(nameof(OnUserSimulationMessage));
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        print(nameof(OnSessionListUpdated));
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        print(nameof(OnCustomAuthenticationResponse));
    }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        print(nameof(OnHostMigration));
    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        print(nameof(OnSceneLoadDone));
    }
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        print(nameof(OnSceneLoadStart));
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        print(nameof(OnObjectExitAOI));
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        print(nameof(OnObjectEnterAOI));
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        print(nameof(OnReliableDataReceived));
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        print(nameof(OnReliableDataProgress));
    }
}

