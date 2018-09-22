using LiteNetLib.Utils;

namespace Packets
{
    public class ServerFrame
    {
        public State State { get; set; }
        public float PlayTime { get; set; }
        public float ZoneSize { get; set; }
        public int AlivePlayers { get; set; }
        public PlayerCameraInfo[] PlayerCameras { get; set; }
        public UnitInfo[] Units { get; set; }
        public RuneInfo[] Runes { get; set; }
    }
}
