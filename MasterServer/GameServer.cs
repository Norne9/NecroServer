using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace MasterServer
{
    public class GameServer
    {
        public IPAddress Address { get; set; } = IPAddress.Any;
        public int Port { get; set; } = 0;
        public bool InLobby { get; set; } = false;
        public int ConnectedPlayers { get; set; } = 0;
        public int TotalPlayers { get; set; } = 0;
        public string Version { get; set; } = "";
        public int GameMode { get; set; } = 0;
        public DateTime LastData { get; set; } = DateTime.Now;
    }
}
