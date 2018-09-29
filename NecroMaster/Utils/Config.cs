using System;
using System.Collections.Generic;
using System.Text;

namespace NecroMaster
{
    public class Config
    {
        public int ServerPort { get; set; } = 7821;
        public string DiscordLog { get; set; } = null;

        public Config(string[] args)
        {

        }
    }
}
