using Riptide;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id {  get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField]
    private Transform camTransform;

    [SerializeField]
    private Interpolator interpolator;

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }
    private void Move(uint tick, bool isTeleport, Vector3 newPosition, Vector3 forward)
    {
        interpolator.NewUpdate(tick, isTeleport, newPosition);

        if (!IsLocal)
        {
            camTransform.forward = forward;
        } 
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal= false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
    
    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if(list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.Move(message.GetUInt(), message.GetBool(), message.GetVector3(), message.GetVector3());
        }
    }

}
