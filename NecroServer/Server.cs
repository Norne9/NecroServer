using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Packets;
using Game;

namespace NecroServer
{
    class Server : INetEventListener
    {
        private readonly NetManager _server;
        private bool _work = true;
        private readonly Config _config;
        private readonly MasterClient _masterClient;
        private NetSerializer _netSerializer;
        
        private World _world;
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, NetPeer> _peers = new Dictionary<int, NetPeer>();

        private ServerState _serverState = ServerState.Started;
        private DateTime _startTime = DateTime.Now;
        private float _waitTime = 0f;

        private ConcurrentQueue<Func<Task>> _requestQueue = new ConcurrentQueue<Func<Task>>();

        public Server(Config config, MasterClient masterClient)
        {
            Logger.Log($"SERVER creating...");
            _masterClient = masterClient;
            _config = config;
            _server = new NetManager(this, _config.MaxPlayers, _config.ConnectionKey)
            {
                UpdateTime = _config.UpdateDelay,
            };

            Logger.Log($"SERVER register packets");
            _netSerializer = new NetSerializer();
            Packet.Register(_netSerializer);
            _netSerializer.SubscribeReusable<ClientConnection, NetPeer>(OnClientConnection);
            _netSerializer.SubscribeReusable<ClientInput, NetPeer>(OnClientInput);

            _world = new World(_config);
            _world.OnGameEnd += World_OnGameEnd;
            _world.OnPlayerDead += World_OnPlayerDead;

            Logger.Log($"SERVER created");
        }

        private async void OnClientConnection(ClientConnection clientConnection, NetPeer peer)
        {
            //check game state
            if (_serverState == ServerState.Playing || _serverState == ServerState.EndGame)
            {
                Logger.Log($"SERVER connection attempt during game: {clientConnection.UserId}");
                _server.DisconnectPeer(peer);
                return;
            }

            //check player
            var clientData = await _masterClient.RequestClientInfo(clientConnection.UserId, clientConnection.UserKey);
            if (!clientData.Valid)
            {
                Logger.Log($"SERVER invalid connection attempt: {clientConnection.UserId}");
                _server.DisconnectPeer(peer);
                return;
            }

            //create player
            var player = new Player(clientData.UserId, peer.GetUId(), clientData.Name, clientData.DoubleUnits, _config);
            _players.Add(peer.GetUId(), player);
            if (_players.Count == 1)
            {
                _waitTime = _config.PlayerWaitTime;
                _serverState = ServerState.WaitingPlayers;
            }

            //send map
            peer.Send(_netSerializer.Serialize(_world.GetServerMap()), SendOptions.ReliableUnordered);

            //add time
            if (_waitTime < _config.MinWaitTime)
                _waitTime = _config.MinWaitTime;

            //min time if last player
            if (_players.Count >= _config.MaxPlayers && _waitTime > _config.MinWaitTime)
                _waitTime = _config.MinWaitTime;

            //send player info
            Logger.Log($"SERVER player '{player.Name}' connected");
            SendPlayersInfo();

            //Send info to master
            await SendInfoToMaster();
        }

        private void SendPlayersInfo()
        {
            var packet = new ServerPlayers()
            {
                WaitTime = _waitTime,
                Players = _players.Values.Select((p) => p.GetPlayerInfo()).ToArray(),
            };
            var data = _netSerializer.Serialize(packet);
            foreach (var (netId, player) in _players)
            {
                if (_peers.ContainsKey(netId))
                    _peers[netId].Send(data, SendOptions.ReliableUnordered);
                else if (netId >= 0)
                    Logger.Log($"SERVER unknown peer {netId}");
            }
        }

        private void OnClientInput(ClientInput input, NetPeer peer)
        {
            var res = _world.SetInput(peer.GetUId(), input);
            if (!res) _server.DisconnectPeer(peer);
        }

        private void World_OnGameEnd()
        {
            Logger.Log($"SERVER game finished {(DateTime.Now - _startTime).ToString()}");
            _serverState = ServerState.EndGame;
            _startTime = DateTime.Now;
        }

        private void World_OnPlayerDead(long userId, PlayerStatus status)
        {
            _requestQueue.Enqueue(async () =>
            {
                var stat = status;
                var id = userId;

                var result = await _masterClient.SendStatus(stat, id);
                if (result.UserId != id)
                    Logger.Log($"MASTER failed send player #{id} status");
                status.Rating = result.Rating;
            });
        }

