﻿using LiteNetLib.Utils;
using Game;

namespace Packets
{
    public struct RuneInfo : INetSerializable
    {
        public VisualEffect Rune { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Rune = (VisualEffect)reader.GetByte();
            PosX = reader.GetFloat();
            PosY = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Rune);
            writer.Put(PosX);
            writer.Put(PosY);
        }
    }
}
