using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
}