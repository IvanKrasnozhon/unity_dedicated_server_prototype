using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField]
    private float timeElapsed = 0f;

    [SerializeField]
    private float timeToReachTarget = 0.05f;

    [SerializeField]
    private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new();
    private float squareMovementThreshold;

    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        to = new TransformUpdate(NetworkManager.Singleton.ServerTick, transform.position, false);
        from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position, false);
        previous = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position, false);
    }

    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (NetworkManager.Singleton.ServerTick >= futureTransformUpdates[i].Tick)
            {
                if (futureTransformUpdates[i].IsTeleport)
                {
                    to = futureTransformUpdates[i];
                    from = to;
                    previous = to;
                    transform.position = to.Position;
                }
                else
                {
                    previous = to;
                    to = futureTransformUpdates[i];
                    from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position, false);
                }

                futureTransformUpdates.RemoveAt(i);
                i--;
                timeElapsed = 0f;
                timeToReachTarget = Mathf.Max(0.05f, (to.Tick - from.Tick) * Time.fixedDeltaTime);
            }
        }

        timeElapsed += Time.deltaTime;
        InterpolatePosition(timeElapsed / timeToReachTarget);
    }

    private void InterpolatePosition(float lerpAmount)
    {
        if ((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
        {
            if (to.Position != from.Position)
            {
                transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);
            }

            return;
        }

        //if (float.IsNaN(to.Position.x) || float.IsNaN(to.Position.y) || float.IsNaN(to.Position.z))
        //{
        //    Debug.LogError("to.Position contains NaN!");
        //    return;
        //}
        //if (float.IsNaN(from.Position.x) || float.IsNaN(from.Position.y) || float.IsNaN(from.Position.z))
        //{
        //    Debug.LogError("from.Position contains NaN!");
        //    return;
        //}

        float sqrMag = (to.Position - previous.Position).sqrMagnitude;
        Debug.Log($"Squared Magnitude: {sqrMag}");

        //if (float.IsNaN(lerpAmount) || float.IsInfinity(lerpAmount))
        //{
        //    Debug.LogError($"Invalid lerpAmount: {lerpAmount}");
        //    return;
        //}


        //if (float.IsNaN(sqrMag) || float.IsInfinity(sqrMag))
        //{
        //    Debug.LogError("Invalid squared magnitude calculation!");
        //    return;
        //}
        Debug.Log($"FROM: {from.Position}, TO: {to.Position}");


        transform.position = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
    }

    public void NewUpdate(uint tick, bool isTeleport, Vector3 position)
    {
        if (tick <= NetworkManager.Singleton.InterpolationTick && !isTeleport)
        {
            return;
        }

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position, isTeleport));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position, isTeleport));
    }
}
