﻿using LiteNetLib.Utils;
using Game;

namespace Packets
{
    public class ServerMap
    {
        public GameMode GameMode { get; set; }
        public float Scale { get; set; }
        public int MaxPlayers { get; set; }
        public int MaxUnits { get; set; }
        public int MapType { get; set; }
        public int Color1 { get; set; } = 0;
        public int Color2 { get; set; } = 0;
        public float RiseRadius { get; set; }
        public ObstacleInfo[] Obstacles { get; set; }
        public UnitInfo[] Units { get; set; }
        public RuneInfo[] Runes { get; set; }
    }
}
