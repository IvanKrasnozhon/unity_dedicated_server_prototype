using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

public enum ServerToClientId : ushort
{
    sync = 1,
    playerSpawned,
    playerMovement,
}

public enum ClientToServerId : ushort
{
    name = 1,
    input = 2,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }
    public uint CurrentTick { get; private set; } = 0;

    [SerializeField]
    private ushort port;

    [SerializeField]
    private ushort maxClientsCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server  = new Server();

        Server.Start(port, maxClientsCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Update();

        if (CurrentTick % 320 == 0)
            SendSync();

        CurrentTick++;
    }


    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Client.Id].gameObject);
    }
    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.sync);
        message.AddUInt(CurrentTick);

        Server.SendToAll(message);
    }
}
