using UnityEngine;

public class TransformUpdate 
{
    public uint Tick {  get; private set; }
    public bool IsTeleport {  get; private set; }
    public Vector3 Position { get; private set; }

    public TransformUpdate(uint tick, Vector3 position, bool isTeleport)
    {
        Tick = tick;
        Position = position;
        IsTeleport = isTeleport;
    }
}
