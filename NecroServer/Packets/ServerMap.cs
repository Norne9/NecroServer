using LiteNetLib.Utils;

namespace Packets
{
    public class ServerMap
    {
        public float Scale { get; set; }
        public int MaxPlayers { get; set; }
        public int MapType { get; set; }
        public ObstacleInfo[] Obstacles { get; set; }
    }
}
