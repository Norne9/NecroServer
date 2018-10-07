using LiteNetLib.Utils;

namespace Packets
{
    public class ServerMap
    {
        public float Scale { get; set; }
        public int MaxPlayers { get; set; }
        public int MaxUnits { get; set; }
        public int MapType { get; set; }
        public ObstacleInfo[] Obstacles { get; set; }
        public UnitInfo[] Units { get; set; }
        public RuneInfo[] Runes { get; set; }
    }
}
