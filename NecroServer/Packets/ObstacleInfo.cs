using LiteNetLib.Utils;

namespace Packets
{
    public struct ObstacleInfo : INetSerializable
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; }
        public byte Mesh { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PosX = reader.GetFloat();
            PosY = reader.GetFloat();
            Scale = reader.GetFloat();
            Mesh = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PosX);
            writer.Put(PosY);
            writer.Put(Scale);
            writer.Put(Mesh);
        }
    }
}
