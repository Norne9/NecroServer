using Game;
using LiteNetLib.Utils;

namespace Packets
{
    public class ServerFrame
    {
        public WorldState State { get; set; }
        public float PlayTime { get; set; }
        public float ZoneSize { get; set; }
        public int AlivePlayers { get; set; }
        public int UnitKill { get; set; }
        public int PlayerUnitCount { get; set; }
        public float Cooldown { get; set; }
        public VisualEffect VisualEffect { get; set; }
        public float VisualEffectTime { get; set; }
        public PlayerCameraInfo[] PlayerCameras { get; set; }
        public RuneInfo[] Runes { get; set; }
        public int[] EnabledUnits { get; set; }
    }
}
