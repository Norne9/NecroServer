using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public class Config
    {
        public int Port { get; set; } = 15364;
        public int MaxPlayers { get; set; } = 40;
        public int UpdateTime { get; set; } = 10;
        public string DiscordLog { get; set; } = null;
        public string ConnectionKey { get; set; } = "debug";

        public Config(string[] args)
        {

        }
    }
}
