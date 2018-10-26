using System;

namespace Packets
{
    public class UnitFrame
    {
        public static uint UnitsPacketId { get; set; } = 0;

        public uint PacketId { get; set; }
        public UnitInfo[] Units { get; set; }
    }
}
