using LiteNetLib.Utils;

namespace Packets
{
    public struct UnitInfo : INetSerializable
    {
        public ushort UnitId { get; set; }
        public byte UnitMesh { get; set; }
        public short PosX { get; set; }
        public short PosY { get; set; }
        public byte Rot { get; set; }
        public byte Health { get; set; }
        public RuneType Rune { get; set; }
        public bool Attack { get; set; }
        public bool PlayerOwned { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UnitId = reader.GetUShort();
            UnitMesh = reader.GetByte();
            PosX = reader.GetShort();
            PosY = reader.GetShort();
            Rot = reader.GetByte();
            Health = reader.GetByte();
            Rune = (RuneType)reader.GetByte();
            byte attackAndPlayer = reader.GetByte();
            Attack = (attackAndPlayer & 1) > 0;
            PlayerOwned = (attackAndPlayer & 2) > 0;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UnitId);
            writer.Put(UnitMesh);
            writer.Put(PosX);
            writer.Put(PosY);
            writer.Put(Rot);
            writer.Put(Health);
            writer.Put((byte)Rune);
            byte attackAndPlayer = (byte)((Attack ? 1 : 0) + (PlayerOwned ? 2 : 0));
            writer.Put(attackAndPlayer);
        }
    }
}
