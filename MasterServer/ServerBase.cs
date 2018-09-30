using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterReqResp;
using System.Net;

namespace MasterServer
{
    public class ServerBase
    {
        private List<GameServer> Servers = new List<GameServer>();
        private Config Config;

        public ServerBase(Config config)
        {
            Config = config;
        }

        public void UpdateServer(ReqState state, IPAddress ip)
        {
            var server = Servers.Where((s) => s.Address == ip && s.Port == state.Port).FirstOrDefault();
            if (server == null)
            {
                server = new GameServer()
                {
                    Address = ip,
                    ConnectedPlayers = state.ConnectedPlayers,
                    InLobby = state.InLobby,
                    LastData = DateTime.Now,
                    Port = state.Port,
                    TotalPlayers = state.TotalPlayers
                };
                Servers.Add(server);
            }
            else
            {
                server.ConnectedPlayers = state.ConnectedPlayers;
                server.InLobby = state.InLobby;
                server.TotalPlayers = state.TotalPlayers;
                server.LastData = DateTime.Now;
            }
        }

        public GameServer FindServer()
        {
            for (int i = Servers.Count - 1; i >= 0; i--)
                if ((DateTime.Now - Servers[i].LastData).TotalSeconds > Config.ServerTime)
                    Servers.RemoveAt(i);

            var servers = Servers.Where((s) => s.InLobby && s.ConnectedPlayers < s.TotalPlayers && s.ConnectedPlayers > 0);
            if (!servers.Any()) servers = Servers.Where((s) => s.InLobby && s.ConnectedPlayers < s.TotalPlayers);

            var server = servers.OrderBy((s) => s.LastData.Ticks).FirstOrDefault();
            if (server != null) server.ConnectedPlayers++;

            if (server == null)
            {
                Logger.Log($"SRV not enouth servers", true);
                DebugServers();
            }

            return server;
        }

        public void DebugServers()
        {
            var onlinePlayers = Servers.Select((s) => s.ConnectedPlayers).Sum();
            var maxPlayers = Servers.Where((s) => (DateTime.Now - s.LastData).TotalSeconds > Config.ServerTime)
                .Select((s) => s.TotalPlayers).Sum();
            Logger.Log($"DEBUG players {onlinePlayers}/{maxPlayers}");
            var allServers = Servers.Where((s) => (DateTime.Now - s.LastData).TotalSeconds > Config.ServerTime);
            var playServers = allServers.Where((s) => !s.InLobby).Count();
            Logger.Log($"DEBUG count {playServers}/{allServers.Count()}");
        }
    }
}
