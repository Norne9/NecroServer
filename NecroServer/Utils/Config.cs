using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public class Config
    {
        public int Port { get; set; } = 15364;

        public int MaxUnitCount { get; set; } = 9;
        public int MaxPlayers { get; set; } = 20;
        public int UpdateDelay { get; set; } = 30;

        public float PlayerWaitTime { get; set; } = 45f;
        public float EndWaitTime { get; set; } = 15f;

        public string DiscordLog { get; set; } = null;
        public string ConnectionKey { get; set; } = "debug";

        public string MasterServer { get; set; } = "http://127.0.0.1:8856/";
        public int MasterUpdateDelay { get; set; } = 2000;

        public int UnitCount { get; set; } = 150;
        public int RuneCount { get; set; } = 8;
        public int ObstacleCount { get; set; } = 200;

        public int MaxWorldScale { get; set; } = 80;
        public int MinWorldScale { get; set; } = 60;

        public float RuneTime { get; set; } = 20f;

        public float UnitRange { get; set; } = 0.95f;
        public float ObstacleRange { get; set; } = 1.1f;
        public float RuneRange { get; set; } = 0.5f;
        public float ViewRange { get; set; } = 20f;

        public float RiseRadius { get; set; } = 6f;
        public float RiseCooldown { get; set; } = 12f;

        public float HealValue { get; set; } = 0.2f;
        public float RandomDamage { get; set; } = 0.1f;

        public float ZoneDps { get; set; } = 15f;

        public float InputDeadzone { get; set; } = 0.1f;

        public float StaticTime { get; set; } = 120f;
        public float ResizeTime { get; set; } = 160f;

        public Config(string[] args)
        {

        }

        public void AppendArgs(string[] args)
        {

        }
    }
}