        private void StartGame()
        {
            _world.StartGame(_players);
            _serverState = ServerState.Playing;
            SendPlayersInfo();
            _startTime = DateTime.Now;
        }

        public async Task Run()
        {
            _server.Start(_config.Port);
            var masterUpdateTask = Task.Run(() => MasterUpdate());
            Logger.Log($"SERVER started on port {_config.Port}");
            while (_work)
            {
                //update network
                _server.PollEvents();

                //countdown to start
                if (_serverState == ServerState.WaitingPlayers)
                {
                    if (_waitTime < 0f) StartGame();
                    _waitTime -= _config.UpdateDelay / 1000.0f;
                }

                //update world
                if (_serverState == ServerState.Playing)
                {
                    _world.Update();
                    foreach (var (netId, player) in _players)
                    {
                        if (player.IsAI) //AI player
                            AI.MakeStep(_config, player, _world);
                        else if (_peers.ContainsKey(netId)) //Real & connected player
                        {
                            if (player.IsAlive) //Send world frame
                            {
                                var packet = _netSerializer.Serialize(_world.GetServerFrame(player));
                                _peers[netId].Send(packet, SendOptions.Sequenced);
                            }
                            else //Send end packet
                            {
                                var packet = _netSerializer.Serialize(_world.GetServerEnd(player));
                                _peers[netId].Send(packet, SendOptions.Unreliable);
                            }
                        }
                        else
                            Logger.Log($"SERVER unknown peer {netId}");
                    }
                }

                //send stats
                if (_serverState == ServerState.EndGame)
                {
                    foreach (var (netId, player) in _players)
                        if (!player.IsAI && _peers.ContainsKey(netId))
                            _peers[netId].Send(_netSerializer.Serialize(_world.GetServerEnd(player)), SendOptions.Unreliable);
                    if ((DateTime.Now - _startTime).TotalSeconds > _config.EndWaitTime)
                        Stop("timeout");
                }

                //Wait
                await Task.Delay(_config.UpdateDelay);

#if DEBUG
                //Console commands
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.M:
                            _world.DebugMap();
                            break;
                        case ConsoleKey.Q:
                            Stop("stopped by console");
                            break;
                        case ConsoleKey.S:
                            StartGame();
                            break;
                        case ConsoleKey.D:
                            _world.DebugInfo();
                            break;
                    }
                }
#endif
            }
            _server.Stop();
            Logger.Log($"SERVER stopped");
            masterUpdateTask.Wait();
            Logger.Log($"SERVER master update task stopped");
        }

        public void Stop(string reason)
        {
            _work = false;
            Logger.Log($"SERVER stop command '{reason}'");
        }

        public async Task MasterUpdate()
        {
            while (_work)
            {
                await Task.Delay(_config.MasterUpdateDelay);
                while (_requestQueue.TryDequeue(out Func<Task> action))
                    await action();
                await SendInfoToMaster();
            }
        }
        private async Task SendInfoToMaster() =>
            await _masterClient.SendState(_serverState, _players.Where((p) => !p.Value.IsAI).Count(), _config.MaxPlayers, _config.ConnectionKey);

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Logger.Log($"SERVER socket error: {endPoint} -> {socketErrorCode}");
        }

        public void OnPeerConnected(NetPeer peer) //Just add peer and wait for auth packet
        {
            Logger.Log($"SERVER client {peer.GetUId()} connected");
            _peers.Add(peer.GetUId(), peer);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader) //Receive packets
        {
            _netSerializer.ReadAllPackets(reader, peer);
        }

        public async void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.Log($"SERVER client {peer.GetUId()} disconnected, reason: '{disconnectInfo.Reason}'");
            _peers.Remove(peer.GetUId());
            if (_serverState == ServerState.Playing)
                _world.RemovePlayer(peer.GetUId());
            else if (_players.ContainsKey(peer.GetUId()))
            {
                _players.Remove(peer.GetUId());
                SendPlayersInfo();
            }

            //Send info to master
            await SendInfoToMaster();

            if (_peers.Count == 0)
                Stop("no players");
        }


        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType) { }
    }
}
