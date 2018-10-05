﻿using System;
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
        private readonly NetManager server;
        private bool Work = true;
        private readonly Config Config;
        private readonly MasterClient MasterClient;
        private NetSerializer NetSerializer;
        
        private World World;
        private Dictionary<long, Player> Players = new Dictionary<long, Player>();
        private Dictionary<long, NetPeer> Peers = new Dictionary<long, NetPeer>();

        private ServerState ServerState = ServerState.Started;
        private DateTime startTime = DateTime.Now;

        private ConcurrentQueue<Func<Task>> RequestQueue = new ConcurrentQueue<Func<Task>>();

        public Server(Config config, MasterClient masterClient)
        {
            Logger.Log($"SERVER creating...");
            MasterClient = masterClient;
            Config = config;
            server = new NetManager(this, Config.MaxPlayers, Config.ConnectionKey)
            {
                UpdateTime = Config.UpdateDelay
            };

            Logger.Log($"SERVER register packets");
            NetSerializer = new NetSerializer();
            Packet.Register(NetSerializer);
            NetSerializer.SubscribeReusable<ClientConnection, NetPeer>(OnClientConnection);
            NetSerializer.SubscribeReusable<ClientInput, NetPeer>(OnClientInput);

            World = new World(Config);
            World.OnGameEnd += World_OnGameEnd;
            World.OnPlayerDead += World_OnPlayerDead;

            Logger.Log($"SERVER created");
        }

        private async void OnClientConnection(ClientConnection clientConnection, NetPeer peer)
        {
            //check game state
            if (ServerState == ServerState.Playing || ServerState == ServerState.EndGame)
            {
                Logger.Log($"SERVER connection attempt during game: {clientConnection.UserId}");
                server.DisconnectPeer(peer);
                return;
            }

            //check player
            var clientData = await MasterClient.RequestClientInfo(clientConnection.UserId, clientConnection.UserKey);
            if (!clientData.Valid)
            {
                Logger.Log($"SERVER invalid connection attempt: {clientConnection.UserId}");
                server.DisconnectPeer(peer);
                return;
            }

            //create player
            var player = new Player(clientData.UserId, peer.ConnectId, clientData.Name, clientData.DoubleUnits, Config);
            Players.Add(peer.ConnectId, player);
            if (Players.Count == 1)
            {
                startTime = DateTime.Now;
                ServerState = ServerState.WaitingPlayers;
            }

            //send map
            peer.Send(NetSerializer.Serialize(World.GetServerMap()), SendOptions.ReliableUnordered);

            //add time
            if ((DateTime.Now - startTime).TotalSeconds < 10)
                startTime = startTime.AddSeconds(5);

            //send player info
            Logger.Log($"SERVER player '{player.Name}' connected");
            SendPlayersInfo();

            //start game if last player
            if (Players.Count >= Config.MaxPlayers)
                StartGame();
        }

        private void SendPlayersInfo()
        {
            var packet = new ServerPlayers()
            {
                WaitTime = Config.PlayerWaitTime - (float)(DateTime.Now - startTime).TotalSeconds,
                Players = Players.Values.Select((p) => p.GetPlayerInfo()).ToArray(),
            };
            var data = NetSerializer.Serialize(packet);
            foreach (var (netId, player) in Players)
            {
                if (Peers.ContainsKey(netId))
                    Peers[netId].Send(data, SendOptions.ReliableUnordered);
                else if (netId >= 0)
                    Logger.Log($"SERVER unknown peer {netId}");
            }
        }

        private void OnClientInput(ClientInput input, NetPeer peer)
        {
            var res = World.SetInput(peer.ConnectId, input);
            if (!res) server.DisconnectPeer(peer);
        }

        private void World_OnGameEnd()
        {
            Logger.Log($"SERVER game finished {(DateTime.Now - startTime).ToString()}");
            ServerState = ServerState.EndGame;
            startTime = DateTime.Now;
        }

        private void World_OnPlayerDead(long userId, PlayerStatus status)
        {
            RequestQueue.Enqueue(async () =>
            {
                var stat = status;
                var id = userId;

                var result = await MasterClient.SendStatus(stat, id);
                if (result.UserId != id)
                    Logger.Log($"MASTER failed send player #{id} status");
                status.Rating = result.Rating;
            });
        }

        private void StartGame()
        {
            World.StartGame(Players);
            ServerState = ServerState.Playing;
            SendPlayersInfo();
            startTime = DateTime.Now;
        }

        public async Task Run()
        {
            server.Start(Config.Port);
            _ = Task.Run(() => MasterUpdate());
            Logger.Log($"SERVER started on port {Config.Port}");
            while (Work)
            {
                //update network
                server.PollEvents();

                //countdown to start
                if (ServerState == ServerState.WaitingPlayers)
                {
                    if ((DateTime.Now - startTime).TotalSeconds > Config.PlayerWaitTime)
                        StartGame();
                }

                //update world
                if (ServerState == ServerState.Playing)
                {
                    World.Update();
                    foreach (var (netId, player) in Players)
                    {
                        if (player.IsAI) //AI player
                            AI.MakeStep(Config, player, World);
                        else if (Peers.ContainsKey(netId)) //Real & connected player
                        {
                            if (player.IsAlive) //Send world frame
                                Peers[netId].Send(NetSerializer.Serialize(World.GetServerFrame(player)), SendOptions.Unreliable);
                            else //Send end packet
                                Peers[netId].Send(NetSerializer.Serialize(World.GetServerEnd(player)), SendOptions.Unreliable);
                        }
                        else
                            Logger.Log($"SERVER unknown peer {netId}");
                    }
                }

                //send stats
                if (ServerState == ServerState.EndGame)
                {
                    foreach (var (netId, player) in Players)
                        if (!player.IsAI) Peers[netId].Send(NetSerializer.Serialize(World.GetServerEnd(player)), SendOptions.Unreliable);
                    if ((DateTime.Now - startTime).TotalSeconds > Config.EndWaitTime)
                        Stop("timeout");
                }

                //Wait
                await Task.Delay(Config.UpdateDelay);

                //Console commands
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.M:
                            World.DebugMap();
                            break;
                        case ConsoleKey.Q:
                            Stop("stopped by console");
                            break;
                        case ConsoleKey.S:
                            StartGame();
                            break;
                        case ConsoleKey.D:
                            World.DebugInfo();
                            break;
                    }
                }
            }
            server.Stop();
            Logger.Log($"SERVER stopped");
        }

        public void Stop(string reason)
        {
            Work = false;
            Logger.Log($"SERVER stop command '{reason}'");
        }

        public async Task MasterUpdate()
        {
            while (Work)
            {
                while (RequestQueue.TryDequeue(out Func<Task> action))
                    await action();
                await Task.Delay(Config.MasterUpdateDelay);
                await MasterClient.SendState(ServerState, Players.Count, Config.MaxPlayers);
            }
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Logger.Log($"SERVER socket error: {endPoint} -> {socketErrorCode}");
        }

        public void OnPeerConnected(NetPeer peer) //Just add peer and wait for auth packet
        {
            Logger.Log($"SERVER client {peer.ConnectId} connected");
            Peers.Add(peer.ConnectId, peer);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader) //Receive packets
        {
            NetSerializer.ReadAllPackets(reader, peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.Log($"SERVER client {peer.ConnectId} disconnected, reason: '{disconnectInfo.Reason}'");
            Peers.Remove(peer.ConnectId);
            if (ServerState == ServerState.Playing)
                World.RemovePlayer(peer.ConnectId);
            else if (Players.ContainsKey(peer.ConnectId))
            {
                Players.Remove(peer.ConnectId);
                SendPlayersInfo();
            }

            if (Peers.Count == 0)
                Stop("no players");
        }


        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType) { }
    }
}
