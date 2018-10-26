using LiteNetLib.Utils;

namespace Packets
{
    public class ClientInput
    {
        public float MoveX { get; set; }
        public float MoveY { get; set; }
        public float ViewRange { get; set; }
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public bool Rise { get; set; }
    }
}
