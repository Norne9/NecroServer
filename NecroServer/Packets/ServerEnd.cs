using LiteNetLib.Utils;

namespace Packets
{
    public class ServerEnd
    {
        public float AliveTime { get; set; }
        public int Place { get; set; }
        public int Rating { get; set; }
        public float DamageDeal { get; set; }
        public float DamageReceive { get; set; }
        public int UnitRise { get; set; }
        public int UnitKill { get; set; }
    }
}
