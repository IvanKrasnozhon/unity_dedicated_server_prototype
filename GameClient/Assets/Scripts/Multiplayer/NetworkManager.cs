using Riptide;
using Riptide.Utils;
using System;
using System.Linq;
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
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    private uint _serverTick;

    public uint ServerTick
    {
        get => _serverTick;
        private set
        {
            _serverTick = value;
            InterpolationTick = (uint)(value - TicksBetweenPositionUpdates);
        }
    }

    public uint InterpolationTick { get; private set; }

    private uint _ticksBetweenPositionUpdates = 2;
    public uint TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = (uint)(ServerTick - value);
        }
    }

    [SerializeField]
    private string ip;

    [SerializeField]
    private ushort port;

    [Space(10)]
    [SerializeField]
    private uint tickDivergenceTolerance = 1;


    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.Disconnected += DidDisconnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.ConnectionFailed += FailedToConnect;

        ServerTick = 2;
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }
    private void DidDisconnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
        foreach (Player player in Player.list.Values)
        {
            Destroy(player.gameObject);
        }
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
    }


    private void FixedUpdate()
    {
        Client.Update();
        ServerTick++;
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    private void SetTick(uint serverTick)
    {
        if(Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
        {
            Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    public static void Sync(Message message)
    {
        Singleton.SetTick(message.GetUInt());
    }
}
