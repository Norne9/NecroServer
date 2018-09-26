using System;
using System.Collections.Generic;
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
        private NetSerializer NetSerializer;
        private World World;

        public Server(Config config)
        {
            Logger.Log($"SERVER creating...");
            Config = config;
            server = new NetManager(this, Config.MaxPlayers, Config.ConnectionKey)
            {
                UpdateTime = Config.UpdateTime
            };
            Logger.Log($"SERVER register packets");
            NetSerializer = new NetSerializer();
            Packet.Register(NetSerializer);

            World = new World(Config);

            Logger.Log($"SERVER created");
        }

        public async Task Run()
        {
            server.Start(Config.Port);
            Logger.Log($"SERVER started on port {Config.Port}");
            while (Work)
            {
                server.PollEvents();
                await Task.Delay(Config.UpdateTime);
#if DEBUG
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.M)
                        World.DebugMap();
                }
#endif
            }
            server.Stop();
            Logger.Log($"SERVER stopped");
        }


        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Logger.Log($"SERVER socket error: {endPoint} -> {socketErrorCode}");
        }

        public void OnPeerConnected(NetPeer peer)
        {
            throw new NotImplementedException();
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            throw new NotImplementedException();
        }


        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType) { }
    }
}
