using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Packets;

namespace NecroServer
{
    class Server : INetEventListener
    {
        private readonly NetManager server;
        private bool Work = true;
        private readonly Config Config;
        private NetSerializer NetSerializer;

        public Server(Config config)
        {
            Config = config;
            server = new NetManager(this, Config.MaxPlayers, Config.ConnectionKey)
            {
                UpdateTime = Config.UpdateTime
            };
            NetSerializer = new NetSerializer();
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
