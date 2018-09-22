using LiteNetLib.Utils;

namespace Packets
{
    public struct PlayerCameraInfo : INetSerializable
    {
        public long UserId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetLong();
            PosX = reader.GetFloat();
            PosY = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(PosX);
            writer.Put(PosY);
        }
    }
}
