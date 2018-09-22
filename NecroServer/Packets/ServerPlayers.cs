using LiteNetLib.Utils;

namespace Packets
{
    public class ServerPlayers
    {
        public float WaitTime { get; set; }
        public PlayerInfo[] Players { get; set; }
    }
}
