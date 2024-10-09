using Fusion;
using UnityEngine;


public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField]
    private NetworkPrefabRef _playerPrefab;

    [Networked, Capacity(12)]
    private NetworkDictionary<PlayerRef, Player> _players => default;

    public void PlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        NetworkObject networkObject = Runner.Spawn(_playerPrefab, Vector3.up, Quaternion.identity, player);
        _players.Add(player, networkObject.GetComponent<Player>());
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (!_players.TryGet(player, out Player playerBehaviour))
            return;

        _players.Remove(player);
        Runner.Despawn(playerBehaviour.Object);
    }
}
