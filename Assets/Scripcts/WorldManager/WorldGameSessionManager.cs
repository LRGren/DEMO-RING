using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Transports.SinglePlayer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class WorldGameSessionManager : MonoBehaviour
{
    public static WorldGameSessionManager instance;

    [Header("Active Players In Session")]
    public List<PlayerManager> players = new List<PlayerManager>();

    [Header("Network Session")]
    private const int roomDiscoveryPort = 7778;
    private UdpClient roomDiscoverySocket;

    [Header("Debug Options")]
    [SerializeField] private bool joinRoom;
    [SerializeField] private bool createRoom;
    [SerializeField] private bool closeRoom;
    [SerializeField] private bool leaveRoom;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GetComponent<NetworkManager>().ConnectionApprovalCallback = ApproveConnection;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region network session
    private void Update()
    {
        if (joinRoom)
        {
            joinRoom = false;
            StartCoroutine(JoinRoom());
        }

        if (createRoom)
        {
            createRoom = false;
            RequestCreateRoom();
        }

        if (closeRoom)
        {
            closeRoom = false;
            RequestCloseRoom();
        }

        if (leaveRoom)
        {
            leaveRoom = false;
            RequestLeaveRoom();
        }

        CheckRoomDiscovery();
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var network = NetworkManager.Singleton;

        // 房主信息
        bool isHostPlayer = request.ClientNetworkId == network.LocalClientId;
        PlayerManager hostPlayer = null;
        foreach (var obj in network.SpawnManager.SpawnedObjects.Values)
        {
            if (obj.IsPlayerObject && obj.OwnerClientId == NetworkManager.ServerClientId)
            {
                hostPlayer = obj.GetComponent<PlayerManager>();
                break;
            }
        }

        // 检查房间是否已满
        bool hasRoom = isHostPlayer || network.ConnectedClients.Count < 3; // Allow up to 3 players in the room
        if (!hasRoom)
        {
            response.Reason = "Room is full. Maximum 3 players allowed.";
        }

        response.Approved = hasRoom;
        response.CreatePlayerObject = true;

        if (!isHostPlayer)
        {
            // 如果不是房主，就生成在房主附近
            if (hostPlayer != null)
            {
                response.Position = hostPlayer.transform.position + hostPlayer.transform.right * 2f;
                response.Rotation = hostPlayer.transform.rotation;
            }
        }
    }

    public bool TryStartSinglePlayer()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.ShutdownInProgress)
            return false;

        if (network.IsListening)
            return network.IsHost && network.NetworkConfig.NetworkTransport is SinglePlayerTransport;

        if (!network.TryGetComponent<SinglePlayerTransport>(out var transport))
        {
            Debug.LogError("Single Player Transport is missing.");
            return false;
        }

        network.NetworkConfig.NetworkTransport = transport;
        return network.StartHost();
    }

    private IEnumerator JoinRoom()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.IsClient && !network.IsHost)
            yield break;

        if (!network.TryGetComponent<UnityTransport>(out var transport))
        {
            Debug.LogError("Unity Transport is missing.");
            yield break;
        }

        IPEndPoint room = null;
        yield return FindRoom(result => room = result);

        if (room == null)
        {
            Debug.Log("No available room found to join.");
            yield break;
        }

        if (network.IsHost && network.IsListening)
        {
            if (network.NetworkConfig.NetworkTransport is SinglePlayerTransport)
            {
                WorldSaveGameManager.instance.SaveGame();
                network.Shutdown();
            }
            else
            {
                Debug.LogWarning("You are already hosting a room. You cannot join another room while hosting. If you want to join a room, please stop hosting first.");
                yield break;
            }
        }

        while (network.ShutdownInProgress)
            yield return null;

        network.NetworkConfig.NetworkTransport = transport;
        transport.SetConnectionData(room.Address.ToString(), (ushort)room.Port);

        network.OnClientConnectedCallback += OnJoinRoomConnected;
        network.OnClientDisconnectCallback += OnJoinRoomDisconnected;
        if (!network.StartClient())
        {
            network.OnClientConnectedCallback -= OnJoinRoomConnected;
            network.OnClientDisconnectCallback -= OnJoinRoomDisconnected;
            StartCoroutine(ReturnToSinglePlayer());
            Debug.LogError("Failed to start client. IsListening is " + network.IsListening + ". Current transport is " + network.NetworkConfig.NetworkTransport.GetType().Name + ".");
            yield break;
        }
        else
        {
            Debug.Log("Joining room... Current transport is " + network.NetworkConfig.NetworkTransport.GetType().Name + ". IsListening is " + network.IsListening + ".");
        }
    }

    private bool TryFindRoom()
    {
        return false;
    }

    private IEnumerator FindRoom(System.Action<IPEndPoint> onFinished)
    {
        var discoveryAddress = new IPEndPoint(IPAddress.Loopback, roomDiscoveryPort);

        using (var socket = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
        {
            // 发送信息
            byte[] requestData = Encoding.UTF8.GetBytes("FIND_ROOM");
            socket.Send(requestData, requestData.Length, discoveryAddress);

            var deadline = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (socket.Available > 0)
                {
                    var sender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] responseData;
                    try
                    {
                        responseData = socket.Receive(ref sender);
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        break;
                    }
                    string reply = Encoding.UTF8.GetString(responseData);

                    // 校验 ROOM_FOUND: 前缀和端口号 端口号必须大于0
                    const string prefix = "ROOM_FOUND:";
                    if (sender.Equals(discoveryAddress) &&
                        reply.StartsWith(prefix) &&
                        ushort.TryParse(reply.Substring(prefix.Length), out ushort gamePort) &&
                        gamePort > 0)
                    {
                        onFinished(new IPEndPoint(sender.Address, gamePort));
                        yield break;
                    }
                }

                yield return null;
            }
        }

        onFinished(null);
    }

    private void StartRoomDiscovery()
    {
        if (roomDiscoverySocket != null)
            return;

        try
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, roomDiscoveryPort);
            roomDiscoverySocket = new UdpClient(endPoint);
            Debug.Log($"Room discovery started on port {roomDiscoveryPort}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start room discovery: {ex.Message}.");
        }
    }

    private void StopRoomDiscovery()
    {
        if (roomDiscoverySocket == null)
            return;

        roomDiscoverySocket?.Dispose();
        roomDiscoverySocket = null;
        Debug.Log("Room discovery stopped.");
    }

    private void CheckRoomDiscovery()
    {
        if (roomDiscoverySocket == null || roomDiscoverySocket.Available == 0)
            return;

        var sender = new IPEndPoint(IPAddress.Any, 0);
        byte[] requestData = roomDiscoverySocket.Receive(ref sender);
        string request = Encoding.UTF8.GetString(requestData);

        if (request != "FIND_ROOM")
            return;

        byte[] responseData = Encoding.UTF8.GetBytes("ROOM_FOUND:7777");
        roomDiscoverySocket.Send(responseData, responseData.Length, sender);

        Debug.Log($"Replied to room discovery request: {sender}");
    }

    private void OnJoinRoomConnected(ulong clientId)
    {
        var network = NetworkManager.Singleton;
        if (network && network.LocalClientId == clientId)
        {
            Debug.Log("Joined room successfully.");
            network.OnClientConnectedCallback -= OnJoinRoomConnected;
            network.OnClientDisconnectCallback -= OnJoinRoomDisconnected;
            network.OnClientStopped += OnClientDisconnected;
        }
    }

    private void OnJoinRoomDisconnected(ulong clientId)
    {
        var network = NetworkManager.Singleton;
        if (network && network.LocalClientId == clientId)
        {
            if (network.DisconnectReason == "")
                Debug.LogWarning("Disconnected from room. No reason provided.");
            else
                Debug.LogWarning(network.DisconnectReason);
            network.OnClientConnectedCallback -= OnJoinRoomConnected;
            network.OnClientDisconnectCallback -= OnJoinRoomDisconnected;
            StartCoroutine(ReturnToSinglePlayer());
        }
    }

    private void RequestCreateRoom()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.ShutdownInProgress)
        {
            Debug.LogWarning("Network Manager is missing.");
            return;
        }

        // 如果不是主机或者网络传输不是单人传输，则无法创建房间
        if (!network.IsHost || !(network.NetworkConfig.NetworkTransport is SinglePlayerTransport))
        {
            Debug.LogWarning("You must open a single player session to create a room.");
            return;
        }

        // Create the room
        Debug.Log("Creating room...");
        StartCoroutine(StopSinglePlayerForRoom(network));
    }

    private void RequestCloseRoom()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.ShutdownInProgress)
        {
            Debug.LogWarning("Network Manager is missing.");
            return;
        }

        // 如果不是主机或者网络传输不是Unity Transport，则无法关闭房间
        if (!network.IsHost || !(network.NetworkConfig.NetworkTransport is UnityTransport))
        {
            Debug.LogWarning("You must be the host of a room to close it.");
            return;
        }

        Debug.Log("Closing room...");
        StartCoroutine(StopHostForRoom(network));
    }

    private void RequestLeaveRoom()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.ShutdownInProgress)
        {
            Debug.LogWarning("Network Manager is missing.");
            return;
        }

        // 如果不是客户端或者网络传输不是Unity Transport，则无法离开房间
        if (!(network.IsConnectedClient && !network.IsHost) || !(network.NetworkConfig.NetworkTransport is UnityTransport))
        {
            Debug.LogWarning("You must be a client in a room to leave it.");
            return;
        }

        Debug.Log("Leaving room...");
        network.Shutdown();
    }

    private void OnClientDisconnected(bool wasHost)
    {
        var network = NetworkManager.Singleton;
        if (!network)
            return;

        network.OnClientStopped -= OnClientDisconnected;

        if (!wasHost)
        {
            Debug.LogWarning("Disconnected from room. You have left the room.");
            StartCoroutine(ReturnToSinglePlayer());
        }
    }

    private IEnumerator ReturnToSinglePlayer()
    {
        var network = NetworkManager.Singleton;
        if (!network)
            yield break;

        yield return null;
        while (network.ShutdownInProgress || network.IsListening)
            yield return null;

        if (!TryStartSinglePlayer())
        {
            Debug.LogError("Failed to restore single player session after join failure.");
            yield break;
        }

        network.LocalClient?.PlayerObject?.GetComponent<PlayerManager>()?.LoadGameFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);

        WorldAIManager.instance.SpawnAllCharacters();
    }

    private IEnumerator StopSinglePlayerForRoom(NetworkManager network)
    {
        // 检查是否支持Unity Transport
        if (!network.TryGetComponent<UnityTransport>(out var transport))
        {
            Debug.LogError("Unity Transport is missing.");
            yield break;
        }

        WorldSaveGameManager.instance.SaveGame();
        network.Shutdown();

        while (network.ShutdownInProgress || network.IsListening)
            yield return null;

        network.NetworkConfig.NetworkTransport = transport;
        Debug.Log("IsListening is " + network.IsListening + ". Single player session stopped. You can now create a room.");
        if (network.StartHost())
        {
            WorldAIManager.instance.SpawnAllCharacters();
            Debug.Log("Room created. Current transport is " + network.NetworkConfig.NetworkTransport.GetType().Name + ". You are now the host. IsHost is " + network.IsHost + ".");
        }
        else
        {
            Debug.LogError("Failed to start host. IsListening is " + network.IsListening + ". Current transport is " + network.NetworkConfig.NetworkTransport.GetType().Name + ".");
            yield break;
        }

        NetworkObject networkPlayer = network.LocalClient?.PlayerObject;
        networkPlayer?.GetComponent<PlayerManager>()?.LoadGameFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);

        StartRoomDiscovery();
    }

    private IEnumerator StopHostForRoom(NetworkManager network)
    {
        WorldSaveGameManager.instance.SaveGame();
        StopRoomDiscovery();
        network.Shutdown();

        while (network.ShutdownInProgress || network.IsListening)
            yield return null;

        if (!TryStartSinglePlayer())
        {
            Debug.LogError("Failed to restore single player session after closing room.");
            yield break;
        }

        network.LocalClient?.PlayerObject?.GetComponent<PlayerManager>()?.LoadGameFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);

        WorldAIManager.instance.SpawnAllCharacters();
    }

    #endregion

    #region local session
    public void AddPlayerToActivePlayerList(PlayerManager player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }

        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }

    public void RemovePlayerFromActivePlayerList(PlayerManager player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }

        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }
    #endregion
}
