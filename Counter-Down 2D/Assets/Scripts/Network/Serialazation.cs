using UnityEngine;
using Unity.Netcode;

struct SVector2 : INetworkSerializable
{
    public Vector2 Vector2D;

    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Vector2D);
    }
    // ~INetworkSerializable
}