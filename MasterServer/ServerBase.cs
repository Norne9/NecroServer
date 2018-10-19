using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterReqResp;
using System.Net;
using System.Text;

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
                    TotalPlayers = state.TotalPlayers,
                    Version = state.ServerVersion,
                    GameMode = state.GameMode,
                };
                Servers.Add(server);
            }
            else
            {
                server.ConnectedPlayers = state.ConnectedPlayers;
                server.InLobby = state.InLobby;
                server.TotalPlayers = state.TotalPlayers;
                server.LastData = DateTime.Now;
                server.Version = state.ServerVersion;
            }
        }

        public GameServer FindServer(string version, int mode)
        {
            for (int i = Servers.Count - 1; i >= 0; i--)
                if ((DateTime.Now - Servers[i].LastData).TotalSeconds > Config.ServerTime)
                    Servers.RemoveAt(i);

            var sameVersion = Servers.Where((s) => s.Version == version && s.GameMode == mode);
            var servers = sameVersion.Where((s) => s.InLobby && s.ConnectedPlayers < s.TotalPlayers && s.ConnectedPlayers > 0);
            if (!servers.Any()) servers = sameVersion.Where((s) => s.InLobby && s.ConnectedPlayers < s.TotalPlayers);

            var server = servers.OrderBy((s) => s.LastData.Ticks).FirstOrDefault();
            if (server != null) server.ConnectedPlayers++;

            if (server == null)
            {
                Logger.Log($"SRV not enouth servers, version '{version}', mode {mode}", true);
                DebugServers();
            }

            return server;
        }

        public void DebugServers()
        {
            var allServers = Servers.Where((s) => (DateTime.Now - s.LastData).TotalSeconds < Config.ServerTime)
                .GroupBy((s) => s.GameMode).ToDictionary(g => g.Key, g => g.ToList());
            var sb = new StringBuilder();
            sb.AppendLine("DEBUG server modes");
            foreach (var mode in allServers.Keys)
            {
                var onlinePlayers = allServers[mode].Select((s) => s.ConnectedPlayers).Sum();
                var maxPlayers = allServers[mode].Select((s) => s.TotalPlayers).Sum();
                var playServers = allServers[mode].Where((s) => !s.InLobby).Count();
                sb.AppendLine($"[{mode}] players {onlinePlayers}/{maxPlayers}\tservers {playServers}/{allServers.Count()}");
            }
            Logger.Log(sb.ToString());
        }
    }
}
