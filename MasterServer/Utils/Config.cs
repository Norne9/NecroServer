using System;
using System.Collections.Generic;
using System.Text;

namespace MasterServer
{
    public class Config
    {
        public int ServerPort { get; set; } = 7821;
        public string DiscordLog { get; set; } = null;
        public string UserBasePath { get; set; } = "Users";
        public string UserBaseExt { get; set; } = ".usr";
        public float LeaderboardTime { get; set; } = 60f * 60f * 24f * 3f;
        public float ServerTime { get; set; } = 3f;
        public float DebugTime { get; set; } = 10f * 60f;
        public int MaxNameLenght { get; set; } = 16;

        public Config(string[] args)
        {

        }
    }
}
