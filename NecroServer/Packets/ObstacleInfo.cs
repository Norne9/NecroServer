using LiteNetLib.Utils;

namespace Packets
{
    public struct ObstacleInfo : INetSerializable
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PosX = reader.GetFloat();
            PosY = reader.GetFloat();
            Scale = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PosX);
            writer.Put(PosY);
            writer.Put(Scale);
        }
    }
}
