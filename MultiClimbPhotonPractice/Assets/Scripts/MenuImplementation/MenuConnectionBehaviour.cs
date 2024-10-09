using Fusion;
using Fusion.Menu;
using Fusion.Photon.Realtime;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiClimb.Menu
{
    public class MenuConnectionBehaviour : FusionMenuConnectionBehaviour
    {
        [SerializeField] 
        private FusionMenuConfig _config;

        [Space]

        [Header("Provide a NetworkRunner prefab to be instantiated.\nIf no prefab is provided, a simple one will be created.")]
        [SerializeField] 
        private NetworkRunner _networkRunnerPrefab;

        private NetworkRunner _networkRunner;
        private bool _connectingSafeCheck;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private void Awake()
        {
            if (!_config)
                Log.Error("Fusion menu configuration file not provided.");
        }

        private string _sessionName;
        public override string SessionName => _sessionName;

        private int _maxPlayerCount;
        public override int MaxPlayerCount => _maxPlayerCount;

        private string _region;
        public override string Region => _region;

        private string _appVersion;
        public override string AppVersion => _appVersion;

        private List<string> _usernames;
        public override List<string> Usernames => _usernames;

        public override bool IsConnected => _networkRunner && _networkRunner.IsRunning;
        
        public override int Ping => (int)(IsConnected ? _networkRunner.GetPlayerRtt(_networkRunner.LocalPlayer) * 1000 : 0);
        
        public override Task<List<FusionMenuOnlineRegion>> RequestAvailableOnlineRegionsAsync(FusionMenuConnectArgs connectArgs)
        {
            // Force best region
            return Task.FromResult(new List<FusionMenuOnlineRegion>() { new FusionMenuOnlineRegion() { Code = string.Empty, Ping = 0 } });
        }
        
        public void SetSessionUsernames(List<string> usernames)
        {
            _usernames = usernames;
        }

        private GameMode ResolveGameMode(FusionMenuConnectArgs args)
        {
            bool isSharedSession = args.Scene.SceneName.Contains("Shared");
            if (args.Creating)
            {
                // Create session
                return isSharedSession ? GameMode.Shared : GameMode.Host;
            }

            if (string.IsNullOrEmpty(args.Session))
            {
                // QuickJoin
                return isSharedSession ? GameMode.Shared : GameMode.AutoHostOrClient;
            }

            // Join session
            return isSharedSession ? GameMode.Shared : GameMode.Client;
        }

        private ShutdownReason ResolveShutdownReason(int reason)
        {
            switch (reason)
            {
                case ConnectFailReason.UserRequest:
                    return ShutdownReason.Ok;
                case ConnectFailReason.ApplicationQuit:
                    return ShutdownReason.Ok;
                case ConnectFailReason.Disconnect:
                    return ShutdownReason.DisconnectedByPluginLogic;
                default:
                    return ShutdownReason.Error;
            }
        }

        private int ResolveConnectFailReason(ShutdownReason reason)
        {
            switch (reason)
            {
                case ShutdownReason.Ok:
                case ShutdownReason.OperationCanceled:
                    return ConnectFailReason.UserRequest;
                case ShutdownReason.DisconnectedByPluginLogic:
                case ShutdownReason.Error:
                    return ConnectFailReason.Disconnect;
                default:
                    return ConnectFailReason.None;
            }
        }

        protected override async Task<ConnectResult> ConnectAsyncInternal(FusionMenuConnectArgs connectArgs)
        {
            if (_connectingSafeCheck) return new ConnectResult() { CustomResultHandling = true, Success = false, FailReason = ConnectFailReason.None };

            _connectingSafeCheck = true;
            if (_networkRunner && _networkRunner.IsRunning)
            {
                await _networkRunner.Shutdown();
            }

            // Create and prepare Runner object
            _networkRunner = CreateRunner();
            var sceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            sceneManager.IsSceneTakeOverEnabled = false;

            // Copy and update AppSettings
            var appSettings = CopyAppSettings(connectArgs);

            // Solve StartGameArgs
            var args = new StartGameArgs();
            args.CustomPhotonAppSettings = appSettings;
            args.GameMode = ResolveGameMode(connectArgs);
            args.SessionName = _sessionName = connectArgs.Session;
            args.PlayerCount = _maxPlayerCount = connectArgs.MaxPlayerCount;

            // Scene info
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(sceneManager.GetSceneRef(connectArgs.Scene.ScenePath), LoadSceneMode.Additive);
            args.Scene = sceneInfo;

            // Cancellation Token
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            args.StartGameCancellationToken = _cancellationToken;

            var regionIndex = _config.AvailableRegions.IndexOf(connectArgs.Region);
            args.SessionNameGenerator = () => _config.CodeGenerator.EncodeRegion(_config.CodeGenerator.Create(), regionIndex);
            var startGameResult = default(StartGameResult);
            var connectResult = new ConnectResult();
            startGameResult = await _networkRunner.StartGame(args);

            connectResult.Success = startGameResult.Ok;
            connectResult.FailReason = ResolveConnectFailReason(startGameResult.ShutdownReason);
            _connectingSafeCheck = false;

            if (connectResult.Success)
            {
                _sessionName = _networkRunner.SessionInfo.Name;
            }

            return connectResult;
        }
        
        protected override async Task DisconnectAsyncInternal(int reason)
        {
            var peerMode = _networkRunner.Config?.PeerMode;
            _cancellationTokenSource.Cancel();
            await _networkRunner.Shutdown(shutdownReason: ResolveShutdownReason(reason));

            if (peerMode is NetworkProjectConfig.PeerModes.Multiple) return;

            for (int i = SceneManager.sceneCount - 1; i > 0; i--)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }

        private NetworkRunner CreateRunner()
        {
            return _networkRunnerPrefab ? UnityEngine.Object.Instantiate(_networkRunnerPrefab) : new GameObject("NetworkRunner", typeof(NetworkRunner)).GetComponent<NetworkRunner>();
        }

        private FusionAppSettings CopyAppSettings(FusionMenuConnectArgs connectArgs)
        {
            FusionAppSettings appSettings = new FusionAppSettings();
            PhotonAppSettings.Global.AppSettings.CopyTo(appSettings);
            appSettings.FixedRegion = _region = connectArgs.Region;
            appSettings.AppVersion = _appVersion = connectArgs.AppVersion;
            return appSettings;
        }
    }
}
