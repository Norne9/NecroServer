using LiteNetLib.Utils;

namespace Packets
{
    public struct PlayerInfo : INetSerializable
    {
        public long UserId { get; set; }
        public string UserName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetLong();
            UserName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(UserName);
        }
    }
}
