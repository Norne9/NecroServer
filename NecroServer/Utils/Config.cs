using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public class Config
    {
        public int Port { get; set; } = 15364;

        public int MaxUnitCount { get; set; } = 16;
        public int MaxPlayers { get; set; } = 20;
        public int UpdateTime { get; set; } = 10;

        public string DiscordLog { get; set; } = null;
        public string ConnectionKey { get; set; } = "debug";

        public int UnitCount { get; set; } = 300;
        public int RuneCount { get; set; } = 8;
        public int ObstacleCount { get; set; } = 400;

        public int MaxWorldScale { get; set; } = 200;
        public int MinWorldScale { get; set; } = 140;

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
    }
}
