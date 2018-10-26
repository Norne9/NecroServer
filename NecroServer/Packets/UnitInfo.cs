using LiteNetLib.Utils;
using Game;

namespace Packets
{
    public struct UnitInfo : INetSerializable
    {
        public ushort UnitId { get; set; }
        public byte UnitMesh { get; set; }
        public short PosX { get; set; }
        public short PosY { get; set; }

        public bool Attack { get; set; }
        public bool PlayerOwned { get; set; }
        public bool HasEffect { get; set; }
        public bool HasHealth { get; set; }
        public bool Alive { get; set; }
        public bool HasExp { get; set; }
        public bool Upgrade { get; set; }
        public bool CustomSize { get; set; }

        public byte Health { get; set; }
        public byte Rot { get; set; }
        public VisualEffect VisualEffect { get; set; }
        public ushort Target { get; set; }
        public byte Exp { get; set; }
        public byte UnitSkin { get; set; }
        public byte UnitSize { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UnitId);
            writer.Put(UnitMesh);
            writer.Put(PosX);
            writer.Put(PosY);

            writer.Put(BoolToByte(
                Attack,
                PlayerOwned,
                HasEffect,
                HasHealth,
                Alive,
                HasExp,
                Upgrade,
                CustomSize
            ));

            if (Alive && HasHealth) writer.Put(Health);
            if (Alive)              writer.Put(Rot);
            if (HasEffect)          writer.Put((byte)VisualEffect);
            if (Attack)             writer.Put(Target);
            if (HasExp)             writer.Put(Exp);
            if (Upgrade)            writer.Put(UnitSkin);
            if (CustomSize)         writer.Put(UnitSize);
        }

        public void Deserialize(NetDataReader reader)
        {
            UnitId = reader.GetUShort();
            UnitMesh = reader.GetByte();
            PosX = reader.GetShort();
            PosY = reader.GetShort();

            bool[] bdata = ByteToBool(reader.GetByte());
            Attack = bdata[0];
            PlayerOwned = bdata[1];
            HasEffect = bdata[2];
            HasHealth = bdata[3];
            Alive = bdata[4];
            HasExp = bdata[5];
            Upgrade = bdata[6];
            CustomSize = bdata[7];

            if (Alive && HasHealth)
                Health = reader.GetByte();
            if (Alive)
                Rot = reader.GetByte();
            if (HasEffect)
                VisualEffect = (VisualEffect)reader.GetByte();
            if (Attack)
                Target = reader.GetUShort();
            if (HasExp)
                Exp = reader.GetByte();
            if (Upgrade)
                UnitSkin = reader.GetByte();
            if (CustomSize)
                UnitSize = reader.GetByte();
        }

        private static byte BoolToByte(params bool[] args)
        {
            byte result = 0;
            for (int i = 0; i < args.Length; i++)
                if (args[i]) result |= (byte)(1 << i);
            return result;
        }

        private static bool[] ByteToBool(byte data)
        {
            var result = new bool[8];
            for (int i = 0; i < result.Length; i++)
                result[i] = (data & (byte)(1 << i)) > 0;
            return result;
        }
    }
}
