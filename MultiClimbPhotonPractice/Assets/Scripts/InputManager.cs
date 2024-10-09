using Fusion;
using Fusion.Menu;
using Fusion.Sockets;
using MultiClimb.Menu;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    private NetInput _accumulatedInput;
    private bool _resetInput;

    public void BeforeUpdate()
    {
        TryResetInput();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (CanToggleCursorLockState(keyboard))
        {
            ToggleCursorLockState();
        }

        // Accumulate input only if the cursor is locked.
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 inputDirection = Vector2.zero;
        GetInputDirection(keyboard, ref inputDirection);
        _accumulatedInput.Direction += inputDirection;

        NetworkButtons buttons = default;
        buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);

        _accumulatedInput.Buttons = new NetworkButtons(_accumulatedInput.Buttons.Bits | buttons.Bits);
    }

    private void TryResetInput()
    {
        if (_resetInput)
        {
            _resetInput = false;
            _accumulatedInput = default;
        }
    }

    private bool CanToggleCursorLockState(Keyboard keyboard) =>
        keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame ||
            keyboard.escapeKey.wasPressedThisFrame;

    private void ToggleCursorLockState()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void GetInputDirection(Keyboard keyboard, ref Vector2 direction)
    {
        direction = Vector2.zero;
        if (keyboard.wKey.isPressed)
            direction += Vector2.up;
        if (keyboard.sKey.isPressed)
            direction += Vector2.down;
        if (keyboard.aKey.isPressed)
            direction += Vector2.left;
        if (keyboard.dKey.isPressed)
            direction += Vector2.right;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        _accumulatedInput.Direction.Normalize();
        input.Set(_accumulatedInput);
        _resetInput = true;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
        {
            await FindFirstObjectByType<MenuConnectionBehaviour>(FindObjectsInactive.Include).DisconnectAsync(ConnectFailReason.Disconnect);
            FindFirstObjectByType<FusionMenuUIGameplay>(FindObjectsInactive.Include).Controller.Show<FusionMenuUIMain>();
        }
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
}
