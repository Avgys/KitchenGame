using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong ClientId;
    public int PlayerIndex;
    public FixedString64Bytes AuthPlayerId;

    public Color Color;
    public int ColorId;
    public FixedString64Bytes PlayerName;

    public readonly bool Equals(PlayerData other) => ClientId == other.ClientId && ColorId == other.ColorId && PlayerName == other.PlayerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerIndex);
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref ColorId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref AuthPlayerId);
    }
}
 